using BinanceTradingBot.Interfaces;
using BinanceTradingBot.Models;
using RestSharp;

namespace BinanceTradingBot.Services;

public class TradeExecutionService : ITradeExecutionService
{
    public async Task ExecuteTradeAsync(BotInstance botInstance, string trend)
    {
        if (trend == "neutral")
            return;

        var requestLeverage = new RestRequest("/sapi/v1/margin/leverage", Method.Post);
        requestLeverage.AddParameter("symbol", botInstance.Symbol);
        requestLeverage.AddParameter("leverage", botInstance.Leverage);

        try
        {
            var responseLeverage = await botInstance.Client.ExecuteAsync(requestLeverage, Method.Post, true);
            if (responseLeverage.IsSuccessful)
            {
                Console.WriteLine($"Leverage set to {botInstance.Leverage} for symbol {botInstance.Symbol}");
            }
            else
            {
                Console.WriteLine($"Failed to set leverage: {responseLeverage.Content}");
                return; 
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting leverage: {ex.Message}");
            return;
        }

        var requestOrder = new RestRequest("/fapi/v1/order", Method.Post);
        requestOrder.AddParameter("symbol", botInstance.Symbol);
        requestOrder.AddParameter("side", trend == "long" ? "BUY" : "SELL");
        requestOrder.AddParameter("type", "MARKET");
        requestOrder.AddParameter("quantity", botInstance.TradeAmount);

        try
        {
            var responseOrder = await botInstance.Client.ExecuteAsync(requestOrder, Method.Post, true);
            if (responseOrder.IsSuccessful)
            {
                Console.WriteLine($"Order placed successfully: {responseOrder.Content}");
            }
            else
            {
                Console.WriteLine($"Error placing order: {responseOrder.Content}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error placing order: {ex.Message}");
        }
    }
}
