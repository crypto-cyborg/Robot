using BinanceTradingBot.Interfaces;
using BinanceTradingBot.Models;
using System.Collections.Concurrent;
using System.Reflection;

namespace BinanceTradingBot.Services;

public class TradingBotService : ITradingBotService
{
    private readonly ConcurrentDictionary<string, BotInstance> _bots;


    public TradingBotService()
    {

        _bots = new ConcurrentDictionary<string, BotInstance>();
    }

    public async Task StartBotAsync(BotInstance botInstance)
    {
        if (_bots.TryAdd(botInstance.ApiKey, botInstance))
        {
            botInstance.CancellationTokenSource = new CancellationTokenSource();

            await Task.Run(async () =>
            {
                try
                {
                    string side = null;
                    await botInstance.PredictionModel.InitializeOrTrainModelAsync(botInstance.Client, botInstance.Symbol);

                    // while (!botInstance.CancellationTokenSource.Token.IsCancellationRequested)
                    // {
                    //     var hasOpenPosition = await botInstance.Client.CheckOpenPositionAsync(botInstance.Symbol);
                    //
                    //     if (!hasOpenPosition)
                    //     {
                    //         await botInstance.PredictionModel.InitializeOrTrainModelAsync(botInstance.Client, botInstance.Symbol);
                    //
                    //         var multiTimeframeData = botInstance.PredictionModel.LoadMultiTimeframeData();
                    //
                    //         var prediction = botInstance.PredictionModel.Predict(tradeDataList.FirstOrDefault());
                    //
                    //         if (prediction == "long")
                    //         {
                    //             await botInstance.Client.ExecuteBuy(botInstance);
                    //             side = "BUY";
                    //             Console.WriteLine($"Executed BUY order for {botInstance.Symbol}.");
                    //         }
                    //         else if (prediction == "short")
                    //         {
                    //             await botInstance.Client.ExecuteSell(botInstance);
                    //             side = "SELL";
                    //             Console.WriteLine($"Executed SELL order for {botInstance.Symbol}.");
                    //         }
                    //     }
                    //     else
                    //     {
                    //         if (side != null)
                    //         {
                    //             Console.WriteLine($"Position is already open for {botInstance.Symbol}, updating trailing stop.");
                    //             await UpdateTrailingStopAsync(botInstance, side);
                    //         }
                    //     }
                    //     await Task.Delay(TimeSpan.FromMinutes(1), botInstance.CancellationTokenSource.Token);
                    // }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error running bot: {ex.Message}");
                }
            }, botInstance.CancellationTokenSource.Token);
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



    private async Task UpdateTrailingStopAsync(BotInstance botInstance, string side)
    {
        var currentPrice = await botInstance.Client.GetCurrentPriceAsync(botInstance.Symbol);
        var atr = await CalculateATR(botInstance, 30);

        var adjustedAtr = atr / botInstance.Leverage;

        if (side == "long")
        {
            var newStopLossPrice = currentPrice - adjustedAtr;

            if (botInstance.PreviousStopLoss == null || newStopLossPrice > botInstance.PreviousStopLoss)
            {
                await botInstance.Client.SetStopLossAsync(botInstance, newStopLossPrice);
                botInstance.PreviousStopLoss = newStopLossPrice;
            }
        }
        else if (side == "short")
        {
            var newStopLossPrice = currentPrice + adjustedAtr;

            if (botInstance.PreviousStopLoss == null || newStopLossPrice < botInstance.PreviousStopLoss)
            {
                await botInstance.Client.SetStopLossAsync(botInstance, newStopLossPrice);
                botInstance.PreviousStopLoss = newStopLossPrice;
            }
        }
    }



    private async Task<float> CalculateATR(BotInstance botInstance, int period)
    {
        var klines = await botInstance.Client.GetKlinesAsync(botInstance.Symbol, "1h", period);
        var trueRanges = new List<float>();

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
