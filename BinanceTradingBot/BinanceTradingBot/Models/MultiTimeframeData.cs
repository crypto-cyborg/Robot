namespace BinanceTradingBot.Models;

public class MultiTimeframeData
{
    public List<TradeData> FourHourData { get; set; }
    public List<TradeData> FiveMinuteData { get; set; }
    public List<TradeData> OneMinuteData { get; set; }
}
