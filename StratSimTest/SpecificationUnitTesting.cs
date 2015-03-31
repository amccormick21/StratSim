using Microsoft.VisualStudio.TestTools.UnitTesting;

using StratSim;
using StratSim.Model;
using StratSim.Model.Files;
using StratSim.View.MyFlowLayout;
using StratSim.View.Panels;
using System.Collections.Generic;
using DataSources;
using StratSim.Model.CalculationControllers;
using MyFlowLayout;

namespace StratSimTest
{
    public class SpecificationUnitTesting
    {
        [TestClass]
        public class DataInputUnitTesting
        {
            [TestMethod]
            public void TestSetSpeedDataFromFile()
            {
                string lineOfData = "0,VETTEL,299.243";
                var data = new Data();
                var speedData = new StratSim.Model.Files.SpeedData();

                speedData.SetDriverSpeed(lineOfData);

                Assert.AreEqual(299.243F, speedData.TopSpeeds[0]);
            }

            [TestMethod]
            public void TestGetLapTimeFromString()
            {
                string lapTimeString = "1:32.554 ";
                float lapTime;

                var lapData = new LapData();

                lapTime = lapData.GetLapTime(lapTimeString);

                Assert.AreEqual(92.554F, lapTime);
            }
        }

        [TestClass]
        public class OvertakeUnitTesting
        {
            [TestMethod]
            public void TestOvertakeProbabilityShouldOccur()
            {
                float paceDelta = 0.8F;
                float speedDelta = 20F;
                float totalDelta = -0.4F;
                float requiredPaceDelta = 0.4F;
                float requiredSpeedDelta = 10F;

                var race = new Race();

                float overtakeProbability = race.GetOvertakeProbability(paceDelta, speedDelta, totalDelta, requiredPaceDelta, requiredSpeedDelta);

                Assert.AreEqual(1F, overtakeProbability);
            }

            [TestMethod]
            public void TestOvertakeProbabilityMightOccur()
            {
                float paceDelta = 0.6F;
                float speedDelta = 15F;
                float totalDelta = 0.2F;
                float requiredPaceDelta = 0.4F;
                float requiredSpeedDelta = 10F;

                var race = new Race();

                float overtakeProbability = race.GetOvertakeProbability(paceDelta, speedDelta, totalDelta, requiredPaceDelta, requiredSpeedDelta);

                Assert.AreEqual(0.25F, overtakeProbability, 0.001);
            }

            [TestMethod]
            public void TestOvertakeProbabilityShouldNotOccur()
            {
                float paceDelta = 0.4F;
                float speedDelta = 10F;
                float totalDelta = 0.2F;
                float requiredPaceDelta = 0.4F;
                float requiredSpeedDelta = 10F;

                var race = new Race();

                float overtakeProbability = race.GetOvertakeProbability(paceDelta, speedDelta, totalDelta, requiredPaceDelta, requiredSpeedDelta);

                Assert.AreEqual(0F, overtakeProbability);
            }
        }

        [TestClass]
        public class RaceUnitTesting
        {
            Race SetupARaceFromFiles(int raceIndex)
            {
                MainFormCollection FormCollection = new MainFormCollection();
                FormCollection.AddForm(StratSim.Program.StartProject());

                StratSim.Program.InfoPanel = new InfoPanel(FormCollection[0]);
                CalculationController.PopulateDriverDataFromFiles(raceIndex);
                CalculationController.CalculatePaceParameters();
                CalculationController.OptimiseAllStrategies(raceIndex);
                CalculationController.SetRaceStrategies();
                RaceStrategy[] strategies = new RaceStrategy[Data.NumberOfDrivers];

                for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
                {
                    strategies[driverIndex] = Data.Drivers[driverIndex].RaceStrategy;
                }

                return new Race(raceIndex, strategies, FormCollection[0]);
            }

            RaceStrategy[] SetupStrategiesForPitStopSimulation(int trackIndex, int numberOfStrategies)
            {
                var data = new Data();
                Driver[] drivers = new Driver[numberOfStrategies];
                PaceParameterCollection[] paceParameters = new PaceParameterCollection[numberOfStrategies];
                Strategy[] strategies = new Strategy[numberOfStrategies];
                RaceStrategy[] raceStrategies = new RaceStrategy[numberOfStrategies];
                MainForm form = new MainForm();
                var parameters = new[] { 320F, -0.8F, 0.1F, 0.3F, 200F, 2.5F, 0.02F, 60F };
                float pitStopLoss = 21;

                for (int strategyIndex = 0; strategyIndex < numberOfStrategies; strategyIndex++)
                {
                    paceParameters[strategyIndex] = new PaceParameterCollection(parameters, pitStopLoss);
                    drivers[strategyIndex] = new Driver();
                    drivers[strategyIndex].PaceParameters = paceParameters[strategyIndex];
                    drivers[strategyIndex].SetDriverIndex(strategyIndex);
                    strategies[strategyIndex] = drivers[strategyIndex].OptimiseStrategy(strategyIndex, trackIndex);
                    raceStrategies[strategyIndex] = new RaceStrategy(strategies[strategyIndex], strategyIndex);
                }

                return raceStrategies;
            }

            RaceStrategy[] SetupRaceForPitStopSimulation(RaceStrategy[] strategies, float[] cumulativeTimes)
            {
                RaceStrategy[] tempStrategies = strategies;
                for (int i = 0; i < strategies.Length; i++)
                {
                    tempStrategies[i].CumulativeTime = cumulativeTimes[i];
                }

                return tempStrategies;
            }

            RaceStrategy[] TestPitStopSimulation(int[] driversToPit, float[] cumulativeTimes)
            {
                float pitStopLoss = 21F;

                int numberOfStrategies = 3;
                int trackIndex = 14;
                RaceStrategy[] strategies = SetupStrategiesForPitStopSimulation(trackIndex, numberOfStrategies);
                strategies = SetupRaceForPitStopSimulation(strategies, cumulativeTimes);
                List<PitStop> pitstops = new List<PitStop>();
                foreach (int driver in driversToPit)
                {
                    pitstops.Add(new PitStop(driver, 1, pitStopLoss));
                }

                PitStop.UpdateRacePositionsAfterPitStop(ref strategies, pitstops);

                return strategies;
            }

            [TestMethod]
            public void TestPitStopSimulationNoOvertakesLastDriver()
            {
                //Arrange
                int[] driverToPit = new[] { 2 };
                float[] cumulativeTimes = new[] { 100F, 110F, 140F };

                //Act
                RaceStrategy[] strategies = TestPitStopSimulation(driverToPit, cumulativeTimes);

                //Assert
                Assert.AreEqual(0, strategies[0].DriverIndex);
                Assert.AreEqual(1, strategies[1].DriverIndex);
                Assert.AreEqual(2, strategies[2].DriverIndex);
            }

            [TestMethod]
            public void TestPitStopSimulationNoOvertakesFirstDriver()
            {
                //Arrange
                int[] driverToPit = new[] { 0 };
                float[] cumulativeTimes = new[] { 100F, 130F, 140F };

                //Act
                RaceStrategy[] strategies = TestPitStopSimulation(driverToPit, cumulativeTimes);

                //Assert
                Assert.AreEqual(0, strategies[0].DriverIndex);
                Assert.AreEqual(1, strategies[1].DriverIndex);
                Assert.AreEqual(2, strategies[2].DriverIndex);
            }

            [TestMethod]
            public void TestPitStopSimulationFirstDropsToLast()
            {
                //Arrange
                int[] driverToPit = new[] { 0 };
                float[] cumulativeTimes = new[] { 100F, 110F, 120F };

                //Act
                RaceStrategy[] strategies = TestPitStopSimulation(driverToPit, cumulativeTimes);

                //Assert
                Assert.AreEqual(1, strategies[0].DriverIndex);
                Assert.AreEqual(2, strategies[1].DriverIndex);
                Assert.AreEqual(0, strategies[2].DriverIndex);
            }

            [TestMethod]
            public void TestPitStopSimulationMiddleDropsToLast()
            {
                //Arrange
                int[] driverToPit = new[] { 1 };
                float[] cumulativeTimes = new[] { 100F, 110F, 120F };

                //Act
                RaceStrategy[] strategies = TestPitStopSimulation(driverToPit, cumulativeTimes);

                //Assert
                Assert.AreEqual(0, strategies[0].DriverIndex);
                Assert.AreEqual(2, strategies[1].DriverIndex);
                Assert.AreEqual(1, strategies[2].DriverIndex);
            }
            [TestMethod]
            public void TestPitStopSimulationAllPit()
            {
                //Arrange
                int[] driverToPit = new[] { 0, 1, 2 };
                float[] cumulativeTimes = new[] { 100F, 110F, 120F };

                //Act
                RaceStrategy[] strategies = TestPitStopSimulation(driverToPit, cumulativeTimes);

                //Assert
                Assert.AreEqual(0, strategies[0].DriverIndex);
                Assert.AreEqual(1, strategies[1].DriverIndex);
                Assert.AreEqual(2, strategies[2].DriverIndex);
            }
            public void TestPitStopSimulationUndercut()
            {
                //Arrange
                float[] cumulativeTimes = new[] { 100F, 110F, 140F };

                int[] driverToPit = new[] { 0 };
                RaceStrategy[] strategies = TestPitStopSimulation(driverToPit, cumulativeTimes);

                driverToPit = new[] { 1 };
                cumulativeTimes[0] = strategies[1].CumulativeTime;
                cumulativeTimes[1] = strategies[0].CumulativeTime;
                cumulativeTimes[2] = strategies[2].CumulativeTime;
                strategies = TestPitStopSimulation(driverToPit, cumulativeTimes);

                //Assert
                Assert.AreEqual(0, strategies[0].Driver.DriverIndex);
                Assert.AreEqual(1, strategies[1].Driver.DriverIndex);
                Assert.AreEqual(2, strategies[2].Driver.DriverIndex);
            }
        }

        [TestClass]
        public class FunctionsUnitTesting
        {
            [TestMethod]
            public void TestQuickSortAscendingAlreadySorted()
            {
                int[] ints = { 1, 2, 3, 4, 5, 6, 7 };

                Functions.QuickSort<int>(ref ints, 0, ints.Length - 1, (a, b) => a > b);

                bool ascending = true;
                for (int i = 0; i < ints.Length - 1; i++)
                {
                    if (ints[i] > ints[i + 1]) { ascending = false; }
                }

                Assert.IsTrue(ascending);
            }

            [TestMethod]
            public void TestQuickSortAscending()
            {
                int[] ints = { 25, 16, 14, 16, 99, 64, 24, 58, 67, 9, 9 };

                Functions.QuickSort<int>(ref ints, 0, ints.Length - 1, (a, b) => a > b);

                bool ascending = true;
                for (int i = 0; i < ints.Length - 1; i++)
                {
                    if (ints[i] > ints[i + 1]) { ascending = false; }
                }

                Assert.IsTrue(ascending);
            }

            [TestMethod]
            public void TestInsertionSortShortAscending()
            {
                int[] ints = { 25, 16, 14 };

                Functions.InsertionSort<int>(ref ints, 0, ints.Length - 1, (a, b) => a > b);

                bool ascending = true;
                for (int i = 0; i < ints.Length - 1; i++)
                {
                    if (ints[i] > ints[i + 1]) { ascending = false; }
                }

                Assert.IsTrue(ascending);
            }

            [TestMethod]
            public void TestInsertionSortLongAscending()
            {
                int[] ints = { 25, 16, 14, 16, 99, 64, 24, 58, 67, 9, 9 };

                Functions.InsertionSort<int>(ref ints, 0, ints.Length - 1, (a, b) => a > b);

                bool ascending = true;
                for (int i = 0; i < ints.Length - 1; i++)
                {
                    if (ints[i] > ints[i + 1]) { ascending = false; }
                }

                Assert.IsTrue(ascending);
            }


            [TestMethod]
            public void TestQuickSortDescending()
            {
                int[] ints = { 25, 16, 14, 16, 99, 64, 24, 58, 67, 9, 9 };

                Functions.QuickSort<int>(ref ints, 0, ints.Length - 1, (a, b) => a < b);

                bool descending = true;
                for (int i = 0; i < ints.Length - 1; i++)
                {
                    if (ints[i] < ints[i + 1]) { descending = false; }
                }

                Assert.IsTrue(descending);
            }

            [TestMethod]
            public void TestBinarySearchLongArray()
            {
                int[] ints = { 25, 16, 14, 16, 99, 64, 24, 58, 67, 9, 9 };
                int indexFound = 0;
                int expectedIndexFound = 8;

                bool found = ints.Exists(64, false, out indexFound);

                Assert.IsTrue(found);
                Assert.AreEqual(expectedIndexFound, indexFound);
            }

            [TestMethod]
            public void TestBinarySearchLongArrayAlreadySorted()
            {
                int[] ints = { 9, 9, 16, 24, 25, 37, 64, 67, 99 };
                int indexFound = 0;
                int expectedIndexFound = 3;

                bool found = ints.Exists(24, true, out indexFound);

                Assert.IsTrue(found);
                Assert.AreEqual(expectedIndexFound, indexFound);
            }

            [TestMethod]
            public void TestBinarySearchShortArray()
            {
                int[] ints = { 58, 67, 9, 10 };
                int indexFound = 0;
                int expectedIndexFound = 0;

                bool found = ints.Exists(9, false, out indexFound);

                Assert.IsTrue(found);
                Assert.AreEqual(expectedIndexFound, indexFound);
            }

            [TestMethod]
            public void TestBinarySearchNotPresent()
            {
                int[] ints = { 58, 67, 9, 10 };
                int indexFound = 0;

                bool found = ints.Exists(11, false, out indexFound);

                Assert.IsFalse(found);
            }

            [TestMethod]
            public void TestBinarySearchZeroLength()
            {
                int[] ints = { };
                int indexFound = 0;

                bool found = ints.Exists(10, false, out indexFound);

                Assert.IsFalse(found);
            }
        }
    }
}
