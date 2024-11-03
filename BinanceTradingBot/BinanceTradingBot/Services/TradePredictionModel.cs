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
    private ITransformer _linearRegressionModel;
    private ITransformer _decisionTreeModel;
    private ITransformer _neuralNetworkModel;
    private ITransformer _randomForestModel;
    private ITransformer _gradientBoostingModel;
    private BinanceRestClient _client;
    private string _symbol;

    private readonly string[] _modelFiles = {
        "LinearRegressionModel.zip",
        "DecisionTreeModel.zip",
        "NeuralNetworkModel.zip",
        "RandomForestModel.zip",
        "GradientBoostingModel.zip"
    };

    public TradePredictionModel()
    {
        _mlContext = new MLContext();
    }

    public async Task InitializeOrTrainModelAsync(BinanceRestClient client, string symbol)
    {
        _client = client;
        _symbol = symbol;

        if (ModelsExist())
        {
            LoadModels();
        }
        else
        {
            var trainingData = await LoadTrainingDataAsync();
            if (trainingData != null)
            {
                TrainAndSaveModels(trainingData);
            }
            else
            {
                Console.WriteLine("No valid training data available.");
            }
        }
    }

    private bool ModelsExist()
    {
        return _modelFiles.All(File.Exists);
    }

    private void LoadModels()
    {
        _linearRegressionModel = _mlContext.Model.Load("LinearRegressionModel.zip", out var _);
        _decisionTreeModel = _mlContext.Model.Load("DecisionTreeModel.zip", out var _);
        _neuralNetworkModel = _mlContext.Model.Load("NeuralNetworkModel.zip", out var _);
        _randomForestModel = _mlContext.Model.Load("RandomForestModel.zip", out var _);
        _gradientBoostingModel = _mlContext.Model.Load("GradientBoostingModel.zip", out var _);
        Console.WriteLine("Models loaded successfully.");
    }

    private async Task<IDataView> LoadTrainingDataAsync()
    {
        var multiTimeframeData = await LoadMultiTimeframeData();
        return PrepareTrainingData(multiTimeframeData);
    }

    private IDataView PrepareTrainingData(MultiTimeframeData multiTimeframeData)
    {
        var allTradeData = new List<TradeData>();

        foreach (var property in typeof(MultiTimeframeData).GetProperties())
        {
            if (property.PropertyType == typeof(List<TradeData>))
            {
                var tradeDataList = property.GetValue(multiTimeframeData) as List<TradeData>;
                if (tradeDataList != null && tradeDataList.Any())
                {
                    allTradeData.AddRange(tradeDataList);
                }
            }
        }

        if (!allTradeData.Any())
        {
            Console.WriteLine("No training data available.");
            return null;
        }

        return _mlContext.Data.LoadFromEnumerable(allTradeData);
    }

    public async Task<MultiTimeframeData> LoadMultiTimeframeData()
    {
        var indicatorsService = new TechnicalIndicatorsService();

        var klines1d = await _client.GetKlinesAsync(_symbol, "1d", 30);
        var klines4h = await _client.GetKlinesAsync(_symbol, "4h", 30);
        var klines5m = await _client.GetKlinesAsync(_symbol, "5m", 30);
        var klines1m = await _client.GetKlinesAsync(_symbol, "1m", 30);

        var dailyPrices = klines1d.Select(k => k.Close).ToList();
        var fourHourPrices = klines4h.Select(k => k.Close).ToList();
        var fiveMinutePrices = klines5m.Select(k => k.Close).ToList();
        var oneMinutePrices = klines1m.Select(k => k.Close).ToList();

        var multiTimeframeData = new MultiTimeframeData
        {
            DailyData = klines1d.Select(k => new TradeData
            {
                Price = k.Close,
                MovingAverage = indicatorsService.CalculateMovingAverage(dailyPrices, 14),
                MacdSignal = indicatorsService.CalculateMacdSignal(dailyPrices),
                Rsi = indicatorsService.CalculateRsi(dailyPrices),
                Volume = k.Volume,
                Trend = k.Close > k.Open ? "long" : "short",
                Symbol = _symbol
            }).ToList(),

            FourHourData = klines4h.Select(k => new TradeData
            {
                Price = k.Close,
                MovingAverage = indicatorsService.CalculateMovingAverage(fourHourPrices, 14),
                MacdSignal = indicatorsService.CalculateMacdSignal(fourHourPrices),
                Rsi = indicatorsService.CalculateRsi(fourHourPrices),
                Volume = k.Volume,
                Trend = k.Close > k.Open ? "long" : "short",
                Symbol = _symbol
            }).ToList(),

            FiveMinuteData = klines5m.Select(k => new TradeData
            {
                Price = k.Close,
                MovingAverage = indicatorsService.CalculateMovingAverage(fiveMinutePrices, 14),
                MacdSignal = indicatorsService.CalculateMacdSignal(fiveMinutePrices),
                Rsi = indicatorsService.CalculateRsi(fiveMinutePrices),
                Volume = k.Volume,
                Trend = k.Close > k.Open ? "long" : "short",
                Symbol = _symbol
            }).ToList(),

            OneMinuteData = klines1m.Select(k => new TradeData
            {
                Price = k.Close,
                MovingAverage = indicatorsService.CalculateMovingAverage(oneMinutePrices, 14),
                MacdSignal = indicatorsService.CalculateMacdSignal(oneMinutePrices),
                Rsi = indicatorsService.CalculateRsi(oneMinutePrices),
                Volume = k.Volume,
                Trend = k.Close > k.Open ? "long" : "short",
                Symbol = _symbol
            }).ToList()
        };

        return multiTimeframeData;
    }

    public async Task TrainModelWithNewDataAsync()
    {
        var newTrainingData = await LoadTrainingDataAsync();

        if (newTrainingData != null)
        {
            TrainAndSaveModels(newTrainingData);
            Console.WriteLine("Models are successfully updated with new data.");
        }
        else
        {
            Console.WriteLine("Failed to load training data.");
        }
    }

    private void TrainAndSaveModels(IDataView dataView)
    {
        var linearRegressionPipeline = _mlContext.Transforms.Concatenate("Features", nameof(TradeData.Price), nameof(TradeData.MacdSignal), nameof(TradeData.MovingAverage), nameof(TradeData.Rsi), nameof(TradeData.Volume))
            .Append(_mlContext.Regression.Trainers.Sdca());
        _linearRegressionModel = linearRegressionPipeline.Fit(dataView);
        _mlContext.Model.Save(_linearRegressionModel, dataView.Schema, "LinearRegressionModel.zip");

        var decisionTreePipeline = _mlContext.Transforms.Concatenate("Features", nameof(TradeData.Price), nameof(TradeData.MacdSignal), nameof(TradeData.MovingAverage), nameof(TradeData.Rsi), nameof(TradeData.Volume))
            .Append(_mlContext.Regression.Trainers.FastTree());
        _decisionTreeModel = decisionTreePipeline.Fit(dataView);
        _mlContext.Model.Save(_decisionTreeModel, dataView.Schema, "DecisionTreeModel.zip");

        var neuralNetworkPipeline = _mlContext.Transforms.Concatenate("Features", nameof(TradeData.Price), nameof(TradeData.MacdSignal), nameof(TradeData.MovingAverage), nameof(TradeData.Rsi), nameof(TradeData.Volume))
            .Append(_mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy());
        _neuralNetworkModel = neuralNetworkPipeline.Fit(dataView);
        _mlContext.Model.Save(_neuralNetworkModel, dataView.Schema, "NeuralNetworkModel.zip");

        var randomForestPipeline = _mlContext.Transforms.Concatenate("Features", nameof(TradeData.Price), nameof(TradeData.MacdSignal), nameof(TradeData.MovingAverage), nameof(TradeData.Rsi), nameof(TradeData.Volume))
            .Append(_mlContext.Regression.Trainers.FastForest());
        _randomForestModel = randomForestPipeline.Fit(dataView);
        _mlContext.Model.Save(_randomForestModel, dataView.Schema, "RandomForestModel.zip");

        var gradientBoostingPipeline = _mlContext.Transforms.Concatenate("Features", nameof(TradeData.Price), nameof(TradeData.MacdSignal), nameof(TradeData.MovingAverage), nameof(TradeData.Rsi), nameof(TradeData.Volume))
            .Append(_mlContext.Regression.Trainers.LightGbm());
        _gradientBoostingModel = gradientBoostingPipeline.Fit(dataView);
        _mlContext.Model.Save(_gradientBoostingModel, dataView.Schema, "GradientBoostingModel.zip");

        Console.WriteLine("All models trained and saved.");
    }

    public string Predict(TradeData input)
    {
        var linearRegressionPrediction = _mlContext.Model.CreatePredictionEngine<TradeData, TradePrediction>(_linearRegressionModel).Predict(input);
        var decisionTreePrediction = _mlContext.Model.CreatePredictionEngine<TradeData, TradePrediction>(_decisionTreeModel).Predict(input);
        var neuralNetworkPrediction = _mlContext.Model.CreatePredictionEngine<TradeData, TradePrediction>(_neuralNetworkModel).Predict(input);
        var randomForestPrediction = _mlContext.Model.CreatePredictionEngine<TradeData, TradePrediction>(_randomForestModel).Predict(input);
        var gradientBoostingPrediction = _mlContext.Model.CreatePredictionEngine<TradeData, TradePrediction>(_gradientBoostingModel).Predict(input);

        var weights = new Dictionary<string, double>
        {
            { linearRegressionPrediction.Trend, 1.0 },
            { decisionTreePrediction.Trend, 1.5 },
            { neuralNetworkPrediction.Trend, 2.0 },
            { randomForestPrediction.Trend, 1.5 },
            { gradientBoostingPrediction.Trend, 2.0 }
        };

        var trend = weights.GroupBy(x => x.Key)
                            .Select(group => new { Trend = group.Key, Weight = group.Sum(w => w.Value) })
                            .OrderByDescending(g => g.Weight)
                            .First()
                            .Trend;

        return trend;
    }
}
