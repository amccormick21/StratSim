using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyFlowLayout;
using TeamStats.Statistics;
using DataSources.DataConnections;

namespace TeamStats.MyFlowLayout
{
    public class TeamStatsPanelControlEvents : PanelControlEvents
    {
        public event ViewPanelEventHandler ShowChampionshipsPanel;
        public event ShowStatisticPanelEventHandler<IStatistic> ShowStatsPanel;
        public event ShowStatisticPanelEventHandler<ISingleSessionStatistic> ShowSingleSessionStatsPanel;
        public event ShowStatisticPanelEventHandler<IPositionFilterStatistic> ShowPositionFilterStatsPanel;
        public event ShowStatisticPanelEventHandler<IDualSessionStatistic> ShowDualSessionStatsPanel;
        public delegate void ShowResultGridEventHandler(MainForm form, Session session);
        public event ShowResultGridEventHandler ShowResultGrid;

        public void OnShowChampionshipsPanel(MainForm form)
        {
            if (ShowChampionshipsPanel != null)
                ShowChampionshipsPanel(form);
        }

        public void OnShowStatsPanel(MainForm form, IStatistic statistic, Competitor[] tabsToShow)
        {
            if (ShowStatsPanel != null)
                ShowStatsPanel(form, statistic, tabsToShow);
        }

        public void OnShowResultGrid(MainForm form, Session session)
        {
            if (ShowResultGrid != null)
                ShowResultGrid(form, session);
        }

        public void OnShowSingleSessionStatsPanel(MainForm form, ISingleSessionStatistic statistic, Competitor[] tabsToShow)
        {
            if (ShowSingleSessionStatsPanel != null)
                ShowSingleSessionStatsPanel(form, statistic, tabsToShow);
        }

        public void OnShowDualSessionStatsPanel(MainForm form, IDualSessionStatistic statistic, Competitor[] tabsToShow)
        {
            if (ShowDualSessionStatsPanel != null)
                ShowDualSessionStatsPanel(form, statistic, tabsToShow);
        }

        public void OnShowPositionFilterStatsPanel(MainForm form, IPositionFilterStatistic statistic, Competitor[] tabsToShow)
        {
            if (ShowPositionFilterStatsPanel != null)
                ShowPositionFilterStatsPanel(form, statistic, tabsToShow);
        }
    }
}
