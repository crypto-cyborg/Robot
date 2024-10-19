using BinanceTradingBot.BinanceResponses;
using BinanceTradingBot.Models;
using BinanceTradingBot.Services;
using RestSharp;

namespace BinanceTradingBot.Interfaces;

public interface ITradeExecutionService
{
    Task UpdateTrailingStopAsync(BotInstance botInstance, string trend);
    Task SetStopLossOnBinance(BotInstance botInstance, string trend, decimal stopLossPrice);
    Task<decimal> GetCurrentPriceAsync(BinanceRestClient client, string symbol);
}
