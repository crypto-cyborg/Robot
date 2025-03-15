using BinanceTradingBot.BinanceResponses;
using BinanceTradingBot.Enums;
using BinanceTradingBot.Interfaces;
using BinanceTradingBot.Models;
using Newtonsoft.Json;
using RestSharp;

namespace BinanceTradingBot.Services;

public class SandboxClient
{
    
    private readonly RestClient _client;
    private readonly IResponceConverter _responceConverter;

    public SandboxClient(string baseUrl, string apiKey, string apiSecret)
    {
        _client = new RestClient(baseUrl);
        _responceConverter = new ResponceConverter();
    }
    

    private async Task<RestResponse> ExecuteAsync(RestRequest request)
    {
        try
        {
            var response = await _client.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                throw new Exception($"Ошибка API: {response.StatusCode} - {response.ErrorMessage}");
            }
            return response;
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка запроса: {ex.Message}");
        }
    }    

    public async Task<string> PlaceOrderAsync(BotInstance botInstance, Side side)
    {
        var orderDto = new OrderDto
        {
            Id = Guid.NewGuid(),
            WalletId = (Guid)botInstance.WalletId,
            Symbol = botInstance.Symbol,
            Quantity = (decimal)botInstance.TradeAmount,
            OrderType = "MARKET",
            Direction = side.ToString(),
        };

        var request = new RestRequest("/api/MarginTrading/place-order", Method.Post);
        request.AddJsonBody(orderDto);

        var response = await ExecuteAsync(request);
        var orderResponse = JsonConvert.DeserializeObject<OrderDto>(response.Content);
        
        return orderResponse.Id.ToString(); 
    }
    


    public async Task SetTrailingStopAsync(BotInstance botInstance)
    {
        
    }

    public async Task<float> GetCurrentPriceAsync(string symbol)
    {
        var request = new RestRequest("/fapi/v2/ticker/price", Method.Get);
        request.AddQueryParameter("symbol", symbol);

        var response = await _client.ExecuteAsync(request);
        var priceData = JsonConvert.DeserializeObject<PriceData>(response.Content);

        return priceData.Price;
    }

    public async Task<List<Kline>> GetKlinesAsync(string symbol, string interval, int limit)
    {
        try
        {
            var request = new RestRequest("/fapi/v1/klines", Method.Get);
            request.AddQueryParameter("symbol", symbol);
            request.AddQueryParameter("interval", interval);
            request.AddQueryParameter("limit", limit);

            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {                
                throw new Exception($"Ошибка при получении данных: {response.ErrorMessage}");
            }

            var klineData = JsonConvert.DeserializeObject<object[][]>(response.Content);
            var klines = _responceConverter.KlineConvert(klineData);

            return klines;
        }
        catch (Exception ex)
        {            
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
            return new List<Kline>();
        }
    }

    public async Task<List<Position>> GetOpenPositionsAsync(Guid walletId)
    {
        var request = new RestRequest($"/api/MarginTrading/active-positions/{walletId}", Method.Get);
        var response = await ExecuteAsync(request);

        var positions = JsonConvert.DeserializeObject<List<Position>>(response.Content);
        return positions;
    }



}

