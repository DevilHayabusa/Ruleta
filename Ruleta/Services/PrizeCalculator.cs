using Ruleta.Models;
using Ruleta.Models;
using System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ruleta.Services
{
    public class PrizeCalculator
    {
        private readonly Random _random = new Random();

        /// <summary>
        /// Divides the 360 degrees of the roulette equally among all available prizes.
        /// </summary>
        public List<PrizeConfig> CalculateSliceAngles(List<PrizeConfig> rawPrizes)
        {
            if (rawPrizes == null || rawPrizes.Count == 0)
                throw new ArgumentException("Prize list cannot be empty.");

            double degreesPerSlice = 360.0 / rawPrizes.Count;
            double currentAngle = 0;

            foreach (var prize in rawPrizes)
            {
                prize.StartAngle = currentAngle;
                currentAngle += degreesPerSlice;
                prize.EndAngle = currentAngle;
            }

            return rawPrizes;
        }

        /// <summary>
        /// Executes a weighted random selection to ensure high-value items are won less frequently,
        /// then calculates a precise angle within the winning slice.
        /// </summary>
        public PrizeResult DetermineWinningPrize(List<PrizeConfig> configuredPrizes)
        {
            int totalWeight = configuredPrizes.Sum(p => p.WinProbabilityWeight);
            int randomNumber = _random.Next(0, totalWeight);
            int currentWeight = 0;

            PrizeConfig winningPrize = null;

            foreach (var prize in configuredPrizes)
            {
                currentWeight += prize.WinProbabilityWeight;
                if (randomNumber < currentWeight)
                {
                    winningPrize = prize;
                    break;
                }
            }

            // Fallback safety net
            if (winningPrize == null)
                winningPrize = configuredPrizes.Last();

            double targetAngle = GetSafeAngleWithinSlice(winningPrize.StartAngle, winningPrize.EndAngle);

            return new PrizeResult
            {
                WinningPrize = winningPrize,
                TargetAngle = targetAngle
            };
        }

        /// <summary>
        /// Calculates a random stopping point strictly inside the slice.
        /// A 10% margin is added to both sides so the pointer doesn't stop exactly on a dividing line.
        /// </summary>
        private double GetSafeAngleWithinSlice(double startAngle, double endAngle)
        {
            double margin = (endAngle - startAngle) * 0.1;
            double minAngle = startAngle + margin;
            double maxAngle = endAngle - margin;

            return minAngle + (_random.NextDouble() * (maxAngle - minAngle));
        }
    }
}