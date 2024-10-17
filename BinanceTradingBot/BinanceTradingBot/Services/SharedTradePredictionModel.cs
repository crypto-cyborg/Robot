using BinanceTradingBot.Interfaces;
using BinanceTradingBot.Models;
using Microsoft.ML;

namespace BinanceTradingBot.Services
{
    public class SharedTradePredictionModel : ITradePredictionModel
    {
        private readonly MLContext _mlContext;
        private ITransformer _model;

        public SharedTradePredictionModel()
        {
            _mlContext = new MLContext();
            LoadOrTrainModel();
        }

        public string Predict(TradeData input)
        {
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<TradeData, TradePrediction>(_model);
            var prediction = predictionEngine.Predict(input);
            return prediction.Trend ?? "neutral";
        }

        private void LoadOrTrainModel()
        {
            var modelPath = "SharedModel.zip";

            if (System.IO.File.Exists(modelPath))
            {
                _model = _mlContext.Model.Load(modelPath, out _);
                System.Console.WriteLine("Model loaded successfully.");
            }
            else
            {
                var data = LoadTrainingData();
                var dataView = _mlContext.Data.LoadFromEnumerable(data);

                var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(TradeData.Trend))
                    .Append(_mlContext.Transforms.Concatenate("Features", nameof(TradeData.Price), nameof(TradeData.MovingAverage), nameof(TradeData.Macd), nameof(TradeData.Signal)))
                    .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                    .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

                _model = pipeline.Fit(dataView);
                _mlContext.Model.Save(_model, dataView.Schema, modelPath);
                System.Console.WriteLine("Model trained and saved successfully.");
            }
        }

        private IEnumerable<TradeData> LoadTrainingData()
        {
            return new List<TradeData>
            {
                new TradeData { Price = 45000, MovingAverage = 44000, Macd = 1.5m, Signal = 1.2m, Trend = "long" },
                new TradeData { Price = 46000, MovingAverage = 45000, Macd = 1.8m, Signal = 1.4m, Trend = "long" },
                new TradeData { Price = 44000, MovingAverage = 43000, Macd = -1.0m, Signal = -0.8m, Trend = "short" }
            };
        }
    }
}
