using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StratSim.Model;
using TeamStats.MyStatistics;
using TeamStats.Functions;
using DataSources.DataConnections;

namespace TeamStats.ViewModel
{
    /// <summary>
    /// Provides methods for linking championship data to the championship panel.
    /// </summary>
    class ChampionshipData
    {
        Result[,] results;
        internal ChampionshipStatistic Statistic { get; private set; }

        PointScoringSystem pointSystem;
        bool doublePoints;
        Session session;

        internal event EventHandler<ChampionshipStatistic> ChampionshipsModified;
        internal event EventHandler<Session> SessionChanged;

        public ChampionshipData(PointScoringSystem PointSystem, bool DoublePoints, Session session)
        {
            this.pointSystem = PointSystem;
            this.doublePoints = DoublePoints;
            this.session = session;

            DriverResultsTableUpdater.DatabaseModified += DriverResultsTableUpdater_DatabaseModified;
            StatisticManager.ShowComparison += StatisticManager_ResultsUpdated;
            StatisticManager.ComparisonCleared += StatisticManager_ComparisonCleared;
            DataLoadedFromDatabase += ChampionshipData_DataLoaded;
            GetResultsFromData(session);

            PointSystemChanged += ChampionshipData_PointSystemChanged;
            DoublePointsChanged += ChampionshipData_DoublePointsChanged;
        }

        void DriverResultsTableUpdater_DatabaseModified(object sender, ResultsUpdatedEventArgs e)
        {
            this.session = e.session;
            if (SessionChanged != null)
                SessionChanged(this, session);
            GetResultsFromData(e.session);
        }

        internal Session GetSession()
        {
            return session;
        }

        public void SetSession(Session session)
        {
            ChampionshipStatistic originalStatistic = Statistic;
            this.session = session;
            if (SessionChanged != null)
                SessionChanged(this, session);
            GetResultsFromData(session);
            RunResultsComparison(results, session, originalStatistic, Statistic);
        }

        internal event EventHandler ComparisonCleared;
        void StatisticManager_ComparisonCleared(object sender, EventArgs e)
        {
            if (ComparisonCleared != null)
                ComparisonCleared(this, new EventArgs());
        }

        void StatisticManager_ResultsUpdated(object sender, ResultsUpdatedEventArgs e)
        {
            ChampionshipStatistic newStatistic = StatisticManager.GetChampionships(e.results, PointSystem, DoublePoints);
            RunResultsComparison(e.results, e.session, Statistic, newStatistic);
        }

        internal event EventHandler<int[]> DriverDeltasUpdated;
        internal event EventHandler<int[]> TeamDeltasUpdated;
        private void RunResultsComparison(Result[,] results, Session session, ChampionshipStatistic originalStatistic, ChampionshipStatistic newStatistic)
        {
            if (this.session != session) //If the original data needs to be updated
            {
                this.session = session;
                if (SessionChanged != null)
                    SessionChanged(this, session);
                GetResultsFromData(session);
                originalStatistic = Statistic;
            }

            int[] driverDeltasByIndex = ChampionshipStatistic.GetDriverDeltasFrom(originalStatistic, Statistic);
            int[] teamDeltasByIndex = ChampionshipStatistic.GetTeamDeltasFrom(originalStatistic, Statistic);

            this.results = results;
            if (ChampionshipsModified != null)
                ChampionshipsModified(this, newStatistic);
            if (TeamDeltasUpdated != null)
                TeamDeltasUpdated(this, teamDeltasByIndex);
            if (DriverDeltasUpdated != null)
                DriverDeltasUpdated(this, driverDeltasByIndex);
        }

        void ChampionshipData_DataLoaded(object sender, EventArgs e)
        {
            if (ChampionshipsModified != null)
                ChampionshipsModified(this, Statistic);
            if (ComparisonCleared != null)
                ComparisonCleared(this, new EventArgs());
        }

        internal event EventHandler DataLoadedFromDatabase;
        private void GetResultsFromData(Session session)
        {
            results = StatisticManager.GetResultsFromDatabase(session);
            Statistic = StatisticManager.GetChampionships(results, PointSystem, DoublePoints);
            if (DataLoadedFromDatabase != null)
                DataLoadedFromDatabase(this, new EventArgs());
        }

        void ChampionshipData_DoublePointsChanged(object sender, EventArgs e)
        {
            ChampionshipStatistic newStatistic = StatisticManager.GetChampionships(results, PointSystem, DoublePoints);
            RunResultsComparison(results, session, Statistic, newStatistic);
            if (ChampionshipsModified != null)
                ChampionshipsModified(this, newStatistic);
        }

        void ChampionshipData_PointSystemChanged(object sender, EventArgs e)
        {
            ChampionshipStatistic newStatistic = StatisticManager.GetChampionships(results, PointSystem, DoublePoints);
            RunResultsComparison(results, session, Statistic, newStatistic);
            if (ChampionshipsModified != null)
                ChampionshipsModified(this, newStatistic);
        }

        internal int[] GetDriverData()
        {
            int[] driverChampionship = new int[Data.NumberOfDrivers];

            ChampionshipStatistic statistic = StatisticManager.GetChampionships(results, pointSystem, doublePoints);

            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                driverChampionship[driverIndex] = ((ChampionshipDataElement)statistic.DriverStats[driverIndex]).Points;
            }

            return driverChampionship;
        }

        internal int[] GetTeamData()
        {
            int[] teamChampionship = new int[Data.NumberOfDrivers / 2];

            ChampionshipStatistic statistic = StatisticManager.GetChampionships(results, pointSystem, doublePoints);

            for (int teamIndex = 0; teamIndex < Data.NumberOfDrivers / 2; teamIndex++)
            {
                teamChampionship[teamIndex] = ((ChampionshipDataElement)statistic.TeamStats[teamIndex]).Points;
            }

            return teamChampionship;
        }

        internal event EventHandler PointSystemChanged;
        internal PointScoringSystem PointSystem
        {
            get { return pointSystem; }
            set { pointSystem = value; PointSystemChanged(this, new EventArgs()); }
        }
        internal event EventHandler DoublePointsChanged;
        internal bool DoublePoints
        {
            get { return doublePoints; }
            set { doublePoints = value; DoublePointsChanged(this, new EventArgs()); }
        }

        internal Driver GetDriverAtPosition(ChampionshipStatistic statistic, int driverPosition)
        {
            return Data.Drivers[statistic.DriverStats[driverPosition].CompetitorIndex];
        }

        internal Team GetTeamAtPosition(ChampionshipStatistic statistic, int teamPosition)
        {
            return Data.Teams[statistic.TeamStats[teamPosition].CompetitorIndex];
        }

        internal int GetDriverPointsDelta(ChampionshipStatistic statistic, int driverIndex)
        {
            if (driverIndex == 0)
            {
                return -1;
            }
            else
            {
                return (GetDriverPoints(statistic, driverIndex) - GetDriverPoints(statistic, driverIndex - 1));
            }
        }

        internal int GetDriverPoints(ChampionshipStatistic statistic, int driverIndex)
        {
            return ((ChampionshipDataElement)statistic.DriverStats[driverIndex]).Points;
        }

        internal int GetTeamPointsDelta(ChampionshipStatistic statistic, int teamIndex)
        {
            if (teamIndex == 0)
            {
                return -1;
            }
            else
            {
                return (GetTeamPoints(statistic, teamIndex) - GetTeamPoints(statistic, teamIndex - 1));
            }
        }

        internal int GetTeamPoints(ChampionshipStatistic statistic, int teamIndex)
        {
            return ((ChampionshipDataElement)statistic.TeamStats[teamIndex]).Points;
        }
    }
}
