using System;

namespace PlottingOptimizer
{
    internal static class RandomNumberGenerator
    {
        private static readonly Random Random = new Random();

        public static int RandomNumber(int min, int max) => Random.Next(min, max);
    }
}