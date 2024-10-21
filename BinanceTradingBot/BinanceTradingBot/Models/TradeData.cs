namespace BinanceTradingBot.Models;

public class TradeData
{
    public decimal Price { get; set; }
    public decimal MovingAverage { get; set; }
    public decimal Macd { get; set; }
    public decimal Signal { get; set; }
    public string Trend { get; set; }
    public string Symbol { get; set; }
    public decimal Quantity { get; set; }
    public int Leverage { get; set; }
}
