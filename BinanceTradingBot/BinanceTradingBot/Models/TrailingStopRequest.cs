namespace BinanceTradingBot.Models;

public class TrailingStopRequest
{
    public decimal TrailingStopDistance { get; set; }
    public string Symbol {  get; set; }
}