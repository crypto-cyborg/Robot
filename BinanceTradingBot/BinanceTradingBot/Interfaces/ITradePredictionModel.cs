using BinanceTradingBot.Models;

namespace BinanceTradingBot.Interfaces;

public interface ITradePredictionModel
{
    string Predict(TradeData input);
    void LoadOrTrainModel(MultiTimeframeData multiTimeframeData);
}
