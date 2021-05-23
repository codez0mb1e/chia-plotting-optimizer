using System;

namespace PlottingOptimizer.Core.Configurations
{
    public class PlottingSettings
    {
        public PlottingDirectories PlottingDirectories { get; set; }

        public PlottingComputeResources ComputeResources { get; set; }


        public string ChiaGuiVersion { get; set; }

        public string PlottingScriptPath { get; set; }

        public int PlottingLogReadingAttemptsN => 10;

        public TimeSpan PullingPeriod => TimeSpan.FromMinutes(1);

        public TimeSpan PlottingLogReadingDelay => TimeSpan.FromMilliseconds(100);
    }
}
