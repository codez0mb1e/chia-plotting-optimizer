using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlottingOptimizer.Core;
using PlottingOptimizer.Core.Configurations;

namespace PlottingOptimizer.Tests
{
    [TestClass]
    public class PlottingOptimizerStrategyTest
    { 
        private static readonly Lazy<PlottingSettings> PlottingSettings = new(() =>
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.tests.json").Build();

            IConfigurationSection section = config.GetSection(nameof(PlottingSettings));
            return section.Get<PlottingSettings>();
        });

        private readonly PlottingComputeResources _settings = PlottingSettings.Value.ComputeResources;

        private readonly IPlottingOptimizerStrategy _strategy = new PlottingOptimizerStrategy(PlottingSettings.Value.ComputeResources);

        private int AvailableCores => _settings.TotalProcessorCount - _settings.OsDemandProcessorCount - _settings.ChiaDemandProcessorCount;


        [TestMethod]
        public void Test1()
        {
            var emptyStats = new Dictionary<string, int>();
            var actual = _strategy.CalculatePhases1OptimalCount(emptyStats);
            
            Assert.AreEqual(0, emptyStats.Count);
            Assert.AreEqual(_settings.Phase1MaxCount, actual);
        }


        [TestMethod]
        public void Test2()
        {
            var maxPhase1Stats = new Dictionary<string, int> { { "a", 1 }, { "b", 1 }, { "c", 1 }, { "d", 1 }, { "f", 1 }, { "e", 1 }, { "g", 1 } };
            var actual = _strategy.CalculatePhases1OptimalCount(maxPhase1Stats);

            Assert.IsTrue(maxPhase1Stats.Count(s => s.Value == 1) >= _settings.Phase1MaxCount);
            Assert.AreEqual(0, actual);
        }
        
        

        private IDictionary<string, int> MaxOtherPhases => new Dictionary<string, int>
            {
                { "11", 2 }, { "12", 3 }, { "13", 4 }, { "14", 2 }, { "15", 3 }, { "16", 4 },
                { "21", 2 }, { "22", 3 }, { "23", 4 }, { "24", 2 }, { "25", 3 }, { "26", 4 },
                { "31", 2 }, { "32", 3 }, { "33", 4 }, { "34", 2 }, { "35", 3 }, { "36", 4 }
            }
            .Take(AvailableCores)
            .ToDictionary(k => k.Key, v => v.Value);


        [TestMethod]
        public void Test3()
        {
            var actual = _strategy.CalculatePhases1OptimalCount(MaxOtherPhases);

            Assert.IsTrue(MaxOtherPhases.Count == AvailableCores);
            Assert.AreEqual(0, actual);
        }


        [TestMethod]
        public void Test4()
        {
            var allPhasesCompleted = MaxOtherPhases
                .Select(s => (s.Key, 5))
                .ToDictionary(k => k.Key, v => v.Item2);

            var actual = _strategy.CalculatePhases1OptimalCount(allPhasesCompleted);

            Assert.IsTrue(allPhasesCompleted.Count == AvailableCores);
            Assert.IsTrue(allPhasesCompleted.Count(s => s.Value < 5) == 0);
            Assert.AreEqual(_settings.Phase1MaxCount, actual);
        }

        [TestMethod]
        public void Test5()
        {
            var randomPhases = new Dictionary<string, int>
            {
                {"11", 1},
                {"21", 2}, {"22", 2},
                {"31", 3}, {"32", 3}, {"33", 3},
                {"41", 1},
                {"51", 5}, {"52", 5}, {"53", 5},
            };

            var actual = _strategy.CalculatePhases1OptimalCount(randomPhases);

            Assert.AreEqual(2, actual);
        }
    }
}
