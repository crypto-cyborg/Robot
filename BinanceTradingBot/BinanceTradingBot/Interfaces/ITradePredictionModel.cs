using BinanceTradingBot.Models;
using BinanceTradingBot.Services;
using CryptoExchange.Net.CommonObjects;
using Microsoft.ML;

namespace BinanceTradingBot.Interfaces;

public interface ITradePredictionModel
{
    Task InitializeOrTrainModelAsync(BinanceRestClient client, string symbol);
    Task<MultiTimeframeData> LoadMultiTimeframeDataAsync();
    Task TrainModelWithNewDataAsync();
    string Predict(TradeData input);
}
