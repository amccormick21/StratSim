using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeamStats.Statistics;
using TeamStats.MyStatistics;
using StratSim.Model;
using TeamStats;
using DataSources.DataConnections;
using TeamStats.Functions;

namespace StatisticUnitTesting
{
    /// <summary>
    /// All data is valid for the end of the 2014 season. StratSim.Properties.Settings.CurrentYear should be set to 2014
    /// </summary>
    [TestClass]
    public class GeneralStatsUnitTesting
    {
        [TestMethod]
        public void TestSortByIndex()
        {
            var Data = new Data();
            ChampionshipStatistic statistic = new ChampionshipStatistic();

            statistic.CalculateStatistics();
            statistic.Sort(OrderType.Ascending, SortType.Index);
            //The result here depends on the database: it should be the first car in the database.
            Assert.AreEqual("HAMILTON", ((ChampionshipDataElement)statistic.DriverStats[0]).CompetitorName);
        }

        [TestMethod]
        public void TestSortByValue()
        {
            var Data = new Data();
            ChampionshipStatistic statistic = new ChampionshipStatistic();

            statistic.CalculateStatistics();
            statistic.Sort(OrderType.Descending, SortType.Value);

            Assert.AreEqual("HAMILTON", ((ChampionshipDataElement)statistic.DriverStats[0]).CompetitorName);
        }

        [TestMethod]
        public void TestDatabase()
        {
            var Data = new Data();
            var results = DataSources.DataConnections.DriverResultsTableUpdater.GetResultsFromDatabase(Session.Race, Data.NumberOfDrivers, Data.NumberOfTracks, StratSim.Properties.Settings.Default.CurrentYear, Driver.GetDriverIndexDictionary());
            var championship = StatisticManager.GetChampionships(results, PointScoringSystem.Post2009, true);
            Assert.AreEqual(384, ((ChampionshipDataElement)championship.DriverStats[0]).Points);
        }

        [TestMethod]
        public void TestPopulateChampionshipResults()
        {
            var Data = new Data();
            ChampionshipStatistic statistic = new ChampionshipStatistic();

            Result[,] results = new Result[Data.NumberOfDrivers, Data.NumberOfTracks];

            for (int i = 0; i < Data.NumberOfDrivers; i++)
            {
                //First round results:
                results[i, 0].position = (i + 1);
            }

            statistic.SetResults(results);
            statistic.CalculateStatistics();
            statistic.Sort(OrderType.Descending, SortType.Value);

            Assert.AreEqual(0, statistic.DriverStats[0].CompetitorIndex);
            Assert.AreEqual(25, ((ChampionshipDataElement)statistic.DriverStats[0]).Points);
            Assert.AreEqual(43, ((ChampionshipDataElement)statistic.TeamStats[0]).Points);
            Assert.AreEqual(1, ((ChampionshipDataElement)statistic.TeamStats[0]).NumberOfResults[0]);
        }
    }
}
