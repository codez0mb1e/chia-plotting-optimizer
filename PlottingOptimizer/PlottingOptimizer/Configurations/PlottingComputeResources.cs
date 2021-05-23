using System;

namespace PlottingOptimizer.Core.Configurations
{
    public class PlottingComputeResources
    {
        public int TotalProcessorCount { get; } = 16; // TODO: auto detect CPU number: Environment.ProcessorCount;

        public int OsDemandProcessorCount { get; } = 1;

        public int ChiaDemandProcessorCount { get; } = 2;

        public int Phase1ProcessorCount { get; } = 2;

        public int Phase1MaxProcessorCount { get; } = 5;
    }
}