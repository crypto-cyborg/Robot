using BinanceTradingBot.Interfaces;
using BinanceTradingBot.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BinanceTradingBot.Services;

public class TradingBotService : ITradingBotService
{
    private readonly ITradePredictionModel _tradePredictionModel;
    private readonly ConcurrentDictionary<string, BotInstance> _bots = new ConcurrentDictionary<string, BotInstance>();
    

    public TradingBotService(ITradePredictionModel tradePredictionModel)
    {
        _tradePredictionModel = tradePredictionModel;
    }

    public async Task StartBotAsync(BotInstance botInstance)
    {
        if (_bots.TryAdd(botInstance.ApiKey, botInstance))
        {
            var hasOpenPosition = await botInstance.Client.CheckOpenPositionAsync(botInstance.Symbol);

            if (hasOpenPosition)
            {
                Console.WriteLine($"Уже есть открытая позиция для {botInstance.Symbol}. Бот не запущен.");
                return;
            }

            var historicalData = await LoadMultiTimeframeData(botInstance);

            _tradePredictionModel.LoadOrTrainModel(historicalData);

            foreach (var tradeData in historicalData.DailyData)
            {
                var prediction = _tradePredictionModel.Predict(tradeData);

                if (prediction == "long")
                {
                    botInstance.Client.ExecuteBuy(tradeData);
                }
                else if (prediction == "short")
                {
                    botInstance.Client.ExecuteSell(tradeData);
                }
            }
        }
        else
        {
            Console.WriteLine("Bot with this API key is already running.");
            return;
        }
       
    }



    public void StopBot(string apiKey)
    {
        if (_bots.TryGetValue(apiKey, out var botInstance))
        {            
            botInstance.CancellationTokenSource?.Cancel();           
            _bots.TryRemove(apiKey, out _);            
            botInstance.CancellationTokenSource.Dispose();
        }
        else
        {
            Console.WriteLine($"No bot found with API key: {apiKey}");
        }
    }

    private async Task<MultiTimeframeData> LoadMultiTimeframeData(BotInstance botInstance)
    {
        var klines1d = await botInstance.Client.GetKlinesAsync(botInstance.Symbol, "1d", 100);
        var klines4h = await botInstance.Client.GetKlinesAsync(botInstance.Symbol, "4h", 100);
        var klines5m = await botInstance.Client.GetKlinesAsync(botInstance.Symbol, "5m", 100);
        var klines1m = await botInstance.Client.GetKlinesAsync(botInstance.Symbol, "1m", 100);

        var multiTimeframeData = new MultiTimeframeData
        {
            DailyData = klines1d.Select(k => new TradeData
            {
                Price = k.Close,
                MovingAverage = (k.High + k.Low) / 2,
                Macd = k.High - k.Low,
                Signal = k.Close,
                Trend = k.Close > k.Open ? "long" : "short",
                Symbol = botInstance.Symbol
            }).ToList(),

            FourHourData = klines4h.Select(k => new TradeData
            {
                Price = k.Close,
                MovingAverage = (k.High + k.Low) / 2,
                Macd = k.High - k.Low,
                Signal = k.Close,
                Trend = k.Close > k.Open ? "long" : "short",
                Symbol = botInstance.Symbol
            }).ToList(),

            FiveMinuteData = klines5m.Select(k => new TradeData
            {
                Price = k.Close,
                MovingAverage = (k.High + k.Low) / 2,
                Macd = k.High - k.Low,
                Signal = k.Close,
                Trend = k.Close > k.Open ? "long" : "short",
                Symbol = botInstance.Symbol
            }).ToList(),

            OneMinuteData = klines1m.Select(k => new TradeData
            {
                Price = k.Close,
                MovingAverage = (k.High + k.Low) / 2,
                Macd = k.High - k.Low,
                Signal = k.Close,
                Trend = k.Close > k.Open ? "long" : "short",
                Symbol = botInstance.Symbol
            }).ToList()
        };

        return multiTimeframeData;
    }

    private async Task UpdateTrailingStopAsync(BotInstance botInstance, string trend)
    {
        var currentPrice = await botInstance.Client.GetCurrentPriceAsync(botInstance.Symbol);
        var atr = await CalculateATR(botInstance, 14);

        if (trend == "long")
        {
            var stopLossPrice = currentPrice - atr;
            await botInstance.Client.SetStopLossAsync(botInstance, stopLossPrice);
        }
        else if (trend == "short")
        {
            // Логика для короткой позиции
            var stopLossPrice = currentPrice + atr;
            await botInstance.Client.SetStopLossAsync(botInstance, stopLossPrice);
        }
    }
 
    private async Task<decimal> CalculateATR(BotInstance botInstance, int period)
    {
        var klines = await botInstance.Client.GetKlinesAsync( botInstance.Symbol, "1d", period);
        var trueRanges = new List<decimal>();

        for (int i = 1; i < klines.Count; i++)
        {
            var previousClose = klines[i - 1].Close;
            var highLow = klines[i].High - klines[i].Low;
            var highClose = Math.Abs(klines[i].High - previousClose);
            var lowClose = Math.Abs(klines[i].Low - previousClose);

            var trueRange = Math.Max(highLow, Math.Max(highClose, lowClose));
            trueRanges.Add(trueRange);
        }

        return trueRanges.Average();
    }
}
