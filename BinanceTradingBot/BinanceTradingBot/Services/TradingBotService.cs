using BinanceTradingBot.Interfaces;
using BinanceTradingBot.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BinanceTradingBot.Services
{
    public class TradingBotService : ITradingBotService
    {
        private readonly ITradePredictionModel _tradePredictionModel;
        private readonly ITradeExecutionService _tradeExecutionService;
        private readonly BinanceRestClient _binanceRestClient;

        public TradingBotService(ITradePredictionModel tradePredictionModel, ITradeExecutionService tradeExecutionService)
        {
            _tradePredictionModel = tradePredictionModel;
            _tradeExecutionService = tradeExecutionService;
        }

        public async Task StartBot(BotInstance botInstance)
        {
            var historicalData = await LoadMultiTimeframeData(botInstance.Symbol);

            _tradePredictionModel.LoadOrTrainModel(historicalData);

            foreach (var tradeData in historicalData.DailyData)
            {
                var prediction = _tradePredictionModel.Predict(tradeData);

                if (prediction == "long")
                {
                    _tradeExecutionService.ExecuteBuy(botInstance, tradeData);
                }
                else if (prediction == "short")
                {
                    _tradeExecutionService.ExecuteSell(botInstance, tradeData);
                }
            }
        }

        public void StopBot(BotInstance botInstance)
        {
            if (botInstance.CancellationTokenSource != null)
            {
                botInstance.CancellationTokenSource.Cancel();
                Console.WriteLine($"Bot for {botInstance.Symbol} stopped.");
            }
            else
            {
                Console.WriteLine("BotInstance has no active cancellation token.");
            }
        }

        public async Task<MultiTimeframeData> LoadMultiTimeframeData(string symbol)
        {
            var klines1d = await _binanceRestClient.GetKlinesAsync(symbol, "1d", 100);
            var klines4h = await _binanceRestClient.GetKlinesAsync(symbol, "4h", 100);
            var klines5m = await _binanceRestClient.GetKlinesAsync(symbol, "5m", 100);
            var klines1m = await _binanceRestClient.GetKlinesAsync(symbol, "1m", 100);

            var multiTimeframeData = new MultiTimeframeData
            {
                DailyData = klines1d.Select(k => new TradeData
                {
                    Price = k.Close,
                    MovingAverage = (k.High + k.Low) / 2,
                    Macd = k.High - k.Low,
                    Signal = k.Close,
                    Trend = k.Close > k.Open ? "long" : "short",
                    Symbol = symbol
                }).ToList(),

                FourHourData = klines4h.Select(k => new TradeData
                {
                    Price = k.Close,
                    MovingAverage = (k.High + k.Low) / 2,
                    Macd = k.High - k.Low,
                    Signal = k.Close,
                    Trend = k.Close > k.Open ? "long" : "short",
                    Symbol = symbol
                }).ToList(),

                FiveMinuteData = klines5m.Select(k => new TradeData
                {
                    Price = k.Close,
                    MovingAverage = (k.High + k.Low) / 2,
                    Macd = k.High - k.Low,
                    Signal = k.Close,
                    Trend = k.Close > k.Open ? "long" : "short",
                    Symbol = symbol
                }).ToList(),

                OneMinuteData = klines1m.Select(k => new TradeData
                {
                    Price = k.Close,
                    MovingAverage = (k.High + k.Low) / 2,
                    Macd = k.High - k.Low,
                    Signal = k.Close,
                    Trend = k.Close > k.Open ? "long" : "short",
                    Symbol = symbol
                }).ToList()
            };

            return multiTimeframeData;
        }
    }
}
