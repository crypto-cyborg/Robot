using Newtonsoft.Json;

namespace BinanceTradingBot.BinanceResponses;

public class Order
{
    [JsonProperty("clientOrderId")]
    public string ClientOrderId { get; set; }

    [JsonProperty("cumQty")]
    public string CumQty { get; set; }

    [JsonProperty("cumQuote")]
    public string CumQuote { get; set; }

    [JsonProperty("executedQty")]
    public string ExecutedQty { get; set; }

    [JsonProperty("orderId")]
    public long OrderId { get; set; }

    [JsonProperty("avgPrice")]
    public string AvgPrice { get; set; }

    [JsonProperty("origQty")]
    public string OrigQty { get; set; }

    [JsonProperty("price")]
    public string Price { get; set; }

    [JsonProperty("reduceOnly")]
    public bool ReduceOnly { get; set; }

    [JsonProperty("side")]
    public string Side { get; set; }

    [JsonProperty("positionSide")]
    public string PositionSide { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("stopPrice")]
    public string StopPrice { get; set; }

    [JsonProperty("closePosition")]
    public bool ClosePosition { get; set; }

    [JsonProperty("symbol")]
    public string Symbol { get; set; }

    [JsonProperty("timeInForce")]
    public string TimeInForce { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("origType")]
    public string OrigType { get; set; }

    [JsonProperty("activatePrice")]
    public string ActivatePrice { get; set; }

    [JsonProperty("priceRate")]
    public string PriceRate { get; set; }

    [JsonProperty("updateTime")]
    public long UpdateTime { get; set; }

    [JsonProperty("workingType")]
    public string WorkingType { get; set; }

    [JsonProperty("priceProtect")]
    public bool PriceProtect { get; set; }

    [JsonProperty("priceMatch")]
    public string PriceMatch { get; set; }

    [JsonProperty("selfTradePreventionMode")]
    public string SelfTradePreventionMode { get; set; }

    [JsonProperty("goodTillDate")]
    public long GoodTillDate { get; set; }
}
