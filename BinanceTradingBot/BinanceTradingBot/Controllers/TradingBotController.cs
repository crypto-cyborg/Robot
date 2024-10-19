using Microsoft.AspNetCore.Mvc;
using BinanceTradingBot.Interfaces;
using BinanceTradingBot.Models;


namespace BinanceTradingBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradingBotController : ControllerBase
{
    private readonly ITradingBotService _tradingBotService;

    public TradingBotController(ITradingBotService tradingBotService)
    {
        _tradingBotService = tradingBotService;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartBot([FromBody] BotInstance botInstance)
    {
        await _tradingBotService.StartBot(botInstance);
        return Ok("Bot started successfully.");
    }

    [HttpPost("stop")]
    public IActionResult StopBot([FromBody] BotInstance botInstance)
    {
        _tradingBotService.StopBot(botInstance);
        return Ok("Bot stopped successfully.");
    }
}

