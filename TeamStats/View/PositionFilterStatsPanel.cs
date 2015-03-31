using DataSources.DataConnections;
using MyFlowLayout;
using StratSim.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeamStats.Statistics;

namespace TeamStats.View
{
    public class PositionFilterStatsPanel : SingleSessionStatsPanel
    {
        ComboBox finishStateFilterSelect, positionFilterSelect;

        public PositionFilterStatsPanel(MainForm parentForm, IPositionFilterStatistic Statistic, Competitor[] TabsToShow)
            : base(parentForm, Statistic, TabsToShow)
        {
            PositionFilterChanged += PositionFilterStatsPanel_PositionFilterChanged;
            FinishStateFilterChanged += PositionFilterStatsPanel_FinishStateFilterChanged;
        }

        void PositionFilterStatsPanel_FinishStateFilterChanged(object sender, FinishingState e)
        {
            if (finishStateFilterSelect.SelectedIndex != (int)e)
                finishStateFilterSelect.SelectedIndex = (int)e;
            SetStatisticFilter();
        }

        void PositionFilterStatsPanel_PositionFilterChanged(object sender, int e)
        {
            if (positionFilterSelect.SelectedIndex != e - 1)
                positionFilterSelect.SelectedIndex = e - 1;
            SetStatisticFilter();
        }

        protected override int TopBuffer
        {
            get { return 75; }
        }

        internal new IPositionFilterStatistic Statistic
        {
            get { return (IPositionFilterStatistic)base.Statistic; }
            set { base.Statistic = value; }
        }

        private int positionFilter;
        public int PositionFilter
        {
            get { return positionFilter; }
            set
            {
                positionFilter = value;
                if (PositionFilterChanged != null)
                    PositionFilterChanged(this, positionFilter);
            }
        }
        public event EventHandler<int> PositionFilterChanged;

        private FinishingState finishStateFilter;
        public FinishingState FinishStateFilter
        {
            get { return finishStateFilter; }
            set
            {
                finishStateFilter = value;
                if (FinishStateFilterChanged != null)
                    FinishStateFilterChanged(this, finishStateFilter);
            }
        }
        public event EventHandler<FinishingState> FinishStateFilterChanged;

        protected override void InitialiseControls(ICollection<Competitor> tabsToShow)
        {
            base.InitialiseControls(tabsToShow);
            positionFilterSelect = new ComboBox
            {
                Left = LeftBuffer,
                Top = 50,
                Width = 125,
                FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            for (int position = 1; position < Data.NumberOfDrivers + 1; position++)
            {
                positionFilterSelect.Items.Add(position);
            }
            positionFilterSelect.SelectedIndex = Statistic.PositionLimit - 1;
            positionFilterSelect.SelectedIndexChanged += positionFilterSelect_SelectedIndexChanged;

            finishStateFilterSelect = new ComboBox
            {
                Left = LeftBuffer +10 + 125,
                Top = 50,
                Width = 125,
                FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (FinishingState finishState in (FinishingState[])Enum.GetValues(typeof(FinishingState)))
            {
                finishStateFilterSelect.Items.Add(finishState);
            }
            finishStateFilterSelect.SelectedIndex = (int)Statistic.FinishStateLimit;
            finishStateFilterSelect.SelectedIndexChanged += finishStateFilterSelect_SelectedIndexChanged;

        }

        void finishStateFilterSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            FinishStateFilter = (FinishingState)finishStateFilterSelect.SelectedIndex;
        }

        private void positionFilterSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            PositionFilter = positionFilterSelect.SelectedIndex + 1;
        }

        protected override void SetSortParameters()
        {
            base.SetSortParameters();
            PositionFilter = Statistic.PositionLimit;
            FinishStateFilter = Statistic.FinishStateLimit;
        }

        private void SetStatisticFilter()
        {
            Statistic.SetPositionLimit(PositionFilter);
            Statistic.SetFinishStateLimit(FinishStateFilter);
            Statistic.CalculateStatistics();
            Statistic.Sort(OrderType, SortType);
            PopulateStatistics();
        }

        protected override void AddControls(ICollection<Competitor> tabsToShow)
        {
            base.AddControls(tabsToShow);
            this.Controls.Add(positionFilterSelect);
            this.Controls.Add(finishStateFilterSelect);
        }
    }
}
