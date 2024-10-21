using Microsoft.AspNetCore.Mvc;
using BinanceTradingBot.Interfaces;
using BinanceTradingBot.Models;
using BinanceTradingBot.Services;


namespace BinanceTradingBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradingBotController : ControllerBase
{
    private readonly ITradingBotService _tradingBotService;
    private string binanceReal = "https://api.binance.com/api";
    private string binanceTestnet = "https://testnet.binance.vision/api";

    public TradingBotController(ITradingBotService tradingBotService)
    {
        _tradingBotService = tradingBotService;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartBot([FromBody] StartBotRequest request)
    {
        BotInstance botInstance = new BotInstance()
        {
            Symbol = request.Symbol,
            TradeAmount = request.TradeAmount,
            Leverage = request.Leverage,
            Client = new BinanceRestClient(binanceTestnet, request.ApiKey, request.ApiSecret),            
        };

        await _tradingBotService.StartBotAsync(botInstance);
        return Ok("Bot started successfully.");
    }

    [HttpPost("stop")]
    public IActionResult StopBot([FromBody] StopBotRequest request)
    {
        _tradingBotService.StopBot(request.ApiKey);
        return Ok("Bot stopped successfully.");
    }
}

