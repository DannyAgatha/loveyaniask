using System;
using RandN;
using RandN.Compat;
using WingsEmu.Game;

namespace WingsEmu.ClusterScheduler.Utility
{
    public class RandomGenerator : IRandomGenerator
    {
        private static readonly Random Local = RandomShim.Create(SmallRng.Create());

        public int RandomNumber(int min, int max)
        {
            if (min > max)
            {
                return RandomNumber(max, min);
            }

            return min == max ? max : Local.Next(min, max);
        }

        public int RandomNumber(int max) => RandomNumber(0, max);

        public int RandomNumber() => RandomNumber(0, 100);
        
        public double RandomNumber(double min, double max)
        {
            if (min > max)
            {
                return RandomNumber(max, min);
            }

            return Local.NextDouble() * (max - min) + min;
        }
    }
}