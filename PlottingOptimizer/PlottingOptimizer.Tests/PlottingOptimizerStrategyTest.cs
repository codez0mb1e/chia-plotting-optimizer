using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlottingOptimizer.Core;
using PlottingOptimizer.Core.Configurations;

namespace PlottingOptimizer.Tests
{
    [TestClass]
    public class PlottingOptimizerStrategyTest
    {
        [TestMethod]
        public void CalculatePhases1OptimalCountTest()
        {
            // Init
            var config = new Configuration();
            var settings = config.ComputeResources;
            IPlottingOptimizerStrategy strategy = new PlottingOptimizerStrategy(config.ComputeResources);

            int availableCores = settings.TotalProcessorCount - settings.OsDemandProcessorCount - settings.ChiaDemandProcessorCount;        


            // 1.
            var emptyStats = new Dictionary<string, int>();
            var actual = strategy.CalculatePhases1OptimalCount(emptyStats);
            
            Assert.AreEqual(0, emptyStats.Count);
            Assert.AreEqual(settings.Phase1MaxProcessorCount, actual);


            // 2.
            var maxPhase1Stats = new Dictionary<string, int> { { "a",  1 }, { "b",  1 }, { "c",  1 }, { "d",  1 }, { "f",  1 }, { "e",  1 } };
            actual = strategy.CalculatePhases1OptimalCount(maxPhase1Stats);
            
            Assert.IsTrue(maxPhase1Stats.Count(s => s.Value == 1) >= settings.Phase1MaxProcessorCount);
            Assert.AreEqual(0, actual);


            // 3.
            var maxOtherPhases = new Dictionary<string, int>
            {
                { "11", 2 }, { "12", 3 }, { "13", 4 }, { "14", 2 }, { "15", 3 }, { "16", 4 },
                { "21", 2 }, { "22", 3 }, { "23", 4 }, { "24", 2 }, { "25", 3 }, { "26", 4 },
                { "31", 2 }, { "32", 3 }, { "33", 4 }, { "34", 2 }, { "35", 3 }, { "36", 4 }
            }
                .Take(availableCores)
                .ToDictionary(k => k.Key, v => v.Value);

            actual = strategy.CalculatePhases1OptimalCount(maxOtherPhases);

            Assert.IsTrue(maxOtherPhases.Count == availableCores);
            Assert.AreEqual(0, actual);


            // 4.
            var allPhasesCompleted = maxOtherPhases
                .Select(s => (s.Key, 5))
                .ToDictionary(k => k.Key, v => v.Item2);

            actual = strategy.CalculatePhases1OptimalCount(allPhasesCompleted);

            Assert.IsTrue(allPhasesCompleted.Count == availableCores);
            Assert.IsTrue(allPhasesCompleted.Count(s => s.Value < 5) == 0);
            Assert.AreEqual(settings.Phase1MaxProcessorCount, actual);

            // 5.
            var randomPhases = new Dictionary<string, int>
            {
                {"11", 1}, 
                {"21", 2}, {"22", 2},
                {"31", 3}, {"32", 3}, {"33", 3},
                {"41", 1},
                {"51", 5}, {"52", 5}, {"53", 5},
            };

            actual = strategy.CalculatePhases1OptimalCount(randomPhases);

            Assert.AreEqual(2, actual);
        }
    }
}
