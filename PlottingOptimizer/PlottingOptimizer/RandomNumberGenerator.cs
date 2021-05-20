using System;

namespace PlottingOptimizer
{
    public static class RandomNumberGenerator
    {
        private static readonly Random Random = new Random();

        public static int RandomNumber(int min, int max) => Random.Next(min, max);
    }
}