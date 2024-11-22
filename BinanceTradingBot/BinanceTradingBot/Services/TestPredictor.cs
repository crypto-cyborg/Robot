using BinanceTradingBot.Models;

namespace BinanceTradingBot.Services
{
    public class TestPredictor
    {
        public TestPredictor testPredictor = new();

        public async Task StartBotAsync(BotInstance botInstance)
        {
            await botInstance.PredictionModel.InitializeOrTrainModelAsync(botInstance.Client, botInstance.Symbol);

            var multiTimeframeData = botInstance.PredictionModel.LoadMultiTimeframeDataAsync();

            var prediction = botInstance.PredictionModel.Predict(multiTimeframeData.));
        }

    }
}
