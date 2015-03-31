using System.Windows.Forms;

namespace StratSim.View.UserControls
{
    /// <summary>
    /// Represents a tool tip with a custom delay and formatting
    /// </summary>
    class MyToolTip : ToolTip
    {
        /// <summary>
        /// Adds a tool tip with the specified text to a specified control
        /// </summary>
        /// <param name="control">The control to provide the tool tip for</param>
        /// <param name="text">The text to display in the tool tip</param>
        public MyToolTip(Control control, string text)
        {
            AutoPopDelay = 2500;
            InitialDelay = 250;
            ReshowDelay = 500;
            ShowAlways = true;
            this.BackColor = System.Drawing.Color.White;

            SetToolTip(control, text);
        }
    }
}
