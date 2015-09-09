using MyFlowLayout;
using StratSim.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StratSim.View.Panels
{
    public class GridPanel : GridBasePanel
    {
        internal event EventHandler<string> PositionsConfirmed;
        
        public GridPanel(MainForm form, int[] gridOrder)
            : base(form, gridOrder)
        {

        }

        protected override void OnConfirmClicked()
        {
            base.OnConfirmClicked();
            if (PositionsConfirmed != null)
                PositionsConfirmed(this, GetPositionString());
        }

        private string GetPositionString()
        {
            int[] fullGridOrder = GetGridOrderFromRaceBox();
            string positionString = "";
            foreach (int driverIndex in fullGridOrder)
            {
                positionString += Convert.ToString(driverIndex);
                positionString += "\r\n";
            }
            return positionString;
        }
    }
}
