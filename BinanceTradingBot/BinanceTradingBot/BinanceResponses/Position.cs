namespace BinanceTradingBot.BinanceResponses;

public class Position
{
    public string Symbol { get; set; }             // "symbol": "ADAUSDT"
    public string PositionSide { get; set; }       // "positionSide": "BOTH"
    public decimal PositionAmt { get; set; }       // "positionAmt": "30"
    public decimal EntryPrice { get; set; }        // "entryPrice": "0.385"
    public decimal BreakEvenPrice { get; set; }    // "breakEvenPrice": "0.385077"
    public decimal MarkPrice { get; set; }         // "markPrice": "0.41047590"
    public decimal UnRealizedProfit { get; set; }  // "unRealizedProfit": "0.76427700"
    public decimal LiquidationPrice { get; set; }  // "liquidationPrice": "0"
    public decimal IsolatedMargin { get; set; }    // "isolatedMargin": "0"
    public decimal Notional { get; set; }          // "notional": "12.31427700"
    public string MarginAsset { get; set; }        // "marginAsset": "USDT"
    public decimal IsolatedWallet { get; set; }    // "isolatedWallet": "0"
    public decimal InitialMargin { get; set; }     // "initialMargin": "0.61571385"
    public decimal MaintMargin { get; set; }       // "maintMargin": "0.08004280"
    public decimal PositionInitialMargin { get; set; } // "positionInitialMargin": "0.61571385"
    public decimal OpenOrderInitialMargin { get; set; } // "openOrderInitialMargin": "0"
    public int Adl { get; set; }                   // "adl": 2
    public decimal BidNotional { get; set; }       // "bidNotional": "0"
    public decimal AskNotional { get; set; }       // "askNotional": "0"
    public long UpdateTime { get; set; }           // "updateTime": 1720736417660
}

