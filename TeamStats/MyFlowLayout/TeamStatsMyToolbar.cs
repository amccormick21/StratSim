using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MyFlowLayout;
using DataSources.DataConnections;
using TeamStats.View;
using TeamStats.MyStatistics;

namespace TeamStats.MyFlowLayout
{
    public class TeamStatsMyToolbar : MyToolbar
    {
        ToolStripDropDownButton openWindows;
        ToolStripDropDownButton statistics;
        ToolStripDropDownButton championshipStats;
        ToolStripDropDownButton finishingPositionStats;
        ToolStripDropDownButton finishesAheadOfTeammateStats;
        ToolStripDropDownButton finishesAheadOfThresholdStats;
        ToolStripButton positionDeltaStats;
        ToolStripButton pointsDeltaStats;
        ToolStripButton teamProgressStats;
        //TODO: add more buttons as more statistics become available.
        ToolStripButton championships, results;

        TeamStatsPanelControlEvents events;

        public TeamStatsMyToolbar(WindowFlowPanel MainPanel, PanelControlEvents Events)
            : base(MainPanel, Events)
        {
            this.events = (TeamStatsPanelControlEvents)Events;
        }

        public override void AddButtonsToToolBar()
        {
            base.AddButtonsToToolBar();

            //Start Open Windows
            openWindows = new ToolStripDropDownButton("Open Windows");
            MenuBar.Items.Add(openWindows);
            championships = new ToolStripButton("Championships", Properties.Resources.Championships);
            championships.Click += (s, e) => { events.OnShowChampionshipsPanel(base.Form); };
            openWindows.DropDownItems.Add(championships);
            results = new ToolStripButton("Results", Properties.Resources.Results);
            results.Click += (s, e) => { events.OnShowResultGrid(base.Form, Session.Race); };
            openWindows.DropDownItems.Add(results);
            //Finish Open Windows

            //Start statistics
            statistics = new ToolStripDropDownButton("Statistics");
            MenuBar.Items.Add(statistics);
            championshipStats = new ToolStripDropDownButton("Championships");
            //statistics.championshipStats:
            IndexedToolStripButton temp;
            foreach (var session in (Session[])Enum.GetValues(typeof(Session)))
            {
                temp = new IndexedToolStripButton((int)session);
                temp.Text = Convert.ToString(session);
                temp.ButtonClicked += StartChampionshipStatistic;
                championshipStats.DropDownItems.Add(temp);
            }
            statistics.DropDownItems.Add(championshipStats);

            //statistics.finishingPositionStats:
            finishingPositionStats = new ToolStripDropDownButton("Finishes");
            foreach (var session in (Session[])Enum.GetValues(typeof(Session)))
            {
                temp = new IndexedToolStripButton((int)session);
                temp.Text = Convert.ToString(session);
                temp.ButtonClicked += StartFinishingPositionStat;
                finishingPositionStats.DropDownItems.Add(temp);
            }
            statistics.DropDownItems.Add(finishingPositionStats);

            //statistics.finishesAheadOfTeammateStats:
            finishesAheadOfTeammateStats = new ToolStripDropDownButton("Team Battle");
            foreach (var session in (Session[])Enum.GetValues(typeof(Session)))
            {
                temp = new IndexedToolStripButton((int)session);
                temp.Text = Convert.ToString(session);
                temp.ButtonClicked += StartFinishesAheadOfTeammateStat;
                finishesAheadOfTeammateStats.DropDownItems.Add(temp);
            }
            statistics.DropDownItems.Add(finishesAheadOfTeammateStats);

            //statistics.finishesAheadOfThresholdStats:
            finishesAheadOfThresholdStats = new ToolStripDropDownButton("Threshold Position");
            foreach (var session in (Session[])Enum.GetValues(typeof(Session)))
            {
                temp = new IndexedToolStripButton((int)session);
                temp.Text = Convert.ToString(session);
                temp.ButtonClicked += StartFinishesAheadOfThresholdStat;
                finishesAheadOfThresholdStats.DropDownItems.Add(temp);
            }
            statistics.DropDownItems.Add(finishesAheadOfThresholdStats);

            //statistics.positionDeltaStats
            positionDeltaStats = new ToolStripButton("Position Deltas");
            positionDeltaStats.Click += StartPositionDeltaStat;
            statistics.DropDownItems.Add(positionDeltaStats);

            //statistics.pointsDeltaStats
            pointsDeltaStats = new ToolStripButton("Points Deltas");
            pointsDeltaStats.Click += StartPointsDeltaStats;
            statistics.DropDownItems.Add(pointsDeltaStats);

            //statistics.teamProgressStats
            teamProgressStats = new ToolStripButton("Team Progress");
            teamProgressStats.Click += StartTeamProgressStats;
            statistics.DropDownItems.Add(teamProgressStats);
            //Finish statistics
        }

        private void StartTeamProgressStats(object sender, EventArgs e)
        {
            ProgressAgainstTeammateStatistic statistic = new ProgressAgainstTeammateStatistic();
            statistic.DisplayStatistic(events, base.Form);
        }

        private void StartPointsDeltaStats(object sender, EventArgs e)
        {
            SessionPointsDeltaStatistic statistic = new SessionPointsDeltaStatistic();
            statistic.DisplayStatistic(events, base.Form);
        }

        private void StartPositionDeltaStat(object sender, EventArgs e)
        {
            SessionPositionDeltaStatistic statistic = new SessionPositionDeltaStatistic();
            statistic.DisplayStatistic(events, base.Form);
        }

        private void StartFinishesAheadOfThresholdStat(object sender, int e)
        {
            AverageFinishAbovePositionStatistic statistic = new AverageFinishAbovePositionStatistic();
            statistic.DisplayStatistic(events, base.Form);
        }

        private void StartFinishesAheadOfTeammateStat(object sender, int e)
        {
            FinishesAheadOfTeammateStatistic statistic = new FinishesAheadOfTeammateStatistic();
            statistic.DisplayStatistic(events, base.Form);
        }

        private void StartFinishingPositionStat(object sender, int e)
        {
            FinishingPositionChampionship statistic = new FinishingPositionChampionship();
            statistic.DisplayStatistic(events, base.Form, (Session)e);
        }

        void StartChampionshipStatistic(object sender, int e)
        {
            ChampionshipStatistic statistic = new ChampionshipStatistic();
            statistic.DisplayStatistic(events, base.Form, (Session)e);
        }
    }
}
