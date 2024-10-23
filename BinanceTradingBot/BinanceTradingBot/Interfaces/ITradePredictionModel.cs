using BinanceTradingBot.Models;
using BinanceTradingBot.Services;

namespace BinanceTradingBot.Interfaces;

public interface ITradePredictionModel
{
    Task InitializeOrTrainModelAsync(BinanceRestClient client, string symbol);
    Task TrainModelWithNewDataAsync(BinanceRestClient client, string symbol);
    string Predict(TradeData input);
}
