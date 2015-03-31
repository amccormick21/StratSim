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
    public class ResultBasedStatistic : BaseStatistic, ISingleSessionStatistic
    {
        protected event EventHandler ResultsUpdated;

        Result[,] results;
        protected Result[,] Results
        {
            get { return results; }
            set
            {
                results = value;
                if (ResultsUpdated != null)
                { ResultsUpdated(this, new EventArgs()); }
            }
        }

        public ResultBasedStatistic()
            : base()
        {
            //Default session
            SetSession(Session.Race);
        }

        public override void SetupStatistics()
        {
            base.SetupStatistics();
            Results = new Result[Data.NumberOfDrivers, Data.NumberOfTracks];
        }

        public virtual void SetResults(Result[,] results)
        {
            Results = results;
        }
        public virtual void SetSession(Session session)
        {
            this.Results = DriverResultsTableUpdater.GetResultsFromDatabase(session, Data.NumberOfDrivers, Data.NumberOfTracks, StratSim.Properties.Settings.Default.CurrentYear, Driver.GetDriverIndexDictionary());
            DisplaySession = session;
        }
        public override string GetStatisticMetadata(OrderType orderType, SortType sortType)
        {
            string metadata = "Session: " + this.DisplaySession.GetSessionName() + '\n';
            metadata += base.GetStatisticMetadata(orderType, sortType);
            return metadata;
        }

        public virtual Session DisplaySession { get; set; }

        public override void DisplayStatistic(TeamStatsPanelControlEvents events, MainForm form)
        {
            CalculateStatistics();
            events.OnShowSingleSessionStatsPanel(form, this, Competitors.All());
        }

        public void DisplayStatistic(TeamStatsPanelControlEvents events, MainForm form, Session session)
        {
            SetSession(session);
            DisplayStatistic(events, form);
        }
    }
}
