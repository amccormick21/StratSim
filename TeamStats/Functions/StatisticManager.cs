using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStats.Statistics;
using TeamStats.MyStatistics;
using StratSim.Model;
using DataSources;
using System.Data.OleDb;
using System.Data;
using TeamStats.ViewModel;
using DataSources.DataConnections;

namespace TeamStats.Functions
{
    public static class StatisticManager
    {
        public static ChampionshipStatistic GetChampionships(Result[,] results, PointScoringSystem pointSystem, bool doublePoints)
        {
            ChampionshipStatistic statistic = new ChampionshipStatistic();
            statistic.PointSystem = pointSystem;
            statistic.DoublePoints = doublePoints;

            statistic.SetResults(results);
            statistic.CalculateStatistics();
            statistic.Sort(OrderType.Descending, SortType.Value);

            return statistic;
        }

        public static event EventHandler<ResultsUpdatedEventArgs> ShowComparison;
        public static void OnShowComparison(Result[,] results, Session session)
        {
            if (ShowComparison != null)
                ShowComparison(null, new ResultsUpdatedEventArgs(results, session));
        }

        public static event EventHandler ComparisonCleared;
        public static void OnClearComparison()
        {
            if (ComparisonCleared != null)
                ComparisonCleared(null, new EventArgs());
        }

        internal static Result[,] GetResultsFromDatabase(Session session)
        {
            return DriverResultsTableUpdater.GetResultsFromDatabase(session, Data.NumberOfDrivers, Data.NumberOfTracks, StratSim.Properties.Settings.Default.CurrentYear, Driver.GetDriverIndexDictionary());
        }

        internal static void SetResults(Result[,] results, Session session)
        {
            DriverResultsTableUpdater.SetResults(results, session, Data.NumberOfDrivers, Data.NumberOfTracks, StratSim.Properties.Settings.Default.CurrentYear, Driver.GetDriverIndexDictionary(), Driver.GetDriverNumberArray());
        }
    }
}
