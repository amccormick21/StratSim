using StratSim.Model;
using StratSim.ViewModel;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace StratSim.View.UserControls
{
    /// <summary>
    /// Represents a table layout panel linked to a specified driver and strategy,
    /// to be displayed inside a stint panel and linked to a specific stint.
    /// Contains custom formatting and sizing.
    /// </summary>
    class StintButtonsLayoutPanel : TableLayoutPanel
    {
        StintControlButton Up, Down, AddBefore, Remove;
        Size btnDefault = new Size(21, 21);
        Strategy thisStrategy;
        Driver thisDriver;
        int stintIndex;

        public StintButtonsLayoutPanel(Driver driver, int stintIndex, Strategy strategy)
        {
            thisDriver = driver;
            this.stintIndex = stintIndex;
            thisStrategy = strategy;

            InitialiseButtons();

            this.AutoSize = true;
            this.Size = new Size(42, 42);
            this.RowCount = 2;
            this.ColumnCount = 2;
        }

        void InitialiseButtons()
        {
            MyToolTip tooltip;
            if (stintIndex != 0)
            {
                Up = new StintControlButton(0);
                Up.Size = btnDefault;
                Up.BackgroundImage = Properties.Resources.Stint_Up;
                tooltip = new MyToolTip(Up, "Swap with previous stint");
                Up.OnButtonClicked += ButtonClicked;
                this.Controls.Add(Up, 0, 0);
            }

            if (stintIndex != thisStrategy.NoOfStints - 1)
            {
                Down = new StintControlButton(1);
                Down.Size = btnDefault;
                Down.BackgroundImage = Properties.Resources.Stint_Down;
                tooltip = new MyToolTip(Down, "Swap with next stint");
                Down.OnButtonClicked += ButtonClicked;
                this.Controls.Add(Down, 0, 1);
            }

            AddBefore = new StintControlButton(2);
            AddBefore.Size = btnDefault;
            AddBefore.BackgroundImage = Properties.Resources.Stint_Add;
            tooltip = new MyToolTip(AddBefore, "Add a new stint before this one");
            AddBefore.OnButtonClicked += ButtonClicked;
            this.Controls.Add(AddBefore, 1, 0);

            if (thisStrategy.NoOfStints > 2)
            {
                Remove = new StintControlButton(3);
                Remove.Size = btnDefault;
                Remove.BackgroundImage = Properties.Resources.Stint_Remove;
                tooltip = new MyToolTip(Remove, "Remove this stint from the strategy");
                Remove.OnButtonClicked += ButtonClicked;
                this.Controls.Add(Remove, 1, 1);
            }
        }

        /// <summary>
        /// Handles the button clicked events on any of the buttons within the control.
        /// </summary>
        /// <param name="controlNumber">The control number that fired the event</param>
        void ButtonClicked(int controlNumber)
        {
            int startLapNumber = thisStrategy.Stints[stintIndex].startLap;
            int midLapNumber = (int)((thisStrategy.Stints[stintIndex].stintLength) / 2) + startLapNumber;

            //Performs the required action on the strategy:
            switch (controlNumber)
            {
                case 0: thisStrategy.SwapStints(stintIndex - 1, stintIndex); break; //moves stint up
                case 1: thisStrategy.SwapStints(stintIndex, stintIndex + 1); break; //moves stint down
                case 2: thisStrategy.Stints = thisStrategy.AddPitStop(midLapNumber); break; //splits stint
                case 3: thisStrategy.Stints = thisStrategy.RemovePitStop(startLapNumber); break; //merges stint with previous
            }

            //Updates the strategy's parameters
            thisStrategy.UpdateStrategyParameters();
            MyEvents.OnStrategyModified(thisDriver, thisStrategy, false);
        }

        public Driver Driver
        {
            get { return thisDriver; }
            set { thisDriver = value; }
        }

        public int StintIndex
        {
            get { return stintIndex; }
            set { stintIndex = value; }
        }
    }

    /// <summary>
    /// Represents a button containing a custom event, which passes data
    /// about the button when it is pressed.
    /// </summary>
    class StintControlButton : Button
    {
        int controlNumber;

        public delegate void StintButtonClickEventHandler(int controlNumber);
        public event StintButtonClickEventHandler OnButtonClicked;

        public StintControlButton(int controlNumber)
        {
            this.controlNumber = controlNumber;
            this.Click += StintControlButton_OnClick;
            this.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Padding = new Padding(0);
            this.BackColor = Color.White;
        }

        void StintControlButton_OnClick(object sender, EventArgs e)
        {
            //Fire the custom event containing data about the control
            OnButtonClicked(controlNumber);
        }

        public int ControlNumber
        { get { return controlNumber; } }
    }
}
