using System.Collections.Generic;

namespace PlottingOptimizer.Core.Configurations
{
    public class PlottingDirectories
    {
        public string LogDir =>
#if DEBUG
            @"C:\Users\dmitr\.chia\mainnet\test_plotter"
#else
            @"C:\Users\dictator\.chia\mainnet\plotter"
#endif
        ;

        public IReadOnlyList<string> TempDisks => new[] { "X:/", "Y:/" };

        public IReadOnlyList<string> FinalDisks => new[] { "O:/", "Q:/" };
    }
}