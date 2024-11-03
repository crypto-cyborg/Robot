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
                Open = Convert.ToDecimal(item[1]),
                High = Convert.ToDecimal(item[2]),
                Low = Convert.ToDecimal(item[3]),
                Close = Convert.ToDecimal(item[4]),
                Volume = Convert.ToDecimal(item[5]),
                CloseTime = Convert.ToInt64(item[6]),
                QuoteAssetVolume = Convert.ToDecimal(item[7]),
                NumberOfTrades = Convert.ToInt32(item[8]),
                TakerBuyBaseAssetVolume = Convert.ToDecimal(item[9]),
                TakerBuyQuoteAssetVolume = Convert.ToDecimal(item[10])
            };
            klines.Add(kline);
        }
        
        return klines;
    }
}