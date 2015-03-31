using System;
using TeamStats.Functions;
using StratSim.Model;
using System.Data.OleDb;
using System.Data;
using DataSources.DataConnections;

namespace TeamStats.ViewModel
{
    class ResultData
    {
        Result[,] results; //driverIndex, round = position
        Session session;

        internal event EventHandler ResultsModified;

        public ResultData(Session session)
        {
            this.session = session;

            DriverResultsTableUpdater.DatabaseModified += DriverResultsTableUpdater_DatabaseModified;

            GetResultsFromData(session);
        }

        void DriverResultsTableUpdater_DatabaseModified(object sender, ResultsUpdatedEventArgs e)
        {
            this.session = e.session;
            GetResultsFromData(e.session);
            if (ResultsModified != null)
                ResultsModified(this, new EventArgs());
        }

        public void SetSession(Session session)
        {
            this.session = session;
            GetResultsFromData(session);
        }

        private void GetResultsFromData(Session session)
        {
            results = StatisticManager.GetResultsFromDatabase(session);
            if (ResultsModified != null)
                ResultsModified(this, new EventArgs());
        }
        
        internal void SetResultsInDatabase(Result[,] results)
        {
            StatisticManager.SetResults(results, session);
        }

        /// <summary>
        /// Returns an array of driver numbers, indexed by their race position
        /// </summary>
        /// <param name="round">The race round to retrieve data from</param>
        internal Result[] GetResultsForRoundByPosition(int round)
        {
            Result[] resultsByPosition = new Result[Data.NumberOfDrivers];

            Result result;
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                result = results[driverIndex, round];

                if (result.position != 0)
                {
                    resultsByPosition[result.position - 1].position = Data.Drivers[driverIndex].DriverNumber;
                    resultsByPosition[result.position - 1].finishState = result.finishState;
                }
            }

            return resultsByPosition;
        }

        /// <summary>
        /// Returns an array of race positions, indexed by the driver index.
        /// </summary>
        /// <param name="round">The race round to retrieve data from</param>
        /// <returns></returns>
        internal Result[] GetResultsForRoundByDriver(int round)
        {
            Result[] resultsByDriver = new Result[Data.NumberOfDrivers];

            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                resultsByDriver[driverIndex] = results[driverIndex, round];
            }

            return resultsByDriver;
        }

        internal void ShowChangesToResults(Result[,] results)
        {
            StatisticManager.OnShowComparison(results, session);
        }

        internal void HideChangesToResults()
        {
            StatisticManager.OnClearComparison();
        }
    }
}
