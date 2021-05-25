﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using PlottingOptimizer.Core.Configurations;

namespace PlottingOptimizer.Core
{
    public interface IPlottingOptimizerStrategy
    {
        int CalculatePhases1OptimalCount(IDictionary<string, int> currentPhases);
    }


    public class PlottingOptimizerStrategy : IPlottingOptimizerStrategy
    {
        private readonly PlottingComputeResources _computeResources;

        public PlottingOptimizerStrategy(PlottingComputeResources computeResources)
        {
            _computeResources = computeResources ?? throw new ArgumentNullException(nameof(computeResources));
        }


        public int CalculatePhases1OptimalCount(IDictionary<string, int> currentPhases)
        {
            if (currentPhases == null) 
                throw new ArgumentNullException(nameof(currentPhases));

            int newPhase1Count = 0;

            var phasesStats = GetPhaseNumberToCurrentRunningProcessCount(currentPhases).ToImmutableList();
            foreach (var s in phasesStats)
                Console.WriteLine($"Phase #{s.PhaseNumber}: {s.ProcessesCount} active processes."); // TODO: replace to logger


            int phase1Count = phasesStats
                .SingleOrDefault(p => p.PhaseNumber == 1)
                .ProcessesCount;

            int otherPhasesCount = phasesStats
                .Where(p => p.PhaseNumber > 1 & p.PhaseNumber < 5)
                .Sum(p => p.ProcessesCount);


            int availableProcessorCount = _computeResources.TotalProcessorCount - _computeResources.OsDemandProcessorCount - _computeResources.ChiaDemandProcessorCount;
            availableProcessorCount -= (_computeResources.Phase1ProcessorCount * phase1Count + otherPhasesCount);


            newPhase1Count = (int)Math.Floor((decimal)(availableProcessorCount/ _computeResources.Phase1ProcessorCount));

            return newPhase1Count < _computeResources.Phase1MaxCount ? newPhase1Count : _computeResources.Phase1MaxCount;
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