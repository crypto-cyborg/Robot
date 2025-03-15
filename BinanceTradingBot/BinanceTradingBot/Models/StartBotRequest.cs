namespace BinanceTradingBot.Models
{
    public class StartBotRequest
    {
        public bool IsRealTrading { get; set; }
        public string? ApiKey { get; set; }
        public string? ApiSecret { get; set; }
        public string Symbol { get; set; }
        public float TradeAmount { get; set; }
        public int? Leverage { get; set; }
        public Guid? WalletId { get; set; }
    }

}
