using BinanceTradingBot.BinanceResponses;
using BinanceTradingBot.Interfaces;
using BinanceTradingBot.Models;
using RestSharp;
using System.Collections.Concurrent;


namespace BinanceTradingBot.Services
{
    public class TradingBotService : ITradingBotService
    {
        private readonly ConcurrentDictionary<string, BotInstance> _activeBots;
        private readonly ITradePredictionModel _predictionModel;
        private readonly ITradeExecutionService _tradeExecutionService;

        public TradingBotService(ITradePredictionModel predictionModel, ITradeExecutionService tradeExecutionService)
        {
            _activeBots = new ConcurrentDictionary<string, BotInstance>();
            _predictionModel = predictionModel;
            _tradeExecutionService = tradeExecutionService;
        }

        public async Task StartBotAsync(string apiKey, string apiSecret, string symbol, decimal tradeAmount, int leverage)
        {
            if (_activeBots.ContainsKey(apiKey))
            {
                Console.WriteLine($"Bot already running for API Key: {apiKey}");
                return;
            }

            var binanceClient = new BinanceRestClient("https://api.binance.com", apiKey, apiSecret);

            var botInstance = new BotInstance
            {
                ApiKey = apiKey,
                ApiSecret = apiSecret,
                Symbol = symbol,
                TradeAmount = tradeAmount,
                Leverage = leverage, 
                Client = binanceClient,
                PredictionModel = _predictionModel,
                CancellationTokenSource = new CancellationTokenSource()
            };

            

            _activeBots[apiKey] = botInstance;

            Console.WriteLine($"Starting bot for API Key: {apiKey}");

            _ = Task.Run(() => ExecuteBotLogicAsync(botInstance.CancellationTokenSource.Token, botInstance));
        }

        public void StopBot(string apiKey)
        {
            if (_activeBots.TryGetValue(apiKey, out var botInstance))
            {
                botInstance.CancellationTokenSource.Cancel();
                _activeBots.TryRemove(apiKey, out _);
                Console.WriteLine($"Bot stopped for API Key: {apiKey}");
            }
            else
            {
                Console.WriteLine($"No active bot found for API Key: {apiKey}");
            }
        }

        private async Task ExecuteBotLogicAsync(CancellationToken cancellationToken, BotInstance botInstance)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Проверка наличия открытой позиции
                bool isPositionOpen = await CheckIfPositionOpenAsync(botInstance);

                if (isPositionOpen)
                {
                    Console.WriteLine($"Position is already open for {botInstance.Symbol}. Skipping new trade.");
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken); // Ждем 10 секунд перед следующей проверкой
                    continue;
                }

                // Получаем данные рынка и предсказываем тренд
                var trendData = await GetTrendDataAsync(botInstance.Client, botInstance.Symbol);
                if (trendData == null)
                {
                    Console.WriteLine("Failed to get trend data.");
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken); // Ждем 10 секунд
                    continue;
                }

                var trend = botInstance.PredictionModel.Predict(trendData);

                // Выполняем сделку, если позиция не открыта
                await _tradeExecutionService.ExecuteTradeAsync(botInstance, trend);

                // Подождем 10 секунд перед следующей итерацией
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }

        private async Task<TradeData> GetTrendDataAsync(BinanceRestClient client, string symbol)
        {
            var request = new RestRequest("/api/v3/klines", Method.Get);
            request.AddParameter("symbol", symbol);
            request.AddParameter("interval", "1m");

            try
            {
                var response = await client.ExecuteAsync(request, Method.Get);
                var klines = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Kline>>(response.Content);

                if (klines == null || !klines.Any())
                {
                    Console.WriteLine("No klines data returned.");
                    return null;
                }

                var latestKline = klines.Last();
                return new TradeData
                {
                    Price = decimal.Parse(latestKline.Close),
                    MovingAverage = decimal.Parse(latestKline.High),
                    Macd = decimal.Parse(latestKline.Low),
                    Signal = decimal.Parse(latestKline.Open)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching trend data: {ex.Message}");
                return null;
            }
        }


        public async Task<bool> CheckIfPositionOpenAsync(BotInstance botInstance)
        {
            var request = new RestRequest("/fapi/v2/positionRisk", Method.Get);
            request.AddParameter("symbol", botInstance.Symbol);

            var response = await botInstance.Client.ExecuteAsync(request, Method.Get, true);

            if (response.IsSuccessful)
            {
                var positions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Position>>(response.Content);
                var position = positions.FirstOrDefault(p => p.Symbol == botInstance.Symbol);
                if (position != null && position.PositionAmt != 0)
                {
                    Console.WriteLine($"Open position found for {botInstance.Symbol}");
                    return true;
                }
            }

            return false;
        }
    }

}
