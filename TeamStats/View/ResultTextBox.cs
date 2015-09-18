using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using DataSources.DataConnections;

namespace TeamStats.View
{
    class ResultTextBox : TableTextBox
    {
        static Color[] finishStateColours = { Color.White, Color.Yellow, Color.LightGray, Color.DarkGray, Color.Gray, Color.LightBlue };
        ContextMenuStrip thisContextMenu;

        public FinishingState FinishingState { get; private set; }

        public ResultTextBox(int row, int column)
            : base (row, column)
        {
            InitialiseContextMenu();
            AddContextMenu();
            LostFocus += ResultTextBox_LostFocus;
        }

        void ResultTextBox_LostFocus(object sender, EventArgs e)
        {
            if (textChanged)
                ForeColor = System.Drawing.Color.Red;
        }

        protected override void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down || e.KeyCode == Keys.Space)
            {
                if (textChanged)
                    ForeColor = System.Drawing.Color.Red;
            }
            base.KeyPressed(sender, e);
        }

        public void ResetParameters()
        {
            //Don't change the finishing state but reset the colour from any modified colour.
            ForeColor = System.Drawing.Color.Black;
            SetFinishingState(FinishingState);
            textChanged = false;
        }

        public void SetFinishingState(FinishingState state)
        {
            FinishingState = state;
            this.BackColor = finishStateColours[(int)FinishingState];

            foreach (var finishingState in (FinishingState[])Enum.GetValues(typeof(FinishingState)))
            {
                ((IndexedToolStripButton)thisContextMenu.Items[(int)finishingState]).Checked = (finishingState == state);
            }
        }

        private void InitialiseContextMenu()
        {
            thisContextMenu = new ContextMenuStrip();

            IndexedToolStripButton button;
            foreach (var finishState in (FinishingState[])Enum.GetValues(typeof(FinishingState)))
            {
                button = new IndexedToolStripButton((int)finishState)
                {
                    Text = Convert.ToString(finishState),
                };
                button.ButtonClicked += button_ButtonClicked;
                thisContextMenu.Items.Add(button);
            }

        }

        public event EventHandler<FinishingState> FinishingStateChanged;
        void button_ButtonClicked(object sender, int e)
        {
            textChanged = true;
            SetFinishingState((FinishingState)e);
            if (FinishingStateChanged != null)
                FinishingStateChanged(this, FinishingState);
        }

        public void RemoveContextMenu()
        {
            this.ContextMenuStrip = null;
            thisContextMenu.Dispose();
        }

        public void AddContextMenu()
        {
            this.ContextMenuStrip = thisContextMenu;
        }

    }
}
