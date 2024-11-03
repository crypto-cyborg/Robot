using BinanceTradingBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BinanceTradingBot.Services
{
    public class TechnicalIndicatorsService
    {
        public decimal CalculateRsi(List<decimal> prices, int period = 14)
        {
            if (prices == null || prices.Count < period)
                throw new ArgumentException("Недостаточно данных для расчета RSI");

            decimal gainSum = 0;
            decimal lossSum = 0;

           
            for (int i = 1; i <= period; i++)
            {
                var change = prices[i] - prices[i - 1];
                if (change > 0)
                    gainSum += change;
                else
                    lossSum -= change;
            }

            decimal avgGain = gainSum / period;
            decimal avgLoss = lossSum / period;

            
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

            decimal rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
            decimal rsi = 100 - (100 / (1 + rs));
            return rsi;
        }

        public decimal CalculateMacdSignal(List<decimal> prices, int shortPeriod = 12, int longPeriod = 26, int signalPeriod = 9)
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

        public decimal CalculateMovingAverage(List<decimal> prices, int period)
        {
            if (prices == null || prices.Count < period)
                throw new ArgumentException("Недостаточно данных для расчета скользящего среднего");

            return prices.TakeLast(period).Average();
        }

        public decimal CalculateExponentialMovingAverage(List<decimal> prices, int period)
        {
            if (prices == null || prices.Count < period)
                throw new ArgumentException("Недостаточно данных для расчета EMA");

            decimal smoothingFactor = 2m / (period + 1);
            decimal ema = prices.Take(period).Average();

            foreach (var price in prices.Skip(period))
            {
                ema = (price - ema) * smoothingFactor + ema;
            }

            return ema;
        }
    }
}
