using StratSim.Model;
using System;

namespace StratSim.View.MyFlowLayout
{
    /// <summary>
    /// Represents a tool strip button linked to a track.
    /// </summary>
    class RaceDropDown : MyToolStripButton
    {
        public RaceDropDown(int Index)
            : base(Index)
        {
            this.Text = Data.Tracks[Index].name;
        }

        public override void GenerateClickEvent(object sender, EventArgs e)
        {
            base.GenerateClickEvent(sender, e);
            this.Checked = false;
        }
    }
}
