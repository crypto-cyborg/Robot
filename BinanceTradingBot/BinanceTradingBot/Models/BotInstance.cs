using BinanceTradingBot.Interfaces;
using BinanceTradingBot.Services;

namespace BinanceTradingBot.Models;

public class BotInstance
{
    public BinanceRestClient Client { get; set; }
    public SandboxClient SandboxClient { get; set; }
    public ITradePredictionModel? PredictionModel { get; set; }
    public CancellationTokenSource? CancellationTokenSource { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public Guid? WalletId { get; set; }
    public string Symbol { get; set; }
    public float TradeAmount { get; set; }
    public int? Leverage { get; set; } 
    public float? PreviousStopLoss { get; set; } 
    public float? PurchasePrice { get; set; }

}
