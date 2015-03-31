using StratSim.Model;
using StratSim.View.Panels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using StratSim.ViewModel;

namespace StratSim.View.UserControls
{
    /// <summary>
    /// Represents options for the status of a 'Show All' check box
    /// </summary>
    public enum ShowTracesState { All, None, Selected };

    /// <summary>
    /// Represents a radio button which fires a custom event containing the index of the driver
    /// associated with the control.
    /// </summary>
    class NormalisedRadioButton : RadioButton, IDriverIndexControl
    {
        int driverIndex;

        /// <summary>
        /// Creates a new instance of a NormalisedRadioButton, associated with the specified driver index
        /// </summary>
        /// <param name="driverIndex">The driver index associated with the control</param>
        public NormalisedRadioButton(int driverIndex)
        {
            this.driverIndex = driverIndex;
            CheckedChanged += NormalisedRadioButton_CheckedChanged;
        }

        public void NormalisedRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (this.Checked && ControlActivated != null)
                ControlActivated(new DriverIndexEventArgs(this.DriverIndex));
        }

        public int DriverIndex
        {
            get { return driverIndex; }
            set { driverIndex = value; }
        }

        public event DriverIndexEvent ControlActivated;
    }

    /// <summary>
    /// Represents a check box that contains an event on click containing data about
    /// the driver index associated with the control
    /// </summary>
    class ShowDriverCheckBox : CheckBox, IDriverIndexControl
    {
        int driverIndex;

        /// <summary>
        /// Creates a new instance of a ShowDriverCheckBox, associated to the specified driver index
        /// </summary>
        /// <param name="driverIndex">The driver index associated to the control</param>
        public ShowDriverCheckBox(int driverIndex)
        {
            this.driverIndex = driverIndex;
            CheckedChanged += ShowDriverCheckBox_CheckedChanged;
        }

        public void ShowDriverCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ControlActivated(new DriverIndexEventArgs(this.DriverIndex));
        }

        public int DriverIndex
        {
            get { return driverIndex; }
            set { driverIndex = value; }
        }

        public event DriverIndexEvent ControlActivated;
    }

}
