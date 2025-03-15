namespace BinanceTradingBot.Models;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string Symbol { get; set; } 
    public string OrderType { get; set; }
    public decimal Quantity { get; set; }
    public string Direction {get; set;} 

}