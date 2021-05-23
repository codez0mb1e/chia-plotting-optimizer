using System;

namespace PlottingOptimizer.Core.Configurations
{
    public class Configuration
    {
        public PlottingDirectories PlottingDirectories => new();

        public PlottingComputeResources ComputeResources => new();



        public string ChiaGuiVersion => "1.1.4";



        public TimeSpan PullingPeriod => TimeSpan.FromMinutes(1);

        public string PlottingScriptPath =>
#if DEBUG
            "scripts/run_plotting.debug.ps1"
#else
            "scripts/run_plotting.ps1"
#endif
        ;

        public int PlottingLogReadingAttemptsN => 10;
        
        public TimeSpan PlottingLogReadingDelay => TimeSpan.FromMilliseconds(100);
    }
}
