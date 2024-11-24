using BinanceTradingBot.Models;

namespace BinanceTradingBot.Services
{
    public class TestPredictor
    {
        public TestPredictor testPredictor = new();

        public async Task Execute(BotInstance botInstance)
        {
            await botInstance.PredictionModel.InitializeOrTrainModelAsync(botInstance.Client, botInstance.Symbol);

            var multiTimeframeData = await botInstance.PredictionModel.LoadMultiTimeframeDataAsync();

            var DailyPrediction = botInstance.PredictionModel.Predict(multiTimeframeData.DailyData.Last());
            var FourHourPrediction = botInstance.PredictionModel.Predict(multiTimeframeData.FourHourData.Last());
            var FiveMinPrediction = botInstance.PredictionModel.Predict(multiTimeframeData.FiveMinuteData.Last());
            var OneMinPrediction = botInstance.PredictionModel.Predict(multiTimeframeData.OneMinuteData.Last());
            Console.WriteLine($"DailyPrediction: {DailyPrediction} \n FourHourPrediction: {FourHourPrediction} \n FiveMinPrediction: {FiveMinPrediction}, OneMinPrediction: {OneMinPrediction}");
        }

    }
}
