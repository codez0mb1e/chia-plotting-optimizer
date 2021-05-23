using System.Collections.Generic;

namespace PlottingOptimizer.Core.Configurations
{
    public class PlottingDirectories
    {
        public string LogDir { get; set; }

        public IReadOnlyList<string> TempPathList { get; set; }

        public IReadOnlyList<string> FinalPathList { get; set; }
    }
}