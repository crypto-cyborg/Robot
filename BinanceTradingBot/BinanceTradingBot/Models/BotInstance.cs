using BinanceTradingBot.Interfaces;
using BinanceTradingBot.Services;

namespace BinanceTradingBot.Models;

public class BotInstance
{
    public BinanceRestClient Client { get; set; }
    public ITradePredictionModel? PredictionModel { get; set; }
    public CancellationTokenSource? CancellationTokenSource { get; set; }
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
    public string Symbol { get; set; }
    public decimal TradeAmount { get; set; }
    public int Leverage { get; set; } 
    public decimal? PreviousStopLoss { get; set; } 
    public decimal? PurchasePrice { get; set; }  
}
