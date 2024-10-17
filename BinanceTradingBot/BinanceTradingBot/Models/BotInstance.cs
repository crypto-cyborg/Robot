using BinanceTradingBot.Interfaces;
using BinanceTradingBot.Services;

namespace BinanceTradingBot.Models;

public class BotInstance
{
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
    public string Symbol { get; set; }
    public BinanceRestClient Client { get; set; }
    public ITradePredictionModel PredictionModel { get; set; }
    public decimal TradeAmount { get; set; }
    public int Leverage { get; set; }
    public CancellationTokenSource CancellationTokenSource { get; set; }
}
