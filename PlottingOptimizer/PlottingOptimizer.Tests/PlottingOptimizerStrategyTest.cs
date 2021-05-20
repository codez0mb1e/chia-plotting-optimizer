using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PlottingOptimizer.Tests
{
    [TestClass]
    public class PlottingOptimizerStrategyTest
    {
        [TestMethod]
        public void CalculatePhases1OptimalCountTest()
        {
            IPlottingOptimizerStrategy strategy = new PlottingOptimizerStrategy(new());


            var emptyStats = new Dictionary<string, int>();
            var maxPhase1Stats = new Dictionary<string, int> { { "a",  1 }, { "b",  1 }, { "c",  1 }, { "d",  1 }, { "f",  1 }, { "e",  1 } };


            var actual = strategy.CalculatePhases1OptimalCount(emptyStats);

            Assert.Equals(0, actual);

        }
    }
}
