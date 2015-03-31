using DataSources.DataConnections;
using MyFlowLayout;
using StratSim.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStats.MyFlowLayout;

namespace TeamStats.Statistics
{
    /// <summary>
    /// Provides base functionality for a statistic which compares results from two different sessions.
    /// </summary>
    public class SessionComparisonStatistic : BaseStatistic, IDualSessionStatistic
    {
        Result[,] firstSessionResults, secondSessionResults;
        protected Result[,] FirstSessionResults
        {
            get { return firstSessionResults; }
            private set { firstSessionResults = value; }
        }
        protected Result[,] SecondSessionResults
        {
            get { return secondSessionResults; }
            private set { secondSessionResults = value; }
        }

        Session firstSession, secondSession;
        public Session FirstSession
        {
            get { return firstSession; }
            private set { firstSession = value; }
        }
        public Session SecondSession
        {
            get { return secondSession; }
            private set { secondSession = value; }
        }

        public SessionComparisonStatistic()
            : base()
        {
            //Defaults:
            SetSessions(Session.Grid, Session.Race);
        }

        public override void SetupStatistics()
        {
            FirstSessionResults = new Result[Data.NumberOfDrivers, Data.NumberOfTracks];
            SecondSessionResults = new Result[Data.NumberOfDrivers, Data.NumberOfTracks];
        }

        public void SetSessions(Session firstSession, Session secondSession)
        {
            SetFirstSession(firstSession);
            SetSecondSession(secondSession);
        }
        public void SetFirstSession(Session firstSession)
        {
            FirstSession = firstSession;
            FirstSessionResults = DriverResultsTableUpdater.GetResultsFromDatabase(firstSession, Data.NumberOfDrivers, Data.NumberOfTracks, StratSim.Properties.Settings.Default.CurrentYear, Driver.GetDriverIndexDictionary());
        }
        public void SetSecondSession(Session secondSession)
        {
            SecondSession = secondSession;
            SecondSessionResults = DriverResultsTableUpdater.GetResultsFromDatabase(secondSession, Data.NumberOfDrivers, Data.NumberOfTracks, StratSim.Properties.Settings.Default.CurrentYear, Driver.GetDriverIndexDictionary());
        }

        public override string GetStatisticName()
        {
            return "Comparison Statistic";
        }

        public override string GetStatisticMetadata(OrderType orderType, SortType sortType)
        {
            string metaData = "";
            metaData += "First Session: " + FirstSession.GetSessionName() + '\n';
            metaData += "Second Session: " + SecondSession.GetSessionName() + '\n';
            metaData += base.GetStatisticMetadata(orderType, sortType);
            return metaData;
        }

        public void DisplayStatistic(TeamStatsPanelControlEvents events, MainForm form, Session firstSession, Session secondSession)
        {
            SetSessions(firstSession, secondSession);
            DisplayStatistic(events, form);
        }

        public override void DisplayStatistic(TeamStatsPanelControlEvents events, MainForm form)
        {
            CalculateStatistics();
            events.OnShowDualSessionStatsPanel(form, this, Competitors.All());
        }
    }
}
