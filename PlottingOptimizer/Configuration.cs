using System;
using System.Collections.Generic;

namespace PlottingOptimizer
{
    public class Configuration
    {
        public string PlotterLogsDir =>
#if DEBUG
            @"C:\Users\dmitr\.chia\mainnet\test_plotter"
#else
            @"C:\Users\dictator\.chia\mainnet\plotter"
#endif
        ;

        
        public int Phase1ThreadsN => 2;

        public int MaxPhase1ProcessN => 5;

        public string ChiaGuiVersion => "1.1.4";

        public int OsThreadsN => 1;

        public int ChiaNetworkThreadsN => 2;

        public TimeSpan PullingPeriod => TimeSpan.FromMinutes(1);
        public IReadOnlyList<string> TempDisks => new[] { "X:/", "Y:/" };
        public IReadOnlyList<string> FinalDisks => new[] { "O:/", "Q:/" };

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
