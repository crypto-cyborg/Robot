using RestSharp;
using System.Security.Cryptography;
using System.Text;

namespace BinanceTradingBot.Services
{
    public class BinanceRestClient
    {
        private readonly RestClient _client;
        private readonly string _apiKey;
        private readonly string _apiSecret;

        public BinanceRestClient(string baseUrl, string apiKey, string apiSecret)
        {
            _client = new RestClient(baseUrl);
            _apiKey = apiKey;
            _apiSecret = apiSecret;
        }

        private string CreateSignature(string queryString)
        {
            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(_apiSecret)))
            {
                var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(queryString));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public async Task<RestResponse> ExecuteAsync(RestRequest request, Method method, bool requireSignature = false)
        {
            request.AddHeader("X-MBX-APIKEY", _apiKey);
            request.Method = method;

            if (requireSignature)
            {
                var queryString = request.Resource + "?" + request.Parameters.ToQueryString();
                var signature = CreateSignature(queryString);
                request.AddParameter("signature", signature, ParameterType.QueryString);
            }

            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"Error: {response.StatusCode}, Message: {response.Content}");
            }

            return response;
        }
    }

    public static class RequestExtensions
    {
        public static string ToQueryString(this RequestParameters parameters)
        {
            var queryString = new StringBuilder();
            foreach (var param in parameters)
            {
                if (param.Type == ParameterType.QueryString)
                {
                    queryString.Append($"{param.Name}={param.Value}&");
                }
            }
            return queryString.ToString().TrimEnd('&');
        }
    }
}
