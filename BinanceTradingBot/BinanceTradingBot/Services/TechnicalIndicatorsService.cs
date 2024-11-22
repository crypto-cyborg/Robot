using BinanceTradingBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BinanceTradingBot.Services
{
    public class TechnicalIndicatorsService
    {
        public float CalculateRsi(List<float> prices, int period = 14)
        {
            if (prices == null || prices.Count < period)
                throw new ArgumentException("Недостаточно данных для расчета RSI");

            float gainSum = 0;
            float lossSum = 0;

           
            for (int i = 1; i <= period; i++)
            {
                var change = prices[i] - prices[i - 1];
                if (change > 0)
                    gainSum += change;
                else
                    lossSum -= change;
            }

            float avgGain = gainSum / period;
            float avgLoss = lossSum / period;

            
            for (int i = period; i < prices.Count; i++)
            {
                var change = prices[i] - prices[i - 1];

                if (change > 0)
                {
                    avgGain = ((avgGain * (period - 1)) + change) / period;
                    avgLoss = (avgLoss * (period - 1)) / period;
                }
                else
                {
                    avgGain = (avgGain * (period - 1)) / period;
                    avgLoss = ((avgLoss * (period - 1)) - change) / period;
                }
            }

            var rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
            var rsi = 100 - (100 / (1 + rs));
            return rsi;
        }

        public float CalculateMacdSignal(List<float> prices, int shortPeriod = 12, int longPeriod = 26, int signalPeriod = 9)
        {
            if (prices == null || prices.Count < longPeriod)
                throw new ArgumentException("Недостаточно данных для расчета MACD");

            var shortEma = CalculateExponentialMovingAverage(prices, shortPeriod);
            var longEma = CalculateExponentialMovingAverage(prices, longPeriod);
            var macdLine = shortEma - longEma;
            
            var macdValues = prices.Select((p, index) => index >= shortPeriod ? macdLine : 0).ToList();
            var signalLine = CalculateExponentialMovingAverage(macdValues, signalPeriod);

            return signalLine;
        }

        public float CalculateMovingAverage(List<float> prices, int period)
        {
            if (prices == null || prices.Count < period)
                throw new ArgumentException("Недостаточно данных для расчета скользящего среднего");

            return prices.TakeLast(period).Average();
        }

        public float CalculateExponentialMovingAverage(List<float> prices, int period)
        {
            if (prices == null || prices.Count < period)
                throw new ArgumentException("Недостаточно данных для расчета EMA");

            var smoothingFactor = 2f / (period + 1);
            var ema = prices.Take(period).Average();

            foreach (var price in prices.Skip(period))
            {
                ema = (price - ema) * smoothingFactor + ema;
            }

            return ema;
        }
    }
}
