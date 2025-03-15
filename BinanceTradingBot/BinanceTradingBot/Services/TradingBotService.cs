using BinanceTradingBot.Enums;
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
                    await botInstance.PredictionModel.InitializeOrTrainModelAsync(botInstance.Client, botInstance.Symbol);

                    while (!botInstance.CancellationTokenSource.Token.IsCancellationRequested)
                    {
                        Side side;
                        var OpenPositions = await botInstance.Client.GetOpenPositionsAsync();

                        if (OpenPositions.Count == 0)
                        {
                            await botInstance.PredictionModel.InitializeOrTrainModelAsync(botInstance.Client, botInstance.Symbol);

                            var multiTimeframeData = await botInstance.PredictionModel.LoadMultiTimeframeDataAsync();

                            var DailyPrediction = botInstance.PredictionModel.Predict(multiTimeframeData.DailyData.Last());
                            var FourHourPrediction = botInstance.PredictionModel.Predict(multiTimeframeData.FourHourData.Last());
                            var FiveMinPrediction = botInstance.PredictionModel.Predict(multiTimeframeData.FiveMinuteData.Last());
                            var OneMinPrediction = botInstance.PredictionModel.Predict(multiTimeframeData.OneMinuteData.Last());

                            if (botInstance.Client != null)
                            {
                                if (FiveMinPrediction == "long")
                                {
                                    await botInstance.Client.PlaceOrderAsync(botInstance, Side.BUY);
                                    side = Side.SELL;
                                    await botInstance.Client.SetTrailingStopAsync(botInstance);
                                    Console.WriteLine($"Executed BUY order for {botInstance.Symbol}.");
                                }
                                else if (FiveMinPrediction == "short")
                                {
                                    await botInstance.Client.PlaceOrderAsync(botInstance, Side.SELL);
                                    side = Side.BUY;
                                    await botInstance.Client.SetTrailingStopAsync(botInstance);
                                    Console.WriteLine($"Executed SELL order for {botInstance.Symbol}.");
                                }
                            }
                            else
                            {
                                if (FiveMinPrediction == "long")
                                {
                                    await botInstance.SandboxClient.PlaceOrderAsync(botInstance, Side.BUY);
                                    side = Side.SELL;
                                    await botInstance.SandboxClient.SetTrailingStopAsync(botInstance);
                                    Console.WriteLine($"Executed BUY order for {botInstance.Symbol}.");
                                }
                                else if (FiveMinPrediction == "short")
                                {
                                    await botInstance.SandboxClient.PlaceOrderAsync(botInstance, Side.SELL);
                                    side = Side.BUY;
                                    await botInstance.SandboxClient.SetTrailingStopAsync(botInstance);
                                    Console.WriteLine($"Executed SELL order for {botInstance.Symbol}.");
                                }
                            }
     

                        }
                        else
                        {
                            Console.WriteLine($"Position is already open for {botInstance.Symbol}, updating trailing stop.");

                        }
                        await Task.Delay(TimeSpan.FromMinutes(1), botInstance.CancellationTokenSource.Token);
                    }
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

}
