using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StratSim.View.Panels;
using MyFlowLayout;
using DataSources.DataConnections;
using StratSim.Model.Files;
using StratSim.Model.RaceHistory;
using StratSim.Model;

namespace StratSimTest
{
    /// <summary>
    /// Summary description for RaceHistoryUnitTesting
    /// </summary>
    [TestClass]
    public class RaceHistoryUnitTesting
    {
        public RaceHistoryUnitTesting()
        {
            StratSim.Properties.Settings.Default.CurrentYear = 2015;
            var data = new Data();
            historySimulation = GetRaceHistorySimulation();
        }

        private RaceHistorySimulation historySimulation;

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestGetTextFromFile()
        {
            var fileText = GetTextFromFile();
            Assert.AreNotEqual("", fileText);
        }

        [TestMethod]
        public void TestGetDataFromText()
        {
            HistoryData raceHistoryData = GetRaceHistoryData();

            Assert.AreEqual(0, raceHistoryData.RaceLapCollection[0][0].DriverIndex);
            Assert.IsTrue(CheckIsClose(142.988F, raceHistoryData.RaceLapCollection[0][0].LapTime, 0.001F));
            Assert.AreEqual(RaceLapAction.Retire, raceHistoryData.RaceLapCollection[19][0].LapAction);
            Assert.AreEqual(2, raceHistoryData.RaceLapCollection[0][24].LapStatus);
            Assert.AreEqual(1, raceHistoryData.RaceLapCollection[6][57].LapDeficit);
        }

        [TestMethod]
        public void TestGetCumulativeTimes()
        {
            var cumulativeTimes = historySimulation.GetCumulativeTimes();

            Assert.IsTrue(CheckIsClose(142.988F, cumulativeTimes[0][0].LapTime, 0.001F));
            Assert.IsTrue(CheckIsClose(701.721F, cumulativeTimes[0][5].LapTime, 0.001F));
            Assert.IsTrue(CheckIsClose(0F, cumulativeTimes[19][0].LapTime, 0.001F));
        }

        [TestMethod]
        public void TestFindEvents()
        {
            Assert.AreEqual(RaceLapAction.Stuck, historySimulation.raceLapCollection[4][6].LapAction);
            Assert.AreEqual(RaceLapAction.Overtake, historySimulation.raceLapCollection[5][5].LapAction);
            Assert.AreEqual(RaceLapAction.Overtake, historySimulation.raceLapCollection[5][22].LapAction);
            Assert.AreNotEqual(RaceLapAction.Overtake, historySimulation.raceLapCollection[5][21].LapAction);
            Assert.AreNotEqual(RaceLapAction.Overtake, historySimulation.raceLapCollection[5][23].LapAction);
        }

        [TestMethod]
        public void TestGetPositionData()
        {
            Assert.IsTrue(CheckIsClose(0, historySimulation.positions[0, 0].gap, 0.001F));
            Assert.IsTrue(CheckIsClose(142.988F, historySimulation.positions[0, 0].cumulativeTime, 0.001F));
            Assert.IsTrue(CheckIsClose(1.362F, historySimulation.positions[1, 0].gap, 0.001F));
            Assert.IsTrue(CheckIsClose(1.362F, historySimulation.positions[1, 0].interval, 0.001F));
            Assert.IsTrue(CheckIsClose(3.125F, historySimulation.positions[2, 0].gap, 0.001F));
            Assert.IsTrue(CheckIsClose(1.763F, historySimulation.positions[2, 0].interval, 0.001F));
            Assert.AreEqual(0, historySimulation.positions[0, 0].position);
            Assert.IsTrue(CheckIsClose(701.721F, historySimulation.positions[0, 5].cumulativeTime, 0.001F));
            Assert.IsTrue(CheckIsClose(-0.5F, historySimulation.positions[19, 57].gap, 0.001F));
            Assert.AreEqual(9, historySimulation.positions[10, 57].driver);
            Assert.IsTrue(CheckIsClose(-2F, historySimulation.positions[10, 57].gap, 0.001F));
        }

        [TestMethod]
        public void TestGetStints()
        {
            var raceStints = historySimulation.GetRaceStints();
            Assert.AreEqual(25, raceStints[0][0].StintLength);
            Assert.AreEqual(25, raceStints[0][1].StartLapIndex);
            Assert.AreEqual(TyreType.Option, raceStints[1][0].TyreType);
            Assert.AreEqual(TyreType.Prime, raceStints[1][1].TyreType);
            Assert.AreEqual(34, raceStints[6][1].StintLength);
            Assert.AreEqual(34, raceStints[4][1].StintLength);
            Assert.AreEqual(1, raceStints[7].Count);
            Assert.AreEqual(1, raceStints[3].Count);
        }

        private string GetTextFromFile()
        {
            var dataInput = new DataInput(new MainForm());
            var fileText = dataInput.GetTextFromSelectedPDFFile(Session.History, 0, "AUSTRALIA");
            return fileText;
        }

        private HistoryData GetRaceHistoryData()
        {
            var fileText = GetTextFromFile();
            var dataController = new DataController();
            var dataToAnalyse = dataController.GetDataType(fileText, TimingDataType.HistoryData);
            dataToAnalyse.AnalyseData(Session.History);
            dataToAnalyse.WriteArchiveData(Session.History, 0);

            HistoryData raceHistoryData = dataToAnalyse as HistoryData;
            return raceHistoryData;
        }

        private RaceHistorySimulation GetRaceHistorySimulation()
        {
            var historyData = GetRaceHistoryData();
            var historySimulation = new RaceHistorySimulation(historyData.RaceLapCollection, 0, new MainForm());
            return historySimulation;
        }

        private bool CheckIsClose(float intended, float actual, float e)
        {
            return (actual > intended - e) && (actual < intended + e);
        }
    }
}
