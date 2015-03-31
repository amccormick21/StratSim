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

        Strategy thisStrategy;
        Driver thisDriver;

        int originalLength;
        int upperBound;

        private int stintIndex;

        public StintPanel(int stint, Driver driver, Strategy strategy)
        {
            this.Size = new Size(280, 100);
            stintIndex = stint;
            thisDriver = driver;
            thisStrategy = (Strategy)strategy.Clone();
            AddControls();
        }

        void AddControls()
        {
            MyToolTip toolTip;

            stintLabel = new Label();
            stintLabel.Location = new Point(10, 10);
            stintLabel.Size = lblDefault;
            stintLabel.Text = "Stint " + (this.StintNumber + 1);
            this.Controls.Add(stintLabel);

            //Start the stint buttons layout panel.
            Buttons = new StintButtonsLayoutPanel(thisDriver, stintIndex, thisStrategy);
            Buttons.Location = new Point(10, 40);
            this.Controls.Add(Buttons);

            length = new Label();
            length.Location = new Point(100, 10);
            length.Size = lblDefault;
            length.Text = "Length";
            this.Controls.Add(length);

            time = new Label();
            time.Location = new Point(100, 35);
            time.Size = lblDefault;
            time.Text = "Time";
            this.Controls.Add(time);

            tyre = new Label();
            tyre.Location = new Point(100, 60);
            tyre.Size = lblDefault;
            tyre.Text = "Tyre";
            this.Controls.Add(tyre);

            //calculate the length and upper bounds.
            originalLength = thisStrategy.Stints[stintIndex].stintLength;

            //upper bound depends on either previous or next stint
            if (stintIndex == thisStrategy.NoOfStints - 1) //if stint is last in strategy
            {
                upperBound = originalLength + thisStrategy.Stints[stintIndex - 1].stintLength;
            }
            else
            {
                upperBound = originalLength + thisStrategy.Stints[stintIndex + 1].stintLength;
            }

            stintLength = new TextBox();
            stintLength.Size = txtDefault;
            stintLength.Location = new Point(200, 10);
            stintLength.Text = Convert.ToString(originalLength);
            stintLength.BorderStyle = System.Windows.Forms.BorderStyle.None;
            toolTip = new MyToolTip(stintLength, "The laps in this stint");
            stintLength.LostFocus += stintLength_LostFocus;
            this.Controls.Add(stintLength);

            stintTime = new Label();
            stintTime.Size = lblDefault;
            stintTime.Location = new Point(200, 35);
            stintTime.Text = Convert.ToString(thisStrategy.Stints[stintIndex].TotalTime());
            stintTime.BorderStyle = System.Windows.Forms.BorderStyle.None;
            toolTip = new MyToolTip(stintTime, "The total time for this stint");
            this.Controls.Add(stintTime);

            tyreType = new ComboBox();
            foreach (var type in (TyreType[])Enum.GetValues(typeof(TyreType)))
            {
                tyreType.Items.Add(Convert.ToString(type));
            }
            tyreType.Size = txtDefault;
            tyreType.Location = new Point(200, 60);
            tyreType.SelectedIndex = (int)thisStrategy.Stints[stintIndex].tyreType;
            tyreType.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            tyreType.DropDownStyle = ComboBoxStyle.DropDownList;
            tyreType.SelectedIndexChanged += tyreType_SelectedIndexChanged;
            toolTip = new MyToolTip(tyreType, "The tyre type for this stint");
            this.Controls.Add(tyreType);
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
                newStintLaps = (int)Functions.ValidateBetweenExclusive(newStintLaps, 0, upperBound, "stint length", ref incorrectValue, true);
            }
            if (!incorrectValue)
            {
                thisStrategy.Stints = thisStrategy.ChangeStintLength(stintIndex, newStintLaps);
            }

            //If something has changed, update the stint and the strategy.
            if (newStintLaps != originalLength)
            {
                originalLength = newStintLaps;
                stintLength.LostFocus -= stintLength_LostFocus;
                thisStrategy.UpdateStrategyParameters();
                MyEvents.OnStrategyModified(thisDriver, thisStrategy, false);
            }
        }

        /// <summary>
        /// Occurs when the user changes the tyre type selected for this stint.
        /// </summary>
        void tyreType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Changes the tyre type of the stint.
            thisStrategy.ChangeStintTyreType(stintIndex, (TyreType)tyreType.SelectedIndex);

            tyreType.SelectedIndexChanged -= tyreType_SelectedIndexChanged;
        }

        public Driver Driver
        {
            get{return thisDriver;}
            set{thisDriver = value;}
        }
        public int StintNumber
        {
            get{return stintIndex;}
            set{stintIndex = value;}
        }
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
            get{return driverIndex;}
            set{driverIndex = value;}
        }
    }
}
