namespace BinanceTradingBot.BinanceResponses;

using Newtonsoft.Json;

public class Position
{
    [JsonProperty("symbol")]
    public string Symbol { get; set; }            

    [JsonProperty("positionSide")]
    public string PositionSide { get; set; }      

    [JsonProperty("positionAmt")]
    public float PositionAmt { get; set; }      

    [JsonProperty("entryPrice")]
    public float EntryPrice { get; set; }       

    [JsonProperty("breakEvenPrice")]
    public float BreakEvenPrice { get; set; }   

    [JsonProperty("markPrice")]
    public float MarkPrice { get; set; }        

    [JsonProperty("unRealizedProfit")]
    public float UnRealizedProfit { get; set; } 

    [JsonProperty("liquidationPrice")]
    public float LiquidationPrice { get; set; } 

    [JsonProperty("isolatedMargin")]
    public float IsolatedMargin { get; set; }   

    [JsonProperty("notional")]
    public float Notional { get; set; }         

    [JsonProperty("marginAsset")]
    public string MarginAsset { get; set; }       

    [JsonProperty("isolatedWallet")]
    public float IsolatedWallet { get; set; }   

    [JsonProperty("initialMargin")]
    public float InitialMargin { get; set; }    

    [JsonProperty("maintMargin")]
    public float MaintMargin { get; set; }      

    [JsonProperty("positionInitialMargin")]
    public float PositionInitialMargin { get; set; } 

    [JsonProperty("openOrderInitialMargin")]
    public float OpenOrderInitialMargin { get; set; } 

    [JsonProperty("adl")]
    public int Adl { get; set; }                  

    [JsonProperty("bidNotional")]
    public float BidNotional { get; set; }      

    [JsonProperty("askNotional")]
    public float AskNotional { get; set; }      

    [JsonProperty("updateTime")]
    public long UpdateTime { get; set; }          
}


