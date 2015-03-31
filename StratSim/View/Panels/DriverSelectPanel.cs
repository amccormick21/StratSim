using StratSim.Model;
using StratSim.View.MyFlowLayout;
using StratSim.View.UserControls;
using StratSim.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MyFlowLayout;

namespace StratSim.View.Panels
{
    /// <summary>
    /// Represents a list of drivers with options to select which drivers are displayed on a graph
    /// and which driver is selected for detailed analysis
    /// Also allows timing data from the race simulation to be displayed
    /// </summary>
    public class DriverSelectPanel : MyPanel
    {
        NormalisedRadioButton[] radioButtons;
        ShowDriverCheckBox[] checkBoxes;
        CheckBox showAll;
        Label[,] driverLabels;
        Label[,] gapLabels;
        ComboBox lapNumber;

        List<int> selectedDriverIndices = new List<int>();

        const int YSpacing = 23;

        NewGraph Graph;
        int currentLapNumber;

        bool showingTimeGaps;

        public delegate void DriverSelectionChangedEventHandler();
        public event DriverSelectionChangedEventHandler DriverSelectionsChanged;

        public delegate void NormalisedDriverChangedEventHandler();
        public event NormalisedDriverChangedEventHandler NormalisedDriverChanged;

        public DriverSelectPanel(MainForm FormToAdd)
            : base(180, 550, "Drivers", FormToAdd, Properties.Resources.Driver_Select)
        {
            showingTimeGaps = false;
            currentLapNumber = Data.GetRaceLaps();

            InitialiseControls();
            AddControls();

            MyEvents.UpdateIntervals += MyEvents_UpdateIntervals;

            SetPanelProperties(DockTypes.TopRight, AutosizeTypes.AutoHeight, FillStyles.FullHeight, this.Size);
        }

        public void SetGraph(NewGraph graph)
        {
            Graph = graph;
        }

        void InitialiseControls()
        {
            showAll = new CheckBox();
            driverLabels = new Label[Data.NumberOfDrivers, 2];
            gapLabels = new Label[Data.NumberOfDrivers, 4];
            radioButtons = new NormalisedRadioButton[Data.NumberOfDrivers];
            checkBoxes = new ShowDriverCheckBox[Data.NumberOfDrivers];

            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                driverLabels[driverIndex, 0] = new Label();
                driverLabels[driverIndex, 1] = new Label();
                radioButtons[driverIndex] = new NormalisedRadioButton(driverIndex);
                checkBoxes[driverIndex] = new ShowDriverCheckBox(driverIndex);
            }
        }

        void AddControls()
        {
            //The widths of the columns
            int[] Widths = { 20, 110, 20, 20, 20, 30, 30, 20 };
            int XLocation;
            int YLocation = MyPadding.Top;
            MyToolTip tooltip;

            XLocation = MyPadding.Left;
            XLocation += Widths[0] + Widths[1] + Widths[2];

            showAll = new CheckBox();
            showAll.Location = new Point(XLocation, YLocation);
            showAll.Width = Widths[3];
            showAll.Checked = true;
            showAll.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            tooltip = new MyToolTip(showAll, "Show/Hide all traces");
            showAll.CheckedChanged += showAll_CheckedChanged;
            this.Controls.Add(showAll);

            YLocation += YSpacing;

            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                XLocation = MyPadding.Left;

                driverLabels[driverIndex, 0].Location = new Point(XLocation, YLocation);
                driverLabels[driverIndex, 0].Text = Convert.ToString(Data.Drivers[driverIndex].DriverNumber);
                driverLabels[driverIndex, 0].Width = Widths[0];
                this.Controls.Add(driverLabels[driverIndex, 0]);
                XLocation += Widths[0];

                driverLabels[driverIndex, 1].Location = new Point(XLocation, YLocation);
                driverLabels[driverIndex, 1].ForeColor = Data.Drivers[driverIndex].LineColour;
                driverLabels[driverIndex, 1].Text = Data.Drivers[driverIndex].DriverName;
                driverLabels[driverIndex, 1].Width = Widths[1];
                this.Controls.Add(driverLabels[driverIndex, 1]);
                XLocation += Widths[1];

                radioButtons[driverIndex].Location = new Point(XLocation, YLocation);
                radioButtons[driverIndex].ControlActivated += NormalisedRadioButtonChecked;
                radioButtons[driverIndex].Width = Widths[2];
                radioButtons[driverIndex].FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
                this.Controls.Add(radioButtons[driverIndex]);
                XLocation += Widths[2];

                checkBoxes[driverIndex].Location = new Point(XLocation, YLocation);
                checkBoxes[driverIndex].ControlActivated += DriverCheckBoxChanged;
                checkBoxes[driverIndex].Width = Widths[3];
                checkBoxes[driverIndex].FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
                this.Controls.Add(checkBoxes[driverIndex]);
                XLocation += Widths[3];

                if (showingTimeGaps)
                {
                    for (int column = 0; column < 4; column++)
                    {
                        gapLabels[driverIndex, column] = new Label();
                        gapLabels[driverIndex, column].Width = Widths[column + 4];
                        gapLabels[driverIndex, column].Location = new Point(XLocation, YLocation + 5);
                        XLocation += Widths[column + 4];
                        this.Controls.Add(gapLabels[driverIndex, column]);
                    }
                    tooltip = new MyToolTip(gapLabels[driverIndex, 0], "Position");
                    tooltip = new MyToolTip(gapLabels[driverIndex, 1], "Gap");
                    tooltip = new MyToolTip(gapLabels[driverIndex, 2], "Interval");
                    tooltip = new MyToolTip(gapLabels[driverIndex, 3], "Stops");
                }

                YLocation += YSpacing;
            }

            if (showingTimeGaps) //Show the timing data text boxes
            {
                lapNumber = new ComboBox();
                lapNumber.Location = new Point(MyPadding.Left, YLocation);

                int laps = Data.GetRaceLaps();
                for (int lapIndex = 1; lapIndex <= laps; lapIndex++)
                {
                    lapNumber.Items.Add(lapIndex);
                }
                lapNumber.SelectedIndex = laps - 1;
                lapNumber.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;

                lapNumber.SelectedIndexChanged += lapNumber_SelectedIndexChanged;
                this.Controls.Add(lapNumber);
            }

        }

        void lapNumber_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentLapNumber = lapNumber.SelectedIndex + 1;
            MyEvents.OnLapNumberChanged(currentLapNumber);
        }

        /// <summary>
        /// Redraws all controls on the form and redraws them when
        /// the show time gaps value is changed
        /// </summary>
        /// <param name="gapLabelsCurrentlyDisplayed">Specifies whether the time gaps should be shown or hidden</param>
        void ResetPanel(bool gapLabelsCurrentlyDisplayed)
        {
            showAll.CheckedChanged -= showAll_CheckedChanged;
            this.Controls.Remove(showAll);
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                this.Controls.Remove(driverLabels[driverIndex, 0]);
                this.Controls.Remove(driverLabels[driverIndex, 1]);
                radioButtons[driverIndex].ControlActivated -= NormalisedRadioButtonChecked;
                this.Controls.Remove(radioButtons[driverIndex]);
                checkBoxes[driverIndex].ControlActivated -= DriverCheckBoxChanged;
                this.Controls.Remove(checkBoxes[driverIndex]);
            }

            if (gapLabelsCurrentlyDisplayed)
            {
                for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        this.Controls.Remove(gapLabels[driverIndex, j]);
                    }
                }

                lapNumber.SelectedIndexChanged -= lapNumber_SelectedIndexChanged;
                this.Controls.Remove(lapNumber);
                this.Width = 180;
            }
            else
            {
                this.Width = 280;
            }

            SetPanelProperties(DockTypes.TopRight, AutosizeTypes.AutoHeight, FillStyles.FullHeight, this.Size);

            //Add the controls again
            AddControls();
        }

        void MyEvents_UpdateIntervals(racePosition[,] positions, int lapNumber)
        {
            if (lapNumber == currentLapNumber && showingTimeGaps)
            {
                WriteIntervalData(positions);
            }
        }

        void WriteIntervalData(racePosition[,] positions)
        {
            int positionIndex = -1;
            int lapNumber = currentLapNumber - 1;

            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                positionIndex = -1;
                while (positions[++positionIndex, lapNumber].driver != driverIndex)
                { }
                gapLabels[driverIndex, 0].Text = Convert.ToString(positions[positionIndex, lapNumber].position + 1);
                gapLabels[driverIndex, 1].Text = Convert.ToString(positions[positionIndex, lapNumber].gap);
                gapLabels[driverIndex, 2].Text = Convert.ToString(positions[positionIndex, lapNumber].interval);
                gapLabels[driverIndex, 3].Text = Convert.ToString(Data.Drivers[driverIndex].StopsBeforeLap(lapNumber));

                PositionDriverControls(positionIndex, driverIndex);
            }
        }

        /// <summary>
        /// Repositions a row of driver controls according to their position in the race
        /// </summary>
        /// <param name="position">The race position of the driver being relocated</param>
        /// <param name="driverIndex">The index of the driver being relocated</param>
        void PositionDriverControls(int position, int driverIndex)
        {
            int YPosition = MyPadding.Top + (position + 1) * YSpacing;
            driverLabels[driverIndex, 0].Top = YPosition;
            driverLabels[driverIndex, 1].Top = YPosition;
            checkBoxes[driverIndex].Top = YPosition;
            radioButtons[driverIndex].Top = YPosition;
            gapLabels[driverIndex, 0].Top = YPosition + 5;
            gapLabels[driverIndex, 1].Top = YPosition + 5;
            gapLabels[driverIndex, 2].Top = YPosition + 5;
            gapLabels[driverIndex, 3].Top = YPosition + 5;
        }

        void showAll_CheckedChanged(object sender, EventArgs e)
        {
            if (showAll.Checked)
            {
                UpdateCheckBoxes(ShowTracesState.All);
            }
            else
            {
                UpdateCheckBoxes(ShowTracesState.None);
            }
            Graph.ShowTraces();
        }

        /// <summary>
        /// Re-normalises the graph when a change to the check boxes requires it.
        /// </summary>
        void RenormaliseGraphFromDriverSelectPanel()
        {
            Graph.GraphPanel.NormaliseOnBestTrace();
            Data.DriverIndex = Graph.GraphPanel.NormalisationIndex;
            if (NormalisedDriverChanged != null)
                NormalisedDriverChanged();
        }
        void RenormaliseGraphFromDriverSelectPanel(int driver)
        {
            Graph.GraphPanel.SetNormalisationIndex(driver);
            Data.DriverIndex = driver;
            if (NormalisedDriverChanged != null)
                NormalisedDriverChanged();
        }

        void DriverCheckBoxChanged(DriverIndexEventArgs e)
        {
            bool showTrace = checkBoxes[e.DriverIndex].Checked;
            ShowHideTraces(e.DriverIndex, showTrace);
        }

        void ShowHideTraces(int driverToShowOrHide, bool showTrace)
        {
            bool renormaliseAfterUpdate = (selectedDriverIndices.Count == 0);

            //Handle the effects of show/hide
            if (showTrace) { selectedDriverIndices.Add(driverToShowOrHide); }
            else
            {
                selectedDriverIndices.Remove(driverToShowOrHide);
                showAll.CheckedChanged -= showAll_CheckedChanged;
                showAll.Checked = false;
                showAll.CheckedChanged += showAll_CheckedChanged;
            }

            //If hiding the normalised driver, change the normalised driver
            if (!showTrace && driverToShowOrHide == Data.DriverIndex)
            {
                RenormaliseGraphFromDriverSelectPanel();
            }

            //Fire event to signify that the list of shown drivers has changed
            if (DriverSelectionsChanged != null)
                DriverSelectionsChanged();

            Graph.ShowTraces();

            //If this is the only trace on the graph, re-normalise on the only remaining trace.
            if (renormaliseAfterUpdate)
                RenormaliseGraphFromDriverSelectPanel(driverToShowOrHide);
        }

        void NormalisedRadioButtonChecked(DriverIndexEventArgs e)
        {
            if (checkBoxes[e.DriverIndex].Checked) //set as the normalised driver
                ChangeNormalisedDriver(e.DriverIndex);
            else //no changes to be made
                UpdateRadioButtons(Data.DriverIndex);
        }

        void ChangeNormalisedDriver(int checkedDriver)
        {
            Data.DriverIndex = checkedDriver;
            if (NormalisedDriverChanged != null)
                NormalisedDriverChanged();
            Graph.ChangeNormalised(checkedDriver);
        }

        public void UpdateRadioButtons(int normalisedDriverIndex)
        {
            foreach (NormalisedRadioButton r in radioButtons)
            {
                r.CheckedChanged -= r.NormalisedRadioButton_CheckedChanged;
            }

            foreach (NormalisedRadioButton r in radioButtons)
            {
                r.Checked = (r.DriverIndex == normalisedDriverIndex);
            }

            foreach (NormalisedRadioButton r in radioButtons)
            {
                r.CheckedChanged += r.NormalisedRadioButton_CheckedChanged;
            }
        }

        public void UpdateCheckBoxes(ShowTracesState displayState)
        {
            //If the show traces state is either show all or show none.
            if (displayState != ShowTracesState.Selected)
            {
                selectedDriverIndices.Clear();

                foreach (ShowDriverCheckBox c in checkBoxes)
                {
                    c.CheckedChanged -= c.ShowDriverCheckBox_CheckedChanged;
                }

                foreach (ShowDriverCheckBox c in checkBoxes)
                {
                    if (displayState == ShowTracesState.All)
                    {
                        c.Checked = true;
                        selectedDriverIndices.Add(c.DriverIndex);
                        showAll.CheckedChanged -= showAll_CheckedChanged;
                        showAll.Checked = true;
                        showAll.CheckedChanged += showAll_CheckedChanged;
                    }
                    else
                    {
                        c.Checked = false;
                    }
                }

                foreach (ShowDriverCheckBox c in checkBoxes)
                {
                    c.CheckedChanged += c.ShowDriverCheckBox_CheckedChanged;
                }
            }
        }

        public List<int> SelectedDriverIndices
        {
            get { return selectedDriverIndices; }
            set { selectedDriverIndices = value; }
        }

        public bool ShowTimeGaps
        {
            get { return showingTimeGaps; }
            set
            {
                if (showingTimeGaps != value)
                {
                    showingTimeGaps = value;
                    ResetPanel(!showingTimeGaps);
                }
            }
        }
    }
}
