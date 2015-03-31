using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

using StratSim.Model;
using StratSim.View.MyFlowLayout;
using StratSim.ViewModel;

namespace StratSim.View.Panels
{
    class StartPanel : MyPanel
    {
        Button btnStrategy;
        Button btnArchive;
        Button btnExit;
        ComboBox cbxQuickStartRace;

        public StartPanel(MainForm FormToAdd)
            : base(300, 300, "Start", FormToAdd, Properties.Resources.Start)
        {
            AddControls();
            AddTracks();
        }

        void AddTracks()
        {
            foreach (Track t in Data.Tracks)
            {
                cbxQuickStartRace.Items.Add(t.name);
            }
            cbxQuickStartRace.SelectedIndexChanged += cbxQuickStartRace_SelectedIndexChanged;
        }

        void AddControls()
        {
            btnStrategy = new Button();
            btnArchive = new Button();
            btnExit = new Button();
            cbxQuickStartRace = new ComboBox();

            int leftBorder = 10;
            int topBorder = 10;

            Size defaultSize = new Size(100, 25);

            int leftPosition = MyPadding.Left + leftBorder;
            int topPosition = MyPadding.Top + topBorder;

            btnStrategy.Location = new Point(leftPosition, topPosition);
            btnStrategy.Size = defaultSize;
            btnStrategy.Text = "Strategy";
            btnStrategy.Click += Strategy_Click;
            topPosition += defaultSize.Height + topBorder;

            btnArchive.Location = new Point(leftPosition, topPosition);
            btnArchive.Size = defaultSize;
            btnArchive.Text = "Timing Archive";
            btnArchive.Click += btnArchive_Click;
            topPosition += defaultSize.Height + topBorder;

            btnExit.Location = new Point(leftPosition, topPosition);
            btnExit.Size = defaultSize;
            btnExit.Text = "Exit";
            btnExit.Click += btnExit_Click;
            topPosition += defaultSize.Height + topBorder;

            cbxQuickStartRace.Location = new Point(leftPosition, topPosition);
            cbxQuickStartRace.Size = defaultSize;

            Controls.Add(cbxQuickStartRace);
            Controls.Add(btnExit);
            Controls.Add(btnArchive);
            Controls.Add(btnStrategy);
       }

        private void cbxQuickStartRace_SelectedIndexChanged(object sender, EventArgs e)
        {
            Program.PopulateDriverDataFromFiles(cbxQuickStartRace.SelectedIndex);
            Functions.calculateStrategyParameters(cbxQuickStartRace.SelectedIndex);
            PanelControlEvents.OnLoadPaceParametersFromRace();
            PanelControlEvents.OnShowPaceParameters(Form);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Program.Exit();
        }

        private void btnArchive_Click(object sender, EventArgs e)
        {
            PanelControlEvents.OnShowArchives(Form);
        }

        private void Strategy_Click(object sender, EventArgs e)
        {
            PanelControlEvents.OnShowDataInput(Form);
        }


    }
}
