using Newtonsoft.Json;

namespace BinanceTradingBot.BinanceResponses;

public class AccountBalance
{
    [JsonProperty("accountAlias")]
    public string AccountAlias { get; set; }

    [JsonProperty("asset")]
    public string Asset { get; set; }

    [JsonProperty("balance")]
    public float Balance { get; set; }

    [JsonProperty("withdrawAvailable")]
    public float WithdrawAvailable { get; set; }

    [JsonProperty("crossWalletBalance")]
    public float CrossWalletBalance { get; set; }

    [JsonProperty("crossUnPnl")]
    public float CrossUnPnl { get; set; }

    [JsonProperty("availableBalance")]
    public float AvailableBalance { get; set; }

    [JsonProperty("updateTime")]
    public long UpdateTime { get; set; }
}
