using Newtonsoft.Json;

namespace BinanceTradingBot.BinanceResponses;

public class PriceData
{
    [JsonProperty("symbol")]
    public string Symbol { get; set; }

    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("time")]
    public long Time { get; set; }
}
