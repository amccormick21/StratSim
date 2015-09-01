using DataSources.StratSimDataSetTableAdapters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace DataSources.DataConnections
{
    public static class DriverResultsTableUpdater
    {
        public static Result[,] GetResultsFromDatabase(Session Session, int numberOfDrivers, int numberOfTracks, int currentYear, Dictionary<int, int> driverIndexDictionary)
        {
            Result[,] raceResults = new Result[numberOfDrivers, numberOfTracks];

            //Get all driver results rows when year = currentYear
            var sqlConnectionString = Program.GetConnectionString();
            using (OleDbConnection myConn = new OleDbConnection(sqlConnectionString))
            {
                myConn.Open();
                string commandText = "SELECT DriverNumber, [Round], FirstPractice, SecondPractice, ThirdPractice, QualiPosition, QualiFinishState, SpeedTrap, Grid, [Position], FinishState " +
                    "FROM DriverResults, RaceCalendar " +
                    "WHERE RaceCalendarID = RaceCalendarIndex " +
                    "AND TrackYear = @RaceYear";
                OleDbCommand comm = myConn.CreateCommand();
                comm.CommandText = commandText;
                comm.Parameters.AddWithValue("RaceYear", currentYear);
                comm.CommandType = System.Data.CommandType.Text;
                using (OleDbDataReader reader = comm.ExecuteReader())
                {
                    int driverIndex;
                    int trackIndex;
                    while (reader.Read())
                    {
                        driverIndex = driverIndexDictionary[Convert.ToInt32(reader[0])];
                        trackIndex = Convert.ToInt32(reader[1]) - 1;
                        raceResults[driverIndex, trackIndex] = GetResultFromRow(reader, Session);
                    }
                }
            }

            return raceResults;
        }

        private static Result GetResultFromRow(IDataReader row, Session session)
        {
            Result result = new Result();
            result.modified = true;

            switch (session)
            {
                case Session.FP1:
                    result.position = Convert.ToInt32(row[2]);
                    result.finishState = FinishingState.Finished;
                    break;
                case Session.FP2:
                    result.position = Convert.ToInt32(row[3]);
                    result.finishState = FinishingState.Finished;
                    break;
                case Session.FP3:
                    result.position = Convert.ToInt32(row[4]);
                    result.finishState = FinishingState.Finished;
                    break;
                case Session.Qualifying:
                    result.position = Convert.ToInt32(row[5]);
                    result.finishState = (FinishingState)Convert.ToInt32(row[6]);
                    break;
                case Session.SpeedTrap:
                    result.position = Convert.ToInt32(row[7]);
                    result.finishState = FinishingState.Finished;
                    break;
                case Session.Grid:
                    result.position = Convert.ToInt32(row[8]);
                    result.finishState = FinishingState.Finished;
                    break;
                case Session.Race:
                    result.position = Convert.ToInt32(row[9]);
                    result.finishState = (FinishingState)Convert.ToInt32(row[10]);
                    break;

            }

            return result;
        }

        public static void SetResults(Result[,] results, Session Session, int numberOfDrivers, int numberOfTracks, int currentYear, Dictionary<int, int> driverIndexDictionary, int[] driverNumbers)
        {
            //TODO: return records via query and use update/insert
            var stratSimDataSet = new StratSimDataSet();
            var driverResultsTableAdapter = new DriverResultsTableAdapter();
            var raceCalendarTableAdapter = new RaceCalendarTableAdapter();
            driverResultsTableAdapter.Fill(stratSimDataSet.DriverResults);
            raceCalendarTableAdapter.Fill(stratSimDataSet.RaceCalendar);
            StratSimDataSet.DriverResultsRow row;

            bool[,] rowExists = new bool[numberOfDrivers, numberOfTracks];
            int databaseDriverIndex, databaseTrackIndex;
            int driverNumber;

            //Modify the existing rows and set a flag if they are found.
            for (int rowIndex = 0; rowIndex < stratSimDataSet.DriverResults.Count; rowIndex++)
            {
                row = stratSimDataSet.DriverResults[rowIndex];
                if (currentYear == row.RaceCalendarRow.TrackYear)
                {
                    driverNumber = Convert.ToInt32(row.DriverNumber);
                    databaseDriverIndex = driverIndexDictionary[driverNumber];
                    databaseTrackIndex = row.RaceCalendarRow.Round - 1;
                    if (results[databaseDriverIndex, databaseTrackIndex].modified)
                    {
                        rowExists[databaseDriverIndex, databaseTrackIndex] = true;
                        ModifyRow(ref row, results[databaseDriverIndex, databaseTrackIndex], Session);
                    }
                    driverResultsTableAdapter.Update(row);
                }
            }

            //Create new rows if the results have not been found.
            int yearOffset = (currentYear - 2014) * numberOfDrivers * numberOfTracks;
            for (int raceIndex = 0; raceIndex < numberOfTracks; raceIndex++)
            {
                for (int driverIndex = 0; driverIndex < numberOfDrivers; driverIndex++)
                {

                    if (results[driverIndex, raceIndex].modified && !rowExists[driverIndex, raceIndex])
                    {
                        row = stratSimDataSet.DriverResults.NewDriverResultsRow();
                        driverNumber = driverNumbers[driverIndex];
                        row.DriverNumber = (short)driverNumber;
                        row.RaceCalendarIndex = (short)RaceCalendarConnection.GetRaceCalendarID(raceIndex + 1, currentYear);
                        row.DriverResultID = (short)(stratSimDataSet.DriverResults.Count + 1);
                        ModifyRow(ref row, results[driverIndex, raceIndex], Session);
                        stratSimDataSet.DriverResults.AddDriverResultsRow(row);
                        driverResultsTableAdapter.Update(row);
                    }
                }
            }

            if (DatabaseModified != null)
                DatabaseModified(null, new ResultsUpdatedEventArgs(results, Session));
        }

        /// <summary>
        /// Note that the results only contain the updated results, not a complete list
        /// </summary>
        public static event EventHandler<ResultsUpdatedEventArgs> DatabaseModified;

        private static void ModifyRow(ref StratSimDataSet.DriverResultsRow row, Result result, Session Session)
        {
            for (int column = 3; column < row.ItemArray.Length; column++)
            {
                if (row[column] == DBNull.Value)
                    row[column] = 0;
            }

            switch (Session)
            {
                case Session.FP1:
                    row[3] = result.position;
                    break;
                case Session.FP2:
                    row[4] = result.position;
                    break;
                case Session.FP3:
                    row[5] = result.position;
                    break;
                case Session.Qualifying:
                    row[6] = result.position;
                    row[7] = result.finishState;
                    break;
                case Session.SpeedTrap:
                    row[8] = result.position;
                    break;
                case Session.Grid:
                    row[9] = result.position;
                    break;
                case Session.Race:
                    row[10] = result.position;
                    row[11] = result.finishState;
                    break;
            }
        }
    }
}
