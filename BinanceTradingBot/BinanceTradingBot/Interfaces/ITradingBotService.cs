namespace BinanceTradingBot.Interfaces;

public interface ITradingBotService
{
    Task StartBotAsync(string apiKey, string apiSecret, string symbol, decimal tradeAmount, int leverage);
    void StopBot(string apiKey);
}
