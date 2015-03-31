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

            //Write grid information to database
            WriteGridResultsToDatabase();
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
                Data.Drivers[driverIndex].PracticeTimes.GridPosition = position;
            }
        }

        private void WriteGridResultsToDatabase()
        {
            Result[,] results = new Result[Data.NumberOfDrivers, Data.NumberOfTracks];
            int driverIndexOfThisPosition;

            List<int> driverIndices = new List<int>();
            for (int i = 0; i < Data.NumberOfDrivers; i++) { driverIndices.Add(i); }
            int[] gridOrder = new int[Data.NumberOfDrivers];

            int position = 0;
            for (position = 0; position < GridOrder.Items.Count; position++)
            {
                driverIndexOfThisPosition = Driver.ConvertToDriverIndex(Convert.ToString(GridOrder.Items[position]));
                driverIndices.Remove(driverIndexOfThisPosition);
                gridOrder[position] = driverIndexOfThisPosition;
                results[driverIndexOfThisPosition, Data.RaceIndex].position = position + 1;
                results[driverIndexOfThisPosition, Data.RaceIndex].finishState = 0;
                results[driverIndexOfThisPosition, Data.RaceIndex].modified = true;
            }

            foreach (int driverIndex in driverIndices)
            {
                gridOrder[position] = driverIndex;
                results[driverIndex, Data.RaceIndex].position = position++ + 1;
                results[driverIndex, Data.RaceIndex].finishState = FinishingState.DNS;
                results[driverIndex, Data.RaceIndex].modified = true;
            }

            string filePath = GridData.GetTimingDataDirectory(Data.RaceIndex, Properties.Settings.Default.CurrentYear) + GridData.GetFileName(Session.Grid, Data.RaceIndex);
            GridData.WriteToFile(filePath, gridOrder);

            DriverResultsTableUpdater.SetResults(results, Session.Grid, Data.NumberOfDrivers, Data.NumberOfTracks, Properties.Settings.Default.CurrentYear, Driver.GetDriverIndexDictionary(), Driver.GetDriverNumberArray());
        }

        protected override string GetConfirmButtonText()
        {
            return "Start Race";
        }
    }
}
