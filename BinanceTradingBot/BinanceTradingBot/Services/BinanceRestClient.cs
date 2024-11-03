using BinanceTradingBot.BinanceResponses;
using BinanceTradingBot.Models;
using Newtonsoft.Json;
using RestSharp;
using System.Security.Cryptography;
using System.Text;
using BinanceTradingBot.Interfaces;

namespace BinanceTradingBot.Services;

public class BinanceRestClient
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
            
            request.AddQueryParameter("recvWindow", 10000); 
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

    public async Task ExecuteBuy(BotInstance botInstance)
    {
        try
        {            
            var leverageRequest = new RestRequest("/fapi/v1/leverage", Method.Post);
            leverageRequest.AddParameter("symbol", botInstance.Symbol);
            leverageRequest.AddParameter("leverage", botInstance.Leverage);
            var leverageResponse = await ExecuteAsync(leverageRequest, requireSignature: true);

            if (leverageResponse.IsSuccessful)
            {
                Console.WriteLine($"Leverage {botInstance.Leverage}x set successfully for {botInstance.Symbol}");

                var request = new RestRequest("/fapi/v1/order", Method.Post);
                request.AddParameter("symbol", botInstance.Symbol);
                request.AddParameter("side", "BUY");
                request.AddParameter("type", "MARKET");
                request.AddParameter("quantity", botInstance.TradeAmount);
                request.AddParameter("timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

                var response = await ExecuteAsync(request, requireSignature: true);

                if (response.IsSuccessful)
                {
                    Console.WriteLine($"BUY order executed successfully for {botInstance.TradeAmount} of {botInstance.Symbol}");
                }
                else
                {
                    Console.WriteLine($"Failed to execute BUY order: {response.Content}");
                }
            }
            else
            {
                Console.WriteLine($"Failed to set leverage: {leverageResponse.Content}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing BUY order: {ex.Message}");
        }
    }

    public async Task ExecuteSell(BotInstance botInstance)
    {
        var request = new RestRequest("/fapi/v1/order", Method.Post);
        request.AddParameter("symbol", botInstance.Symbol);
        request.AddParameter("side", "SELL");
        request.AddParameter("type", "MARKET");
        request.AddParameter("quantity", botInstance.TradeAmount);
        request.AddParameter("timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        try
        {
            var response = await ExecuteAsync(request, requireSignature: true);
            if (response.IsSuccessful)
            {
                Console.WriteLine($"SELL order executed successfully for {botInstance.TradeAmount} of {botInstance.Symbol}");
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

    public async Task SetStopLossAsync(BotInstance botInstance, decimal stopLossPrice)
    {
        try
        {            
            var positionRequest = new RestRequest("/fapi/v2/positionRisk", Method.Get);
            positionRequest.AddParameter("symbol", botInstance.Symbol);
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

            string side = position.PositionAmt > 0 ? "SELL" : "BUY";

            var stopLossRequest = new RestRequest("/fapi/v1/order", Method.Post);
            stopLossRequest.AddParameter("symbol", botInstance.Symbol);
            stopLossRequest.AddParameter("side", side);  
            stopLossRequest.AddParameter("type", "STOP_MARKET"); 
            stopLossRequest.AddParameter("stopPrice", stopLossPrice);  
            stopLossRequest.AddParameter("quantity", Math.Abs(position.PositionAmt));  
            var stopLossResponse = await ExecuteAsync(stopLossRequest, requireSignature: true);

            if (stopLossResponse.IsSuccessful)
            {
                Console.WriteLine($"Stop-loss order placed successfully at {stopLossPrice} for {botInstance.Symbol}");
            }
            else
            {
                Console.WriteLine($"Failed to place stop-loss order: {stopLossResponse.Content}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error placing stop-loss order: {ex.Message}");
        }
    }


    public async Task<decimal> GetCurrentPriceAsync(string symbol)
    {
        var request = new RestRequest("/fapi/v2/ticker/price", Method.Get);
        request.AddParameter("symbol", symbol);

        var response = await _client.ExecuteAsync(request);
        var priceData = JsonConvert.DeserializeObject<PriceData>(response.Content);

        return priceData.Price;
    }



    public async Task<List<Kline>> GetKlinesAsync(string symbol, string interval, int limit)
    {
        try
        {
            var request = new RestRequest("/fapi/v1/klines", Method.Get);
            request.AddParameter("symbol", symbol);
            request.AddParameter("interval", interval);
            request.AddParameter("limit", limit);

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

    public async Task<bool> CheckOpenPositionAsync(string symbol)
    {
        var request = new RestRequest("/fapi/v3/positionRisk", Method.Get);
        request.AddParameter("timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        if (!string.IsNullOrEmpty(symbol))
        {
            request.AddParameter("symbol", symbol);
        }

        var response = await ExecuteAsync(request, requireSignature: true);

        var positionData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Position>>(response.Content);
        
        var openPosition = positionData?.FirstOrDefault(p => p.PositionAmt != 0);

        return openPosition != null;
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

    public async Task<List<AccountBalance>> GetFuturesAccountBalanceAsync()
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


