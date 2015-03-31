using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StratSim;
using StratSim.Model;

namespace StratSimTest
{
    [TestClass]
    public class StrategyUnitTest
    {
        [TestMethod]
        [DataSource(@"Provider=Microsoft.ACE.OLEDB.12.0; Data Source=C:\\Users\\Alex\\SkyDrive\\Documents\\Projects\\StratSim\\3.1.9\\Strategy Simulation V2\\Data\\StratSim.accdb;", "StrategyTestData")]
        public void TestStintLengthOptimisation()
        {
            int raceLaps = 58;
            var parameters = new[] { 320F, -0.8F, 0.1F, 0.3F, 200F, 2.5F, 0.02F, 60F };
            float pitStopLoss = 21;

            PaceParameterCollection paceParameters = new PaceParameterCollection(parameters, pitStopLoss);
            Strategy strategy = new Strategy();
            strategy.PaceParameters = paceParameters;

            int primeStints = Convert.ToInt32(TestContext.DataRow["Prime Stints"]);
            int optionStints = Convert.ToInt32(TestContext.DataRow["Option Stints"]);
            int expectedPrimeLength = Convert.ToInt32(TestContext.DataRow["Act Prime"]);
            int expectedOptionLength = Convert.ToInt32(TestContext.DataRow["Act Option"]);

            strategy.SetStintLengths(raceLaps, primeStints, optionStints);

            Assert.AreEqual(expectedPrimeLength, strategy.PrimeLaps);
            Assert.AreEqual(expectedOptionLength, strategy.OptionLaps);
        }

        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }


        [TestMethod]
        public void TestStrategyOptimisation()
        {
            int primeStints = 2;
            int optionStints = 1;
            var parameters = new[] { 320F, -0.8F, 0.1F, 0.3F, 200F, 2.5F, 0.02F, 60F };
            float pitStopLoss = 21;

            Data data = new Data();
            Driver driver = new Driver();
            PaceParameterCollection paceParameters = new PaceParameterCollection(parameters, pitStopLoss);
            driver.PaceParameters = paceParameters;
            Strategy strategy = new Strategy(primeStints + optionStints, primeStints, paceParameters, 0);

            driver.SelectedStrategy = driver.OptimiseStrategy(0, 0);

            for (int stintNo = 0; stintNo < primeStints + optionStints; stintNo++)
            {
                Assert.AreEqual(strategy.Stints[stintNo].stintLength, driver.SelectedStrategy.Stints[stintNo].stintLength);
                Assert.AreEqual(strategy.Stints[stintNo].tyreType, driver.SelectedStrategy.Stints[stintNo].tyreType);
            }
        }
    }
}
