using Newtonsoft.Json;

namespace BinanceTradingBot.BinanceResponses;

public class AccountInfo
{
    [JsonProperty("totalWalletBalance")]
    public decimal TotalWalletBalance { get; set; }

    [JsonProperty("availableBalance")]
    public decimal AvailableBalance { get; set; }

    [JsonProperty("assets")]
    public List<AssetInfo> Assets { get; set; }

    [JsonProperty("positions")]
    public List<Position> Positions { get; set; }
}
