using DataSources;
using StratSim.Model;
using StratSim.ViewModel;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace StratSim.View.UserControls
{
    /// <summary>
    /// Represents a panel which displays data about a specified strategy.
    /// Contains methods for modifying the strategy if necessary.
    /// </summary>
    class StintPanel : Panel
    {
        const int leftBorder = 15;
        const int topBorder = 25;
        const int horizontalBorder = 10;
        const int defaultHeight = 20;
        const int defaultWidth = 70;
        const int wideWidth = 100;

        Size txtDefault = new Size(defaultWidth, defaultHeight);
        Size lblDefault = new Size(defaultWidth, defaultHeight);

        StintButtonsLayoutPanel Buttons;
        TextBox stintLength;
        ComboBox tyreType;
        Label stintLabel, length, time, tyre, stintTime;

        internal int DriverIndex { get; private set; }
        internal int StintLength { get; private set; }
        internal int StintLengthUpperBound { get; private set; }
        internal TyreType TyreType { get; private set; }
        internal float TotalTime { get; private set; }
        internal int StintIndex { get; private set; }

        public StintPanel(int stintIndex, int stintLength, TyreType tyreType, float totalTime, int driverIndex, int stintLengthUpperBound)
        {
            Size = new Size(280, 100);
            StintIndex = stintIndex;
            DriverIndex = driverIndex;
            StintLength = stintLength;
            TyreType = tyreType;
            TotalTime = totalTime;
            StintLengthUpperBound = stintLengthUpperBound;
            AddControls();
        }

        void AddControls()
        {
            MyToolTip toolTip;

            stintLabel = new Label();
            stintLabel.Location = new Point(10, 10);
            stintLabel.Size = lblDefault;
            stintLabel.Text = "Stint " + (StintIndex + 1);
            Controls.Add(stintLabel);

            //Start the stint buttons layout panel.
            Buttons = new StintButtonsLayoutPanel(DriverIndex, StintIndex);
            Buttons.StintOrderChanged += Buttons_StintOrderChanged;
            Buttons.Location = new Point(10, 40);
            Controls.Add(Buttons);

            length = new Label();
            length.Location = new Point(100, 10);
            length.Size = lblDefault;
            length.Text = "Length";
            Controls.Add(length);

            time = new Label();
            time.Location = new Point(100, 35);
            time.Size = lblDefault;
            time.Text = "Time";
            Controls.Add(time);

            tyre = new Label();
            tyre.Location = new Point(100, 60);
            tyre.Size = lblDefault;
            tyre.Text = "Tyre";
            Controls.Add(tyre);

            stintLength = new TextBox();
            stintLength.Size = txtDefault;
            stintLength.Location = new Point(200, 10);
            stintLength.Text = Convert.ToString(StintLength);
            stintLength.BorderStyle = System.Windows.Forms.BorderStyle.None;
            toolTip = new MyToolTip(stintLength, "The laps in this stint");
            stintLength.LostFocus += stintLength_LostFocus;
            Controls.Add(stintLength);

            stintTime = new Label();
            stintTime.Size = lblDefault;
            stintTime.Location = new Point(200, 35);
            stintTime.Text = Convert.ToString(TotalTime);
            stintTime.BorderStyle = System.Windows.Forms.BorderStyle.None;
            toolTip = new MyToolTip(stintTime, "The total time for this stint");
            Controls.Add(stintTime);

            tyreType = new ComboBox();
            foreach (var type in (TyreType[])Enum.GetValues(typeof(TyreType)))
            {
                tyreType.Items.Add(Convert.ToString(type));
            }
            tyreType.Size = txtDefault;
            tyreType.Location = new Point(200, 60);
            tyreType.SelectedIndex = (int)TyreType;
            tyreType.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            tyreType.DropDownStyle = ComboBoxStyle.DropDownList;
            tyreType.SelectedIndexChanged += tyreType_SelectedIndexChanged;
            toolTip = new MyToolTip(tyreType, "The tyre type for this stint");
            Controls.Add(tyreType);
        }

        private void Buttons_StintOrderChanged(object sender, StintOperationEventArgs e)
        {
            if (StintOrderChanged != null)
                StintOrderChanged(this, e);
        }

        /// <summary>
        /// Occurs when the user moves away from the text box containing the stint length
        /// </summary>
        void stintLength_LostFocus(object sender, EventArgs e)
        {
            int newStintLaps;

            //Validates the input
            bool incorrectValue = !int.TryParse(stintLength.Text, out newStintLaps);

            if (!incorrectValue)
            {
                newStintLaps = (int)Functions.ValidateBetweenExclusive(newStintLaps, 0, StintLengthUpperBound, "stint length", ref incorrectValue, true);
            }
            if (!incorrectValue)
            {
                if (newStintLaps != StintLength && StintLengthChanged != null)
                {
                    StintLength = newStintLaps;
                    stintLength.LostFocus -= stintLength_LostFocus;
                    StintLengthChanged(this, new StintLengthChangedEventArgs(StintIndex, DriverIndex, newStintLaps));
                }
            }
        }

        /// <summary>
        /// Occurs when the user changes the tyre type selected for this stint.
        /// </summary>
        void tyreType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TyreTypeChanged != null)
                TyreTypeChanged(this, new TyreTypeChangedEventArgs(StintIndex, DriverIndex, (TyreType)tyreType.SelectedIndex));
            tyreType.SelectedIndexChanged -= tyreType_SelectedIndexChanged;
        }

        internal event EventHandler<StintLengthChangedEventArgs> StintLengthChanged;
        internal event EventHandler<TyreTypeChangedEventArgs> TyreTypeChanged;
        internal event EventHandler<StintOperationEventArgs> StintOrderChanged;
    }

    /// <summary>
    /// Represents a panel that contains summary information about a strategy.
    /// </summary>
    class ResultsPanel : FlowLayoutPanel
    {
        Strategy strategy;
        int driverIndex;

        const int leftBorder = 15;
        const int topBorder = 25;
        const int horizontalBorder = 10;
        const int defaultHeight = 20;
        const int defaultWidth = 70;
        const int wideWidth = 100;

        Size txtDefault = new Size(defaultWidth, defaultHeight);
        Size lblDefault = new Size(defaultWidth - 10, defaultHeight + 5);
        Size lblWide = new Size(wideWidth, defaultHeight + 5);

        Label totalTime, averageLap, pitStops, fastestLapLabel, fuelRequired;
        TextBox time, a_lap, stops, f_lap, fuel;

        /// <summary>
        /// Creates a new instance of a ResultsPanel, linked to the specified driver and strategy.
        /// </summary>
        public ResultsPanel(int driverIndex, Strategy thisStrategy)
        {
            this.AutoSize = true;
            this.MaximumSize = new Size(400, 150);
            this.FlowDirection = FlowDirection.TopDown;
            this.Margin = new System.Windows.Forms.Padding(10, 10, 5, 5);
            this.driverIndex = driverIndex;
            this.strategy = thisStrategy;
            AddControls();
        }

        void AddControls()
        {
            totalTime = new Label();
            totalTime.Size = lblWide;
            totalTime.Text = "Total Time:";
            this.Controls.Add(totalTime);

            averageLap = new Label();
            averageLap.Size = lblWide;
            averageLap.Text = "Average Lap:";
            this.Controls.Add(averageLap);

            fastestLapLabel = new Label();
            fastestLapLabel.Size = lblWide;
            fastestLapLabel.Text = "Fastest Lap:";
            this.Controls.Add(fastestLapLabel);

            pitStops = new Label();
            pitStops.Size = lblWide;
            pitStops.Text = "Pit Stops:";
            this.Controls.Add(pitStops);

            fuelRequired = new Label();
            fuelRequired.Size = lblWide;
            fuelRequired.Text = "Fuel Required:";
            this.Controls.Add(fuelRequired);

            time = new TextBox();
            time.Size = txtDefault;
            time.Text = Convert.ToString(strategy.TotalTime);
            time.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Controls.Add(time);

            a_lap = new TextBox();
            a_lap.Size = txtDefault;
            a_lap.BorderStyle = System.Windows.Forms.BorderStyle.None;
            a_lap.Text = Convert.ToString(strategy.TotalTime / strategy.LapTimes.Length);
            this.Controls.Add(a_lap);

            float fastestLap = Data.Settings.DefaultPace;
            foreach (Stint s in strategy.Stints)
            {
                if (s.FastestLap() < fastestLap)
                    fastestLap = s.FastestLap();
            }

            f_lap = new TextBox();
            f_lap.Size = txtDefault;
            f_lap.Text = Convert.ToString(fastestLap);
            f_lap.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Controls.Add(f_lap);

            string pitStopList = "";
            foreach (int pit in strategy.PitStops)
            {
                pitStopList += Convert.ToString(pit);
                pitStopList += ", ";
            }
            pitStopList.TrimEnd(',', ' ');

            stops = new TextBox();
            stops.Size = txtDefault;
            stops.Text = pitStopList;
            stops.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Controls.Add(stops);

            fuel = new TextBox();
            fuel.Size = txtDefault;
            fuel.Text = Convert.ToString(strategy.FuelUsed);
            fuel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Controls.Add(fuel);
        }

        public int DriverIndex
        {
            get { return driverIndex; }
            set { driverIndex = value; }
        }
    }
}
