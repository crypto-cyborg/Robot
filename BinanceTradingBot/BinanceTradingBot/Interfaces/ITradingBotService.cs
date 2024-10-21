using BinanceTradingBot.Models;
using BinanceTradingBot.Services;

namespace BinanceTradingBot.Interfaces
{
    public interface ITradingBotService
    {
        Task StartBotAsync(BotInstance botInstance);
        void StopBot(string apiKey);
    }
}
