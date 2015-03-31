using DataSources.DataConnections;
using MyFlowLayout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeamStats.Statistics;

namespace TeamStats.View
{
    public class SingleSessionStatsPanel : StatsPanel
    {
        ComboBox selectSessionBox;
        ToolStripDropDownButton selectSession;

        protected override int TopBuffer { get { return 50; } }

        internal new ISingleSessionStatistic Statistic
        {
            get { return (ISingleSessionStatistic)base.Statistic; }
            set { base.Statistic = value; }
        }

        private Session session;
        internal Session Session
        {
            get { return session; }
            set
            {
                session = value;
                if (SessionChanged != null)
                    SessionChanged(this, session);
            }
        }
        internal event EventHandler<Session> SessionChanged;

        public SingleSessionStatsPanel(MainForm parentForm, ISingleSessionStatistic Statistic, Competitor[] TabsToShow)
            : base(parentForm, Statistic, TabsToShow)
        {
            SessionChanged += SessionSelectStatsPanel_SessionChanged;
        }

        private void SessionSelectStatsPanel_SessionChanged(object sender, Session e)
        {
            if (selectSessionBox.SelectedIndex != (int)e)
                selectSessionBox.SelectedIndex = (int)e;
            SetStatisticSession(e);
        }

        protected override void InitialiseToolStrip()
        {
            base.InitialiseToolStrip();
            selectSession = new ToolStripDropDownButton("Session");
            IndexedToolStripButton temp;
            foreach (var session in (Session[])Enum.GetValues(typeof(Session)))
            {
                temp = new IndexedToolStripButton((int)session);
                temp.Text = Convert.ToString(session);
                temp.ButtonClicked += SessionToolbarChanged;
                selectSession.DropDownItems.Add(temp);
            }
            thisToolStripDropDown.DropDownItems.Add(selectSession);
        }

        private void SessionToolbarChanged(object sender, int e)
        {
            Session = (Session)e;
        }

        protected override void InitialiseControls(ICollection<Competitor> tabsToShow)
        {
            base.InitialiseControls(tabsToShow);

            selectSessionBox = new ComboBox
            {
                Left = LeftBuffer,
                Top = base.TopBuffer,
                Width = 125,
                FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (var session in (Session[])Enum.GetValues(typeof(Session)))
            {
                selectSessionBox.Items.Add(session);
            }
            selectSessionBox.SelectedIndex = (int)Session;
            selectSessionBox.SelectedIndexChanged += selectSessionBox_SelectedIndexChanged;
        }

        void selectSessionBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session = (Session)selectSessionBox.SelectedIndex;
        }

        protected override void SetSortParameters()
        {
            Session = Statistic.DisplaySession;
            base.SetSortParameters();
        }

        private void SetStatisticSession(Session session)
        {
            Statistic.SetSession(session);
            Statistic.CalculateStatistics();
            Statistic.Sort(OrderType, SortType);
            PopulateStatistics();
        }

        protected override void AddControls(ICollection<Competitor> tabsToShow)
        {
            base.AddControls(tabsToShow);
            this.Controls.Add(selectSessionBox);
        }
    }
}
