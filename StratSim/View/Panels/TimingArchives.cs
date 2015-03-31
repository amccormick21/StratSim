using StratSim.Model;
using StratSim.View.MyFlowLayout;
using System;
using System.Drawing;
using System.Windows.Forms;
using MyFlowLayout;
using DataSources.DataConnections;

namespace StratSim.View.Panels
{
    /// <summary>
    /// Represents a panel displaying archived data from the PDF files
    /// Allows data to be viewed inside the panel
    /// </summary>
    public class TimingArchives : MyPanel
    {
        ComboBox driverCombo;
        Label driverLabel;
        Label raceLabel;
        ComboBox raceCombo;
        ComboBox sessionCombo;
        Label sessionLabel;
        ListBox lapTimes;
        Button getData;

        public TimingArchives(MainForm FormToAdd)
            : base(400, 160, "Archives", FormToAdd, Properties.Resources.Archives)
        {
            InitialiseControls();
            getData.Click += GetData_Click;

            PopulateComboBoxes();
            SetRaceIndex(Data.RaceIndex);
        }

        void GetData_Click(object sender, EventArgs e)
        {
            Data.DriverIndex = driverCombo.SelectedIndex;
            Session session = (Session)sessionCombo.SelectedIndex;
            //Data.SessionIndex = sessionCombo.SelectedIndex;
            LoadTimingData(driverCombo.SelectedIndex, raceCombo.SelectedIndex, session);
        }

        /// <summary>
        /// Populates the list box with the lap times required
        /// </summary>
        /// <param name="driver">The driver to show</param>
        /// <param name="race">The race from which to display data</param>
        /// <param name="session">The session to display data from</param>
        void LoadTimingData(int driver, int race, Session session)
        {
            string lapIdentifier;
            lapTimes.Items.Clear();

            foreach (Stint s in Data.Drivers[driver].PracticeTimes.PracticeSessionStints[session.GetSessionIndex()])
            {
                for (int lapInStint = 0; lapInStint < s.lapTimes.Count; lapInStint++)
                {
                    lapIdentifier = Convert.ToString(s.lapTimes[lapInStint]);
                    if (lapInStint == 0) { lapIdentifier = "OUT " + lapIdentifier; }
                    if (lapInStint == s.lapTimes.Count - 1) { lapIdentifier += " IN"; }
                    lapTimes.Items.Add(lapIdentifier);
                }
            }
        }

        /// <summary>
        /// Loads data into the combo boxes
        /// </summary>
        void PopulateComboBoxes()
        {
            foreach (Driver d in Data.Drivers)
            {
                driverCombo.Items.Add(d.DriverName);
            }

            foreach (Track t in Data.Tracks)
            {
                raceCombo.Items.Add(t.name);
            }

            foreach (Session session in (Session[])Enum.GetValues(typeof(Session)))
            {
                if (session.GetTimingDataType() == TimingDataType.LapTimeData)
                    sessionCombo.Items.Add(session.GetSessionName());
            }
        }

        void raceCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Data.RaceIndex = raceCombo.SelectedIndex;
            StratSim.Model.CalculationControllers.CalculationController.PopulateDriverDataFromFiles(raceCombo.SelectedIndex);
        }

        void InitialiseControls()
        {
            driverCombo = new ComboBox();
            driverLabel = new Label();
            raceLabel = new Label();
            raceCombo = new ComboBox();
            sessionCombo = new ComboBox();
            sessionLabel = new Label();
            lapTimes = new ListBox();
            getData = new Button();

            int topBorder = 10;
            int leftBorder = 10;

            Size defaultSize = new Size(100, 20);

            int leftPosition = MyPadding.Left + leftBorder;
            int topPosition = MyPadding.Top + topBorder;

            int col2LeftPosition = leftPosition + defaultSize.Width + leftBorder;
            int col3LeftPosition = col2LeftPosition + defaultSize.Width + leftBorder;

            raceLabel.Location = new Point(leftPosition, topPosition);
            raceLabel.Size = defaultSize;
            raceLabel.Text = "Race";

            raceCombo.Location = new Point(col2LeftPosition, topPosition);
            raceCombo.Size = defaultSize;
            raceCombo.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            raceCombo.SelectedIndexChanged += raceCombo_SelectedIndexChanged;
            topPosition += defaultSize.Height + topBorder;

            driverLabel.Location = new Point(leftPosition, topPosition);
            driverLabel.Size = defaultSize;
            driverLabel.Text = "Driver";

            driverCombo.Location = new Point(col2LeftPosition, topPosition);
            driverCombo.Size = defaultSize;
            driverCombo.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            driverCombo.SelectedIndexChanged += (s, e) => Data.DriverIndex = driverCombo.SelectedIndex;
            topPosition += defaultSize.Height + topBorder;

            sessionLabel.Location = new Point(leftPosition, topPosition);
            sessionLabel.Size = defaultSize;
            sessionLabel.Text = "Session";

            sessionCombo.Location = new Point(col2LeftPosition, topPosition);
            sessionCombo.Size = defaultSize;
            sessionCombo.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            topPosition += defaultSize.Height + topBorder;

            getData.Location = new Point((leftPosition + (2 * defaultSize.Width + leftBorder) - getData.Width) / 2, topPosition);
            getData.Size = new Size(100, 25);
            getData.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            getData.Text = "Get Data";

            topPosition = MyPadding.Top + topBorder;
            lapTimes.Location = new Point(col3LeftPosition, topPosition);
            lapTimes.Size = new Size(150, 150);
            lapTimes.BorderStyle = System.Windows.Forms.BorderStyle.None;

            Controls.Add(getData);
            Controls.Add(lapTimes);
            Controls.Add(sessionCombo);
            Controls.Add(sessionLabel);
            Controls.Add(raceCombo);
            Controls.Add(raceLabel);
            Controls.Add(driverLabel);
            Controls.Add(driverCombo);
        }

        public void SetRaceIndex(int value)
        { raceCombo.SelectedIndex = value; }
    }
}
