using BinanceTradingBot.Models;
using Newtonsoft.Json;
using RestSharp;
using System.Security.Cryptography;
using System.Text;
using BinanceTradingBot.Interfaces;
using BinanceTradingBot.Enums;
using BinanceTradingBot.BinanceResponses;

namespace BinanceTradingBot.Services;

public class BinanceRestClient : IClientService
{
    private readonly RestClient _client;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly IResponceConverter _responceConverter;

    public BinanceRestClient(string baseUrl, string apiKey, string apiSecret)
    {
        _client = new RestClient(baseUrl);
        _apiKey = apiKey;
        _apiSecret = apiSecret;
        _responceConverter = new ResponceConverter();
    }

    private string CreateSignature(string queryString)
    {
        using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(_apiSecret)))
        {
            var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(queryString));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    private async Task<RestResponse> ExecuteAsync(RestRequest request, bool requireSignature = false)
    {
        request.AddHeader("X-MBX-APIKEY", _apiKey); 

        if (requireSignature)
        {
            request.AddQueryParameter("recvWindow", 55000); 
            request.AddQueryParameter("timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()); 

            var queryString = CreateQueryString(request);
            Console.WriteLine($"Query String Before Signing: {queryString}"); 

            var signature = CreateSignature(queryString);
            request.AddQueryParameter("signature", signature);
        }


        var response = await _client.ExecuteAsync(request);

        if (!response.IsSuccessful)
        {
            throw new Exception($"Error: {response.StatusCode}, Message: {response.Content}");
        }

        return response;
    }

    private string CreateQueryString(RestRequest request)
    {
        var queryString = new StringBuilder();
        foreach (var param in request.Parameters)
        {
            if (param.Type == ParameterType.QueryString)
            {
                queryString.Append($"{param.Name}={param.Value}&");
            }
        }
        return queryString.ToString().TrimEnd('&'); 
    }

    public async Task<string> PlaceOrderAsync(BotInstance botInstance, Side side)
    {
        try
        {            
            var leverageRequest = new RestRequest("/fapi/v1/leverage", Method.Post);
            leverageRequest.AddQueryParameter("symbol", botInstance.Symbol);
            leverageRequest.AddQueryParameter("leverage", (int)botInstance.Leverage);
            
            var leverageResponse = await ExecuteAsync(leverageRequest, requireSignature: true);

            if (leverageResponse.IsSuccessful)
            {                
                Console.WriteLine($"Leverage {botInstance.Leverage}x set successfully for {botInstance.Symbol}");

                var price = await GetCurrentPriceAsync(botInstance.Symbol);

                if (price == null)
                {
                    Console.WriteLine("Unable to get current price.");
                    return "Error getting price.";
                }

                var quantity = botInstance.TradeAmount / price;
                quantity *= (int)botInstance.Leverage;
                quantity = (float)Math.Round(quantity, 3);

                var request = new RestRequest("/fapi/v1/order", Method.Post);
                    request.AddQueryParameter("symbol", botInstance.Symbol);
                    request.AddQueryParameter("side", side);
                    request.AddQueryParameter("type", OrderType.MARKET);
                    request.AddQueryParameter("quantity", (decimal)quantity);

                    var response = await ExecuteAsync(request, requireSignature: true);

                    if (response.IsSuccessful)
                    {
                        Console.WriteLine($"{side} order executed successfully for {botInstance.TradeAmount} of {botInstance.Symbol}");
                        return response.Content;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to execute {side} order: {response.Content}");
                    }                
            }
            else
            {
                Console.WriteLine($"Failed to set leverage: {leverageResponse.Content}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing {side} order: {ex.Message}");
        }
        return $"Failed to execute {side} order";
    }


    public async Task SetTrailingStopAsync(BotInstance botInstance)
    {
        try
        {
            var positionRequest = new RestRequest("/fapi/v2/positionRisk", Method.Get);
            positionRequest.AddQueryParameter("symbol", botInstance.Symbol);
            var positionResponse = await ExecuteAsync(positionRequest, requireSignature: true);

            if (!positionResponse.IsSuccessful)
            {
                Console.WriteLine($"Failed to fetch position: {positionResponse.Content}");
                return;
            }

            var positions = JsonConvert.DeserializeObject<List<Position>>(positionResponse.Content);
            var position = positions.FirstOrDefault(p => p.Symbol == botInstance.Symbol && p.PositionAmt != 0);

            if (position == null)
            {
                Console.WriteLine($"No open position found for {botInstance.Symbol}");
                return;
            }

            var side = position.PositionAmt > 0 ? Side.SELL : Side.BUY;

            var trailingStopRequest = new RestRequest("/fapi/v1/order", Method.Post);
            trailingStopRequest.AddQueryParameter("symbol", botInstance.Symbol);
            trailingStopRequest.AddQueryParameter("side", side);
            trailingStopRequest.AddQueryParameter("type", "TRAILING_STOP_MARKET");
            trailingStopRequest.AddQueryParameter("callbackRate", 1m);
            trailingStopRequest.AddQueryParameter("quantity", (decimal)Math.Abs(position.PositionAmt));
                       

            var response = await ExecuteAsync(trailingStopRequest, requireSignature: true);

            if (response.IsSuccessful)
            {
                Console.WriteLine($"Trailing Stop успешно установлен");
            }
            else
            {
                Console.WriteLine($"Ошибка установки Trailing Stop: {response.Content}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
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

    public async Task<List<Position>> GetOpenPositionsAsync()
    {
        var request = new RestRequest("/fapi/v2/positionRisk", Method.Get);
        var response = await ExecuteAsync(request, requireSignature: true);
        if (response.IsSuccessful)
        {
            return JsonConvert.DeserializeObject<List<Position>>(response.Content)
                    .Where(p => p.PositionAmt != 0).ToList();
        }
        else
        {
            Console.WriteLine($"Error while getting futures account balance: {response.Content}");
            throw new Exception($"Unable to get balance: {response.StatusCode}");
        }
    }

    public async Task<List<AccountBalance>> GetBalanceAsync()
    {
        var request = new RestRequest("/fapi/v3/balance", Method.Get); 

        var response = await ExecuteAsync(request, requireSignature: true);

        if (response.IsSuccessful)
        {
            return JsonConvert.DeserializeObject<List<AccountBalance>>(response.Content);
        }
        else
        {
            Console.WriteLine($"Error while getting futures account balance: {response.Content}");
            throw new Exception($"Unable to get balance: {response.StatusCode}");
        }
    }

}


