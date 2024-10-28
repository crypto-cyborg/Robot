using Newtonsoft.Json;

namespace BinanceTradingBot.BinanceResponses;

public class AssetInfo
{
    [JsonProperty("asset")]
    public string Asset { get; set; }

    [JsonProperty("walletBalance")]
    public decimal WalletBalance { get; set; }

    [JsonProperty("unrealizedProfit")]
    public decimal UnrealizedProfit { get; set; }

    [JsonProperty("marginBalance")]
    public decimal MarginBalance { get; set; }
}
