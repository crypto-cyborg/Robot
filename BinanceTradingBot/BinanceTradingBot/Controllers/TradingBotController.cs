using BinanceTradingBot.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BinanceTradingBot.Controllers
{
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
        public async Task<IActionResult> StartBot([FromBody] StartBotRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ApiKey) || string.IsNullOrWhiteSpace(request.ApiSecret))
            {
                return BadRequest("API Key and Secret are required.");
            }

            await _tradingBotService.StartBotAsync(request.ApiKey, request.ApiSecret, request.Symbol, request.TradeAmount, request.Leverage);
            return Ok("Bot started successfully.");
        }

        [HttpPost("stop")]
        public IActionResult StopBot([FromBody] StopBotRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ApiKey))
            {
                return BadRequest("API Key is required.");
            }

            _tradingBotService.StopBot(request.ApiKey);
            return Ok("Bot stopped successfully.");
        }
    }

    public class StartBotRequest
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string Symbol { get; set; }
        public decimal TradeAmount { get; set; }
        public int Leverage { get; set; } 
    }

    public class StopBotRequest
    {
        public string ApiKey { get; set; }
    }
}
