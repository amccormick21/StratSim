using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeamStats.Statistics;
using MyFlowLayout;
using DataSources.DataConnections;

namespace TeamStats.View
{
    public class DualSessionStatsPanel : StatsPanel
    {
        ComboBox selectFirstSessionBox, selectSecondSessionBox;
        ToolStripDropDownButton selectFirstSession, selectSecondSession;

        protected override int TopBuffer { get { return 50; } }

        internal new IDualSessionStatistic Statistic
        {
            get { return (IDualSessionStatistic)base.Statistic; }
            set { base.Statistic = value; }
        }

        private Session firstSession;
        internal Session FirstSession
        {
            get { return firstSession; }
            set
            {
                firstSession = value;
                if (FirstSessionChanged != null)
                    FirstSessionChanged(this, firstSession);
            }
        }
        internal event EventHandler<Session> FirstSessionChanged;

        private Session secondSession;
        internal Session SecondSession
        {
            get { return secondSession; }
            set
            {
                secondSession = value;
                if (SecondSessionChanged != null)
                    SecondSessionChanged(this, secondSession);
            }
        }
        internal event EventHandler<Session> SecondSessionChanged;

        public DualSessionStatsPanel(MainForm parentForm, IDualSessionStatistic Statistic, Competitor[] TabsToShow)
            : base(parentForm, Statistic, TabsToShow)
        {
            FirstSessionChanged += DualSessionStatsPanel_FirstSessionChanged;
            SecondSessionChanged += DualSessionStatsPanel_SecondSessionChanged;
        }

        void DualSessionStatsPanel_SecondSessionChanged(object sender, Session e)
        {
            if (selectSecondSessionBox.SelectedIndex != (int)e)
                selectSecondSessionBox.SelectedIndex = (int)e;
            SetStatisticSecondSession(e);
        }

        void DualSessionStatsPanel_FirstSessionChanged(object sender, Session e)
        {
            if (selectFirstSessionBox.SelectedIndex != (int)e)
                selectFirstSessionBox.SelectedIndex = (int)e;
            SetStatisticFirstSession(e);
        }

        protected override void InitialiseToolStrip()
        {
            base.InitialiseToolStrip();
            selectFirstSession = new ToolStripDropDownButton("First Session");
            selectSecondSession = new ToolStripDropDownButton("Second Session");
            IndexedToolStripButton temp;
            foreach (var session in (Session[])Enum.GetValues(typeof(Session)))
            {
                temp = new IndexedToolStripButton((int)session);
                temp.Text = Convert.ToString(session);
                temp.ButtonClicked += FirstSessionButtonClicked;
                selectFirstSession.DropDownItems.Add(temp);
                temp = new IndexedToolStripButton((int)session);
                temp.Text = Convert.ToString(session);
                temp.ButtonClicked += SecondSessionButtonClicked;
                selectSecondSession.DropDownItems.Add(temp);
            }
            thisToolStripDropDown.DropDownItems.Add(selectFirstSession);
            thisToolStripDropDown.DropDownItems.Add(selectSecondSession);
        }

        private void SecondSessionButtonClicked(object sender, int e)
        {
            SecondSession = (Session)e;
        }

        private void FirstSessionButtonClicked(object sender, int e)
        {
            FirstSession = (Session)e;
        }

        protected override void InitialiseControls(ICollection<Competitor> tabsToShow)
        {
            base.InitialiseControls(tabsToShow);

            selectFirstSessionBox = new ComboBox
            {
                Left = LeftBuffer,
                Top = base.TopBuffer,
                Width = 125,
                FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            selectSecondSessionBox = new ComboBox
            {
                Left = LeftBuffer + 125 + ControlSpacing,
                Top = base.TopBuffer,
                Width = 125,
                FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (var session in (Session[])Enum.GetValues(typeof(Session)))
            {
                selectFirstSessionBox.Items.Add(session);
                selectSecondSessionBox.Items.Add(session);
            }
            selectFirstSessionBox.SelectedIndex = (int)FirstSession;
            selectSecondSessionBox.SelectedIndex = (int)SecondSession;
            selectFirstSessionBox.SelectedIndexChanged += selectFirstSessionBox_SelectedIndexChanged;
            selectSecondSessionBox.SelectedIndexChanged += selectSecondSessionBox_SelectedIndexChanged;
        }

        void selectSecondSessionBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SecondSession = (Session)selectSecondSessionBox.SelectedIndex;
        }

        void selectFirstSessionBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            FirstSession = (Session)selectFirstSessionBox.SelectedIndex;
        }

        protected override void SetSortParameters()
        {
            FirstSession = Statistic.FirstSession;
            SecondSession = Statistic.SecondSession;
            base.SetSortParameters();
        }

        private void SetStatisticFirstSession(Session firstSession)
        {
            Statistic.SetFirstSession(firstSession);
            Statistic.CalculateStatistics();
            Statistic.Sort(OrderType, SortType);
            PopulateStatistics();
        }

        private void SetStatisticSecondSession(Session secondSession)
        {
            Statistic.SetSecondSession(secondSession);
            Statistic.CalculateStatistics();
            Statistic.Sort(OrderType, SortType);
            PopulateStatistics();
        }

        protected override void AddControls(ICollection<Competitor> tabsToShow)
        {
            base.AddControls(tabsToShow);
            this.Controls.Add(selectFirstSessionBox);
            this.Controls.Add(selectSecondSessionBox);
        }
    }
}
