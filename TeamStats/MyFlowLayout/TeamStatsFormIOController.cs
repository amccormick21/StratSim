using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyFlowLayout;
using TeamStats.View;
using TeamStats.Statistics;
using TeamStats.MyStatistics;
using DataSources.DataConnections;
using StratSim.View.Panels;

namespace TeamStats.MyFlowLayout
{
    public class TeamStatsFormIOController : MainFormIOController
    {
        /* Statistics to show:
         * Team battles - sorted by team
         *  - By percentage
         *  - By positions
         * Qualifying conversions
         *  - Team
         *  - Driver
         *  - By team and by value
         */
        enum StatisticType { TeamBattles, QualiConversions };

        TeamStatsPanelControlEvents events;

        public TeamStatsFormIOController(WindowFlowPanel MainPanel, TeamStatsMyToolbar Toolbar, TeamStatsPanelControlEvents Events, string Title)
            : base(MainPanel, Toolbar, Events, Title)
        {
        }

        public override void SetEvents(PanelControlEvents events)
        {
            base.SetEvents(events);
            this.events = (TeamStatsPanelControlEvents)events;
        }

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            events.ShowChampionshipsPanel += events_ShowChampionshipsPanel;
            events.ShowStatsPanel += events_ShowStatsPanel;
            events.ShowResultGrid += events_ShowResultGrid;
            events.ShowSingleSessionStatsPanel += events_ShowSingleSessionStatsPanel;
            events.ShowDualSessionStatsPanel += events_ShowDualSessionStatsPanel;
            events.ShowPositionFilterStatsPanel += events_ShowPositionFilterStatsPanel;
        }

        void events_ShowPositionFilterStatsPanel(MainForm form, IPositionFilterStatistic statistic, Competitor[] tabsToShow)
        {
            if (form == base.AssociatedForm)
            {
                ShowPositionFilterStatsPanel(statistic, tabsToShow);
            }
        }

        void events_ShowDualSessionStatsPanel(MainForm form, IDualSessionStatistic statistic, Competitor[] tabsToShow)
        {
            if (form == base.AssociatedForm)
            {
                ShowDualSessionStatsPanel(statistic, tabsToShow);
            }
        }

        void events_ShowSingleSessionStatsPanel(MainForm form, ISingleSessionStatistic statistic, Competitor[] tabsToShow)
        {
            if (form == base.AssociatedForm)
            {
                ShowSingleSessionStatsPanel(statistic, tabsToShow);
            }
        }

        private void events_ShowResultGrid(MainForm form, Session session)
        {
            if (form == base.AssociatedForm)
            {
                ShowResultsGrid(session);
            }
        }

        private void events_ShowStatsPanel(MainForm form, IStatistic statistic, Competitor[] tabsToShow)
        {
            if (form == base.AssociatedForm)
            {
                ShowStatsPanel(statistic, tabsToShow);
            }
        }

        private void events_ShowChampionshipsPanel(MainForm form)
        {
            if (form == base.AssociatedForm)
            {
                ShowChampionshipsPanel();
            }
        }

        public override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();
            events.ShowChampionshipsPanel -= events_ShowChampionshipsPanel;
            events.ShowStatsPanel -= events_ShowStatsPanel;
            events.ShowResultGrid -= events_ShowResultGrid;
            events.ShowSingleSessionStatsPanel -= events_ShowSingleSessionStatsPanel;
            events.ShowDualSessionStatsPanel -= events_ShowDualSessionStatsPanel;
            events.ShowPositionFilterStatsPanel -= events_ShowPositionFilterStatsPanel;
        }

        public void ShowChampionshipsPanel()
        {
            AddPanel(new ChampionshipPanel(base.AssociatedForm));
            AddPanel(this.ContentTabControl);
        }

        private void ShowStatsPanel(IStatistic Statistic, Competitor[] tabsToShow)
        {
            StatsPanel panel = new StatsPanel(base.AssociatedForm, Statistic, tabsToShow);
            AddContentPanel(panel);
            panel.AddToolStrip();
            FinishedAdding();
        }

        private void ShowSingleSessionStatsPanel(ISingleSessionStatistic Statistic, Competitor[] tabsToShow)
        {
            SingleSessionStatsPanel panel = new SingleSessionStatsPanel(base.AssociatedForm, Statistic, tabsToShow);
            AddContentPanel(panel);
            panel.AddToolStrip();
            FinishedAdding();
        }

        private void ShowDualSessionStatsPanel(IDualSessionStatistic Statistic, Competitor[] tabsToShow)
        {
            DualSessionStatsPanel panel = new DualSessionStatsPanel(base.AssociatedForm, Statistic, tabsToShow);
            AddContentPanel(panel);
            panel.AddToolStrip();
            FinishedAdding();
        }

        private void ShowPositionFilterStatsPanel(IPositionFilterStatistic Statistic, Competitor[] tabsToShow)
        {
            PositionFilterStatsPanel panel = new PositionFilterStatsPanel(base.AssociatedForm, Statistic, tabsToShow);
            AddContentPanel(panel);
            panel.AddToolStrip();
            FinishedAdding();
        }

        private void ShowResultsGrid(Session session)
        {
            ResultGrid panel = new ResultGrid(base.AssociatedForm, session);
            AddPanel(panel);
            panel.AddToolStrip();
            FinishedAdding();
        }

        public override MainFormIOController GetNew()
        {
            global::MyFlowLayout.WindowFlowPanel MainPanel = new WindowFlowPanel();
            TeamStatsPanelControlEvents Events = new TeamStatsPanelControlEvents();
            TeamStatsMyToolbar Toolbar = new TeamStatsMyToolbar(MainPanel, Events);
            return new TeamStatsFormIOController(MainPanel, Toolbar, Events, "Team Stats");
        }
    }
}
