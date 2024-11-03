namespace BinanceTradingBot.Models;
public class TradeData
{
    public float Price { get; set; }            
    public float MovingAverage { get; set; }    
    public float MacdSignal { get; set; }       
    public float Rsi { get; set; }              
    public float Volume { get; set; }           
    public string Trend { get; set; }           
    public string Symbol { get; set; }          
}