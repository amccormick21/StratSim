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
            string positionString = "";
            foreach (object element in GridOrder.Items)
            {
                positionString += Convert.ToString(Driver.ConvertToDriverIndex(Convert.ToString(element)));
                positionString += "\r\n";
            }
            return positionString;
        }
    }
}
