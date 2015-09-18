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
        int driverIndex;
        int stintIndex;

        public StintButtonsLayoutPanel(int driverIndex, int stintIndex)
        {
            this.driverIndex = driverIndex;
            this.stintIndex = stintIndex;

            InitialiseButtons();

            AutoSize = true;
            Size = new Size(42, 42);
            RowCount = 2;
            ColumnCount = 2;
        }

        void InitialiseButtons()
        {
            MyToolTip tooltip;

            Up = new StintControlButton(0);
            Up.Size = btnDefault;
            Up.BackgroundImage = Properties.Resources.Stint_Up;
            tooltip = new MyToolTip(Up, "Swap with previous stint");
            Up.OnButtonClicked += ButtonClicked;
            Controls.Add(Up, 0, 0);

            Down = new StintControlButton(1);
            Down.Size = btnDefault;
            Down.BackgroundImage = Properties.Resources.Stint_Down;
            tooltip = new MyToolTip(Down, "Swap with next stint");
            Down.OnButtonClicked += ButtonClicked;
            Controls.Add(Down, 0, 1);

            AddBefore = new StintControlButton(2);
            AddBefore.Size = btnDefault;
            AddBefore.BackgroundImage = Properties.Resources.Stint_Add;
            tooltip = new MyToolTip(AddBefore, "Add a new stint before this one");
            AddBefore.OnButtonClicked += ButtonClicked;
            Controls.Add(AddBefore, 1, 0);

            Remove = new StintControlButton(3);
            Remove.Size = btnDefault;
            Remove.BackgroundImage = Properties.Resources.Stint_Remove;
            tooltip = new MyToolTip(Remove, "Remove this stint from the strategy");
            Remove.OnButtonClicked += ButtonClicked;
            Controls.Add(Remove, 1, 1);
        }

        /// <summary>
        /// Handles the button clicked events on any of the buttons within the control.
        /// </summary>
        /// <param name="controlNumber">The control number that fired the event</param>
        void ButtonClicked(int controlNumber)
        {
            if (StintOrderChanged != null)
                StintOrderChanged(this, new StintOperationEventArgs(stintIndex, DriverIndex, controlNumber));
        }

        internal event EventHandler<StintOperationEventArgs> StintOrderChanged;

        public int DriverIndex
        {
            get { return driverIndex; }
            set { driverIndex = value; }
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
            Click += StintControlButton_OnClick;
            FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            Padding = new Padding(0);
            BackColor = Color.White;
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
