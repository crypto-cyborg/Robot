namespace BinanceTradingBot.Models
{
    public class StartBotRequest
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string Symbol { get; set; }
        public decimal TradeAmount { get; set; }
        public int Leverage { get; set; }
    }

}
