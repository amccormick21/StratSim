using DataSources.DataConnections;
using MyFlowLayout;
using StratSim.Model;
using StratSim.Model.Files;
using StratSim.View.MyFlowLayout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StratSim.View.Panels
{
    public class RacePanel : GridBasePanel
    {
        public RacePanel(MainForm parent, int[] gridOrder)
            : base(parent, gridOrder)
        {

        }

        protected override void OnConfirmClicked()
        {
            base.OnConfirmClicked();

            //Update the grid for race start
            UpdateDriverGridPositions();

            //Start race
            ((StratSimPanelControlEvents)PanelControlEvents).OnStartRaceFromStrategies();
        }

        private void UpdateDriverGridPositions()
        {
            int driverIndexOfThisPosition;
            List<int> driverIndices = new List<int>();
            for (int i = 0; i < Data.NumberOfDrivers; i++) { driverIndices.Add(i); }

            int position;
            for (position = 0; position < GridOrder.Items.Count; position++)
            {
                driverIndexOfThisPosition = Driver.ConvertToDriverIndex(Convert.ToString(GridOrder.Items[position]));
                driverIndices.Remove(driverIndexOfThisPosition);
                Data.Drivers[driverIndexOfThisPosition].PracticeTimes.GridPosition = position;
            }

            foreach (int driverIndex in driverIndices)
            {
                Data.Drivers[driverIndex].PracticeTimes.GridPosition = position++;
            }
        }

        protected override string GetConfirmButtonText()
        {
            return "Start Race";
        }
    }
}
