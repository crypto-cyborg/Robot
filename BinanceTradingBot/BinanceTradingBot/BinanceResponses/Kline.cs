using Newtonsoft.Json;
namespace BinanceTradingBot.BinanceResponses;

public class Kline
{
    [JsonProperty("openTime")]
    public long OpenTime { get; set; }

    [JsonProperty("open")]
    public decimal Open { get; set; }

    [JsonProperty("high")]
    public decimal High { get; set; }

    [JsonProperty("low")]
    public decimal Low { get; set; }

    [JsonProperty("close")]
    public decimal Close { get; set; }

    [JsonProperty("volume")]
    public decimal Volume { get; set; }

    [JsonProperty("closeTime")]
    public long CloseTime { get; set; }

    [JsonProperty("quoteAssetVolume")]
    public decimal QuoteAssetVolume { get; set; }

    [JsonProperty("numberOfTrades")]
    public int NumberOfTrades { get; set; }

    [JsonProperty("takerBuyBaseAssetVolume")]
    public decimal TakerBuyBaseAssetVolume { get; set; }

    [JsonProperty("takerBuyQuoteAssetVolume")]
    public decimal TakerBuyQuoteAssetVolume { get; set; }
}
