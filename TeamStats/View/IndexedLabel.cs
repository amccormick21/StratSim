using StratSim.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TeamStats.View
{
    class IndexedLabel : Label
    {
        int raceIndex;

        public IndexedLabel(int raceIndex)
            : base()
        {
            this.raceIndex = raceIndex;
            InitialiseComponent();
            Text = Data.Tracks[raceIndex].abbreviation;
        }

        void InitialiseComponent()
        {
            var menuStrip = new ContextMenuStrip();
            var resetButton = new ToolStripButton("Reset");
            resetButton.Click += resetButton_Click;
            menuStrip.Items.Add(resetButton);

            ContextMenuStrip = menuStrip;
        }

        void resetButton_Click(object sender, EventArgs e)
        {
            ResetColumn(this, raceIndex);
        }

        internal event EventHandler<int> ResetColumn;
    }
}
