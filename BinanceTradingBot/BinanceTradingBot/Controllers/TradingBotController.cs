using Microsoft.AspNetCore.Mvc;
using BinanceTradingBot.Interfaces;
using BinanceTradingBot.Models;
using BinanceTradingBot.Services;
using BinanceTradingBot.Enums;


namespace BinanceTradingBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradingBotController : ControllerBase
{
    private readonly ITradingBotService _tradingBotService;
    private string binanceReal = "https://fapi.binance.com/api";
    private string binanceTestnet = "https://testnet.binancefuture.com";

    public TradingBotController(ITradingBotService tradingBotService)
    {
        _tradingBotService = tradingBotService;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartBot([FromBody] StartBotRequest request)
    {
        BotInstance botInstance = new BotInstance()
        {
            ApiKey = request.ApiKey,
            ApiSecret = request.ApiSecret,
            Symbol = request.Symbol,
            TradeAmount = request.TradeAmount,
            Leverage = request.Leverage,
            Client = new BinanceRestClient(binanceTestnet, request.ApiKey, request.ApiSecret),   
            PredictionModel = new TradePredictionModel(),
        };

        _tradingBotService.StartBotAsync(botInstance);
        return Ok("Bot started successfully.");
    }

    [HttpPost("stop")]
    public IActionResult StopBot([FromBody] StopBotRequest request)
    {
        _tradingBotService.StopBot(request.ApiKey);
        return Ok("Bot stopped successfully.");
    }    
    
    [HttpPost("buy")]
    public async Task<IActionResult> Buy([FromBody] StartBotRequest request)
    {
        BotInstance botInstance = new BotInstance()
        {
            ApiKey = request.ApiKey,
            ApiSecret = request.ApiSecret,
            Symbol = request.Symbol,
            TradeAmount = request.TradeAmount,
            Leverage = request.Leverage,
            Client = new BinanceRestClient(binanceTestnet, request.ApiKey, request.ApiSecret),            
        };

        var client = new BinanceRestClient(binanceTestnet, botInstance.ApiKey, botInstance.ApiSecret);
        var Response = await client.PlaceOrderAsync(botInstance, Side.BUY);
        
        return Ok(Response);
    }  

    
    [HttpGet("positions")]
    public async Task<IActionResult> GetOpenPositions([FromQuery] string apiKey, [FromQuery] string apiSecret)
    {
        var client = new BinanceRestClient(binanceTestnet, apiKey, apiSecret);
        var openPositions = await client.GetOpenPositionsAsync();
        return Ok(openPositions);
    }
    
    [HttpGet("balance")]
    public async Task<IActionResult> GetFuturesAccountBalance([FromQuery] string apiKey, [FromQuery] string apiSecret)
    {
        var client = new BinanceRestClient(binanceTestnet, apiKey, apiSecret);
        var balances = await client.GetBalanceAsync();
        return Ok(balances);
    }

    [HttpGet("klines")]
    public async Task<IActionResult> GetKlines([FromQuery] string apiKey, [FromQuery] string apiSecret, [FromQuery] string symbol, [FromQuery] string interval, [FromQuery] int limit)
    {
        var client = new BinanceRestClient(binanceTestnet, apiKey, apiSecret);
        var klines = await client.GetKlinesAsync(symbol, interval, limit);
        return Ok(klines);
    }
}

