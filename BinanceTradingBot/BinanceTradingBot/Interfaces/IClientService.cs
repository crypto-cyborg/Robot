using BinanceTradingBot.Enums;
using BinanceTradingBot.Models;

namespace BinanceTradingBot.Interfaces;

public interface IClientService
{
    Task<string> PlaceOrderAsync(BotInstance botInstance, Side side);
    Task SetTrailingStopAsync(BotInstance botInstance);

}