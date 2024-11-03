using BinanceTradingBot.BinanceResponses;

namespace BinanceTradingBot.Interfaces;

public interface IResponceConverter
{
    List<Kline> KlineConvert(object[][] responce);
}