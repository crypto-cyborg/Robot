using BinanceTradingBot.Models;

namespace BinanceTradingBot.Interfaces;

public interface ITradeExecutionService
{
    Task ExecuteTradeAsync(BotInstance botInstance, string trend);
}
