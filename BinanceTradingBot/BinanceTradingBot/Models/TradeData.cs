namespace BinanceTradingBot.Models;
public class TradeData
{
    public decimal Price { get; set; }            // Цена закрытия свечи
    public decimal MovingAverage { get; set; }    // Скользящее среднее
    public decimal MacdSignal { get; set; }       // Сигнальная линия MACD
    public decimal Rsi { get; set; }              // Индикатор RSI
    public decimal Volume { get; set; }           // Объем торгов
    public string Trend { get; set; }             // Тренд ("long" или "short")
    public string Symbol { get; set; }            // Торговый символ (например, BTCUSDT)
}
