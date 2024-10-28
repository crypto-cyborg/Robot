using Newtonsoft.Json;

namespace BinanceTradingBot.BinanceResponses;

public class AccountBalance
{
    [JsonProperty("accountAlias")]
    public string AccountAlias { get; set; }

    [JsonProperty("asset")]
    public string Asset { get; set; }

    [JsonProperty("balance")]
    public decimal Balance { get; set; }

    [JsonProperty("withdrawAvailable")]
    public decimal WithdrawAvailable { get; set; }

    [JsonProperty("crossWalletBalance")]
    public decimal CrossWalletBalance { get; set; }

    [JsonProperty("crossUnPnl")]
    public decimal CrossUnPnl { get; set; }

    [JsonProperty("availableBalance")]
    public decimal AvailableBalance { get; set; }

    [JsonProperty("updateTime")]
    public long UpdateTime { get; set; }
}
