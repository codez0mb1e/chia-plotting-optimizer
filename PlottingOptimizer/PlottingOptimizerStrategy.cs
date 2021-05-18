using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PlottingOptimizer
{
    public interface IPlottingOptimizerStrategy
    {
        int CalculatePhases1OptimalCount(IDictionary<string, int> currentPhases);
    }


    public class PlottingOptimizerStrategy : IPlottingOptimizerStrategy
    {
        private readonly Configuration _config;

        public PlottingOptimizerStrategy(Configuration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }


        public int CalculatePhases1OptimalCount(IDictionary<string, int> currentPhases)
        {
            if (currentPhases == null) 
                throw new ArgumentNullException(nameof(currentPhases));

            int optimalCount = 0;

            var phasesStats = GetPhaseNumberToCurrentRunningProcessCount(currentPhases).ToImmutableList();
            foreach (var s in phasesStats)
                Console.WriteLine($"Phase #{s.PhaseNumber}: {s.ProcessesCount} active processes.");


            int phase1Count = phasesStats
                .SingleOrDefault(p => p.PhaseNumber == 1)
                .ProcessesCount;

            int otherPhasesCount = phasesStats
                .Where(p => p.PhaseNumber > 1 & p.PhaseNumber < 5)
                .Sum(p => p.ProcessesCount);


            int availableThreadsN = Environment.ProcessorCount - _config.OsThreadsN - _config.ChiaNetworkThreadsN;
            availableThreadsN -= (_config.Phase1ThreadsN * phase1Count + otherPhasesCount);


            optimalCount = (int)Math.Floor((decimal)(availableThreadsN/_config.Phase1ThreadsN));

            return optimalCount < _config.MaxPhase1ProcessN ? optimalCount : _config.MaxPhase1ProcessN;
        }


        private IEnumerable<(int PhaseNumber, int ProcessesCount)> GetPhaseNumberToCurrentRunningProcessCount(IDictionary<string, int> currentPhases)
        {
            if (currentPhases == null)
                throw new ArgumentNullException(nameof(currentPhases));

            return currentPhases
                .GroupBy(s => s.Value)
                .Select(s => (PhaseNumber: s.Key, ProcessesCount: s.Count()))
                .OrderBy(s => s.PhaseNumber);
        }
    }
}