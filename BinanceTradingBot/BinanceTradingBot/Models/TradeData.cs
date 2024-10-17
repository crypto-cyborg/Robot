namespace BinanceTradingBot.Models;

public class TradeData
{
    public decimal Price { get; set; }
    public decimal MovingAverage { get; set; }
    public decimal Macd { get; set; }
    public decimal Signal { get; set; }
    public string Trend { get; set; } 
}
