using System.Windows.Forms;

namespace MyFlowLayout
{
    /// <summary>
    /// Contains events which control hiding and showing of panels on a form
    /// </summary>
    public class PanelControlEvents
    {
        public delegate void ViewPanelEventHandler(MainForm form);
        public event ViewPanelEventHandler ShowContentTabControl;

        public void OnShowContentTabControl(MainForm form)
        {
            if (ShowContentTabControl != null)
                ShowContentTabControl(form);
        }

        public delegate void ToolStripEventHandler(ToolStripDropDownItem cm);
        public event ToolStripEventHandler ShowToolStrip;
        public event ToolStripEventHandler RemoveToolStrip;

        /// <summary>
        /// Fires the ShowToolStrip event, causing the specified tool strip to be shown on the forms.
        /// </summary>
        public void OnShowToolStrip(ToolStripDropDownItem DropDownItem)
        {
            ShowToolStrip(DropDownItem);
        }
        /// <summary>
        /// Fires the RemoveToolStrip event, causing the specified tool strip to be removed from the forms.
        /// </summary>
        public void OnRemoveToolStrip(ToolStripDropDownItem DropDownItem)
        {
            RemoveToolStrip(DropDownItem);
        }
    }
}
