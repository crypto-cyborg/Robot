namespace BinanceTradingBot.BinanceResponses;

using Newtonsoft.Json;

public class Position
{
    [JsonProperty("symbol")]
    public string Symbol { get; set; }            

    [JsonProperty("positionSide")]
    public string PositionSide { get; set; }      

    [JsonProperty("positionAmt")]
    public decimal PositionAmt { get; set; }      

    [JsonProperty("entryPrice")]
    public decimal EntryPrice { get; set; }       

    [JsonProperty("breakEvenPrice")]
    public decimal BreakEvenPrice { get; set; }   

    [JsonProperty("markPrice")]
    public decimal MarkPrice { get; set; }        

    [JsonProperty("unRealizedProfit")]
    public decimal UnRealizedProfit { get; set; } 

    [JsonProperty("liquidationPrice")]
    public decimal LiquidationPrice { get; set; } 

    [JsonProperty("isolatedMargin")]
    public decimal IsolatedMargin { get; set; }   

    [JsonProperty("notional")]
    public decimal Notional { get; set; }         

    [JsonProperty("marginAsset")]
    public string MarginAsset { get; set; }       

    [JsonProperty("isolatedWallet")]
    public decimal IsolatedWallet { get; set; }   

    [JsonProperty("initialMargin")]
    public decimal InitialMargin { get; set; }    

    [JsonProperty("maintMargin")]
    public decimal MaintMargin { get; set; }      

    [JsonProperty("positionInitialMargin")]
    public decimal PositionInitialMargin { get; set; } 

    [JsonProperty("openOrderInitialMargin")]
    public decimal OpenOrderInitialMargin { get; set; } 

    [JsonProperty("adl")]
    public int Adl { get; set; }                  

    [JsonProperty("bidNotional")]
    public decimal BidNotional { get; set; }      

    [JsonProperty("askNotional")]
    public decimal AskNotional { get; set; }      

    [JsonProperty("updateTime")]
    public long UpdateTime { get; set; }          
}


