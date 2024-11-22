using BinanceTradingBot.BinanceResponses;
using BinanceTradingBot.Interfaces;

namespace BinanceTradingBot.Services;

public class ResponceConverter : IResponceConverter
{
    public List<Kline> KlineConvert(object[][] responce)
    {
        List<Kline> klines = new List<Kline>();
        
        foreach (var item in responce)
        {
            var kline = new Kline
            {
                OpenTime = Convert.ToInt64(item[0]),
                Open = Convert.ToSingle(item[1]),  
                High = Convert.ToSingle(item[2]),  
                Low = Convert.ToSingle(item[3]),   
                Close = Convert.ToSingle(item[4]), 
                Volume = Convert.ToSingle(item[5]),
                CloseTime = Convert.ToInt64(item[6]),
                QuoteAssetVolume = Convert.ToSingle(item[7]), 
                NumberOfTrades = Convert.ToInt32(item[8]),
                TakerBuyBaseAssetVolume = Convert.ToSingle(item[9]), 
                TakerBuyQuoteAssetVolume = Convert.ToSingle(item[10])
            };
            klines.Add(kline);
        }
        
        return klines;
    }
}