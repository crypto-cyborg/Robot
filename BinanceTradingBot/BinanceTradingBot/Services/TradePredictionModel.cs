using BinanceTradingBot.Interfaces;
using BinanceTradingBot.Models;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BinanceTradingBot.Services
{
    public class TradePredictionModel : ITradePredictionModel
    {
        private readonly MLContext _mlContext;
        private ITransformer _linearRegressionModel;
        private ITransformer _decisionTreeModel;
        private ITransformer _neuralNetworkModel;

        public TradePredictionModel()
        {
            _mlContext = new MLContext();
            LoadOrTrainModels();
        }

        private void LoadOrTrainModels()
        {
            if (System.IO.File.Exists("LinearRegressionModel.zip"))
            {
                _linearRegressionModel = _mlContext.Model.Load("LinearRegressionModel.zip", out var _);
                _decisionTreeModel = _mlContext.Model.Load("DecisionTreeModel.zip", out var _);
                _neuralNetworkModel = _mlContext.Model.Load("NeuralNetworkModel.zip", out var _);
                Console.WriteLine("Models loaded successfully.");
            }
            else
            {
                Console.WriteLine("No models found. Please train the models with training data.");
            }
        }

        public void LoadOrTrainModel(MultiTimeframeData multiTimeframeData)
        {
            var dataView = LoadTrainingData(multiTimeframeData);

            if (dataView != null)
            {
                TrainAndSaveModels(dataView);
            }
            else
            {
                Console.WriteLine("No training data provided.");
            }
        }
        
        
        private IDataView LoadTrainingData(MultiTimeframeData multiTimeframeData)
        {
            try
            {
                var allTradeData = new List<TradeData>();
                
                foreach (var property in typeof(MultiTimeframeData).GetProperties())
                {
                    if (property.PropertyType == typeof(List<TradeData>))
                    {
                        var res = property.GetValue(multiTimeframeData) as List<TradeData>;

                        if (res != null && res.Any())
                        {
                            allTradeData.AddRange(res); 
                        }
                    }
                }

                if (!allTradeData.Any())
                {
                    Console.WriteLine("No training data available.");
                    return _mlContext.Data.LoadFromEnumerable(new List<TradeData>()); 
                }

                return _mlContext.Data.LoadFromEnumerable(allTradeData);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to load training data: {e.Message}");
            }
        }

        private void TrainAndSaveModels(IDataView dataView)
        {
            
            var linearRegressionPipeline = _mlContext.Transforms.Concatenate("Features", nameof(TradeData.Price), nameof(TradeData.Macd), nameof(TradeData.Signal))
                .Append(_mlContext.Regression.Trainers.Sdca());
            _linearRegressionModel = linearRegressionPipeline.Fit(dataView);
            _mlContext.Model.Save(_linearRegressionModel, dataView.Schema, "LinearRegressionModel.zip");

            
            var decisionTreePipeline = _mlContext.Transforms.Concatenate("Features", nameof(TradeData.Price), nameof(TradeData.Macd), nameof(TradeData.Signal))
                .Append(_mlContext.Regression.Trainers.FastTree());
            _decisionTreeModel = decisionTreePipeline.Fit(dataView);
            _mlContext.Model.Save(_decisionTreeModel, dataView.Schema, "DecisionTreeModel.zip");

            
            var neuralNetworkPipeline = _mlContext.Transforms.Concatenate("Features", nameof(TradeData.Price), nameof(TradeData.Macd), nameof(TradeData.Signal))
                .Append(_mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy());
            _neuralNetworkModel = neuralNetworkPipeline.Fit(dataView);
            _mlContext.Model.Save(_neuralNetworkModel, dataView.Schema, "NeuralNetworkModel.zip");

            Console.WriteLine("Models trained and saved.");
        }

        public string Predict(TradeData input)
        {
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<TradeData, TradePrediction>(_linearRegressionModel);
            var linearRegressionPrediction = predictionEngine.Predict(input);

            predictionEngine = _mlContext.Model.CreatePredictionEngine<TradeData, TradePrediction>(_decisionTreeModel);
            var decisionTreePrediction = predictionEngine.Predict(input);

            predictionEngine = _mlContext.Model.CreatePredictionEngine<TradeData, TradePrediction>(_neuralNetworkModel);
            var neuralNetworkPrediction = predictionEngine.Predict(input);

            if (linearRegressionPrediction.Trend == decisionTreePrediction.Trend)
            {
                return linearRegressionPrediction.Trend;
            }
            else if (neuralNetworkPrediction.Trend == decisionTreePrediction.Trend)
            {
                return neuralNetworkPrediction.Trend;
            }
            else if (neuralNetworkPrediction.Trend == decisionTreePrediction.Trend)
            {
                return decisionTreePrediction.Trend;
            }

            return "neutral";
        }
    }
}
