using BinanceTradingBot.BinanceResponses;
using BinanceTradingBot.Interfaces;
using BinanceTradingBot.Models;
using BinanceTradingBot.Services;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class TradePredictionModel : ITradePredictionModel
{
    private readonly MLContext _mlContext;
    private ITransformer _gradientBoostingModel;
    private PredictionEngine<TradeData, TradePrediction> _predictionEngine;
    private BinanceRestClient _client;
    private string _symbol;
    private string _modelFile;

    public TradePredictionModel()
    {
        _mlContext = new MLContext();
    }

    public async Task InitializeOrTrainModelAsync(BinanceRestClient client, string symbol)
    {
        _client = client;
        _symbol = symbol;
        _modelFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", $"{_symbol}GradientBoostingModel.zip");
        Console.WriteLine($"Path: {_modelFile}");

        if (ModelExists())
        {
            LoadModel();
        }
        else
        {
            var trainingData = await LoadTrainingDataAsync();
            if (trainingData != null)
            {
                TrainAndSaveModel(trainingData);
            }
            else
            {
                Console.WriteLine("No valid training data available.");
            }
        }
    }

    private bool ModelExists()
    {
        return File.Exists(_modelFile);
    }

    private void LoadModel()
    {
        _gradientBoostingModel = _mlContext.Model.Load(_modelFile, out var _);
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<TradeData, TradePrediction>(_gradientBoostingModel);
        Console.WriteLine("Model loaded successfully.");
    }

    private async Task<IDataView> LoadTrainingDataAsync()
    {
        var multiTimeframeData = await LoadMultiTimeframeDataAsync();
        return PrepareTrainingData(multiTimeframeData);
    }

    private IDataView PrepareTrainingData(MultiTimeframeData multiTimeframeData)
    {
        var allTradeData = new List<TradeData>();

        foreach (var property in typeof(MultiTimeframeData).GetProperties())
        {
            var tradeDataList = property.GetValue(multiTimeframeData) as List<TradeData>;
            if (tradeDataList != null && tradeDataList.Any())
            {
                allTradeData.AddRange(tradeDataList);
            }
        }

        if (!allTradeData.Any())
        {
            Console.WriteLine("No training data available.");
            return null;
        }

        return _mlContext.Data.LoadFromEnumerable(allTradeData);
    }

    public async Task<MultiTimeframeData> LoadMultiTimeframeDataAsync()
    {
        var indicatorsService = new TechnicalIndicatorsService();

        var klinesTasks = new[]
        {
            _client.GetKlinesAsync(_symbol, "1d", 100),
            _client.GetKlinesAsync(_symbol, "4h", 100),
            _client.GetKlinesAsync(_symbol, "5m", 100),
            _client.GetKlinesAsync(_symbol, "1m", 100)
        };

        var klines = await Task.WhenAll(klinesTasks);

        if (klines.Any(result => result == null || !result.Any()))
        {
            Console.WriteLine("Error loading kline data.");
            return null;
        }

        var dailyPrices = klines[0].Select(k => k.Close).ToList();
        var fourHourPrices = klines[1].Select(k => k.Close).ToList();
        var fiveMinutePrices = klines[2].Select(k => k.Close).ToList();
        var oneMinutePrices = klines[3].Select(k => k.Close).ToList();

        return new MultiTimeframeData
        {
            DailyData = CreateTradeData(klines[0], dailyPrices, "1d", indicatorsService),
            FourHourData = CreateTradeData(klines[1], fourHourPrices, "4h", indicatorsService),
            FiveMinuteData = CreateTradeData(klines[2], fiveMinutePrices, "5m", indicatorsService),
            OneMinuteData = CreateTradeData(klines[3], oneMinutePrices, "1m", indicatorsService)
        };
    }

    public List<TradeData> CreateTradeData(IEnumerable<Kline> klines, List<float> prices, string timeframe, TechnicalIndicatorsService indicatorsService)
    {
        var timeframeMapping = new Dictionary<string, float>
    {
        { "1m", 1 },
        { "5m", 5 },
        { "4h", 240 },
        { "1d", 1440 }
    };

        if (!timeframeMapping.ContainsKey(timeframe))
        {
            Console.WriteLine($"Unknown timeframe: {timeframe}");
            return new List<TradeData>();
        }

        var timeframeValue = timeframeMapping[timeframe];

        return klines.Select(k => new TradeData
        {
            Price = k.Close,
            MovingAverage = indicatorsService.CalculateMovingAverage(prices, 14),
            MacdSignal = indicatorsService.CalculateMacdSignal(prices),
            Rsi = indicatorsService.CalculateRsi(prices),
            Volume = k.Volume,
            Trend = k.Close > k.Open ? 1f : 0f,
            Timeframe = timeframeValue 
        }).ToList();
    }


    public async Task TrainModelWithNewDataAsync()
    {
        var newTrainingData = await LoadTrainingDataAsync();

        if (newTrainingData != null)
        {
            TrainAndSaveModel(newTrainingData);
            Console.WriteLine("Model successfully updated with new data.");
        }
        else
        {
            Console.WriteLine("Failed to load training data.");
        }
    }

    private void TrainAndSaveModel(IDataView dataView)
    {
        if (dataView == null || !_mlContext.Data.CreateEnumerable<TradeData>(dataView, reuseRowObject: false).Any())
        {
            Console.WriteLine("Training data is empty, model training aborted.");
            return;
        }

        var gradientBoostingPipeline = _mlContext.Transforms.CopyColumns("Label", nameof(TradeData.Trend)) 
            .Append(_mlContext.Transforms.Concatenate("Features",
                nameof(TradeData.Price),
                nameof(TradeData.MacdSignal),
                nameof(TradeData.MovingAverage),
                nameof(TradeData.Rsi),
                nameof(TradeData.Volume)))
            .Append(_mlContext.Regression.Trainers.LightGbm());

        _gradientBoostingModel = gradientBoostingPipeline.Fit(dataView);

        var directory = Path.GetDirectoryName(_modelFile);
        if (directory != null)
        {
            Directory.CreateDirectory(directory);
        }

        _mlContext.Model.Save(_gradientBoostingModel, dataView.Schema, _modelFile);

        Console.WriteLine("Model trained and saved.");
    }


    public string Predict(TradeData input)
    {
        if (_predictionEngine == null)
        {
            Console.WriteLine("Prediction engine is not initialized.");
            return "unknown";
        }

        var prediction = _predictionEngine.Predict(input);
        return prediction.Trend > 0.5 ? "long" : "short";
    }
}
