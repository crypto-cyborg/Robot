using BinanceTradingBot.BinanceResponses;
using BinanceTradingBot.Interfaces;
using BinanceTradingBot.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinanceTradingBot.Services
{
    public class TradeExecutionService : ITradeExecutionService
    {
        public async void ExecuteBuy(BotInstance botInstance, TradeData data)
        {
            Console.WriteLine($"Executing BUY for {data.Quantity} of {data.Symbol}");

            var client = botInstance.Client;
            var request = new RestRequest("/api/v3/order", Method.Post);
            request.AddParameter("symbol", data.Symbol);
            request.AddParameter("side", "BUY");
            request.AddParameter("type", "MARKET");
            request.AddParameter("quantity", data.Quantity);

            try
            {
                var response = await client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    Console.WriteLine($"BUY order executed successfully for {data.Quantity} of {data.Symbol}");
                }
                else
                {
                    Console.WriteLine($"Failed to execute BUY order: {response.Content}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing BUY order: {ex.Message}");
            }
        }

        public async void ExecuteSell(BotInstance botInstance, TradeData data)
        {
            Console.WriteLine($"Executing SELL for {data.Quantity} of {data.Symbol}");

            var client = botInstance.Client;
            var request = new RestRequest("/api/v3/order", Method.Post);
            request.AddParameter("symbol", data.Symbol);
            request.AddParameter("side", "SELL");
            request.AddParameter("type", "MARKET");
            request.AddParameter("quantity", data.Quantity);

            try
            {
                var response = await client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    Console.WriteLine($"SELL order executed successfully for {data.Quantity} of {data.Symbol}");
                }
                else
                {
                    Console.WriteLine($"Failed to execute SELL order: {response.Content}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing SELL order: {ex.Message}");
            }
        }

        public async Task UpdateTrailingStopAsync(BotInstance botInstance, string trend)
        {
            var currentPrice = await GetCurrentPriceAsync(botInstance.Client, botInstance.Symbol);
            var atr = await CalculateATR(botInstance.Client, botInstance.Symbol, 14);

            if (trend == "long")
            {
                // Логика для длинной позиции
                var stopLossPrice = currentPrice - atr;
                await SetStopLossOnBinance(botInstance, trend, stopLossPrice);
            }
            else if (trend == "short")
            {
                // Логика для короткой позиции
                var stopLossPrice = currentPrice + atr;
                await SetStopLossOnBinance(botInstance, trend, stopLossPrice);
            }
        }

        public async Task SetStopLossOnBinance(BotInstance botInstance, string trend, decimal stopLossPrice)
        {
            var client = botInstance.Client;
            var request = new RestRequest("/api/v3/order", Method.Post);
            request.AddParameter("symbol", botInstance.Symbol);
            request.AddParameter("side", trend == "long" ? "SELL" : "BUY");
            request.AddParameter("type", "STOP_LOSS_LIMIT");
            request.AddParameter("quantity", botInstance.TradeAmount);
            request.AddParameter("stopPrice", stopLossPrice);
            request.AddParameter("price", stopLossPrice);
            request.AddParameter("timeInForce", "GTC");

            try
            {
                var response = await client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    Console.WriteLine($"Stop Loss order placed successfully at {stopLossPrice}.");
                }
                else
                {
                    Console.WriteLine($"Failed to place Stop Loss order: {response.Content}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error placing Stop Loss order: {ex.Message}");
            }
        }

        public async Task<decimal> GetCurrentPriceAsync(BinanceRestClient client, string symbol)
        {
            var request = new RestRequest("/fapi/v2/ticker/price", Method.Get);
            request.AddParameter("symbol", symbol);

            var response = await client.ExecuteAsync(request);
            var priceData = Newtonsoft.Json.JsonConvert.DeserializeObject<PriceData>(response.Content);

            return priceData.Price;
        }

        private async Task<decimal> CalculateATR(BinanceRestClient client, string symbol, int period)
        {
            var klines = await GetKlinesAsync(client, symbol, "1d", period);
            var trueRanges = new List<decimal>();

            for (int i = 1; i < klines.Count; i++)
            {
                var previousClose = klines[i - 1].Close;
                var highLow = klines[i].High - klines[i].Low;
                var highClose = Math.Abs(klines[i].High - previousClose);
                var lowClose = Math.Abs(klines[i].Low - previousClose);

                var trueRange = Math.Max(highLow, Math.Max(highClose, lowClose));
                trueRanges.Add(trueRange);
            }

            return trueRanges.Average();
        }

        private async Task<List<Kline>> GetKlinesAsync(BinanceRestClient client, string symbol, string interval, int limit)
        {
            var request = new RestRequest("/api/v3/klines", Method.Get);
            request.AddParameter("symbol", symbol);
            request.AddParameter("interval", interval);
            request.AddParameter("limit", limit);

            var response = await client.ExecuteAsync(request);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Kline>>(response.Content);
        }
    }

}
