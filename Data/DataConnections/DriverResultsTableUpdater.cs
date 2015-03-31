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
            Result result;
            int driverNumber, raceCalendarIndex;

            //Open the sql connection
            var sqlConnectionString = DataSources.Program.GetConnectionString();
            using (var myConn = new OleDbConnection(sqlConnectionString))
            {
                myConn.Open();

                //For each result
                for (int raceIndex = 0; raceIndex < numberOfTracks; raceIndex++)
                {
                    for (int driverIndex = 0; driverIndex < numberOfDrivers; driverIndex++)
                    {
                        result = results[driverIndex, raceIndex];
                        if (result.modified)
                        {
                            //Get values for the driver number and raceCalendarIndex
                            driverNumber = driverNumbers[driverIndex];
                            raceCalendarIndex = (short)RaceCalendarConnection.GetRaceCalendarID(raceIndex + 1, currentYear);

                            //Check if the row exists
                            var commandText = "SELECT * FROM DriverResults WHERE DriverNumber = @driverNumber AND RaceCalendarIndex = @raceCalendarIndex";
                            var comm = myConn.CreateCommand();
                            comm.CommandText = commandText;
                            comm.CommandType = CommandType.Text;
                            comm.Parameters.AddWithValue("driverNumber", driverNumber);
                            comm.Parameters.AddWithValue("raceCalendarIndex", raceCalendarIndex);
                            var returnValue = comm.ExecuteScalar();

                            if (returnValue != null)
                            {
                                //The record exists. Data can be updated.
                                commandText = "UPDATE DriverResults " +
                                    "SET " +  GetSetStatement(result, Session) +
                                    "WHERE DriverNumber = @driverNumber AND RaceCalendarIndex = @raceCalendarIndex";
                                comm.CommandText = commandText;
                                comm.Parameters.Clear();
                                comm.Parameters.AddWithValue("driverNumber", driverNumber);
                                comm.Parameters.AddWithValue("raceCalendarIndex", raceCalendarIndex);
                                int resultsModified = comm.ExecuteNonQuery();
                                if (resultsModified != 1) { throw new InvalidOperationException("Updated multiple rows in driverResults"); }
                            }
                            else
                            {
                                //The record has to be inserted
                                commandText = "INSERT INTO DriverResults " + GetFieldsToUpdate(Session) + " " +
                                    "VALUES " + GetValuesToInsert(result, Session, driverNumber, raceCalendarIndex);
                                comm.CommandText = commandText;
                                comm.Parameters.Clear();
                                int rowsAdded = comm.ExecuteNonQuery();
                                if (rowsAdded != 1) { throw new InvalidOperationException("Inserted multiple rows in driverResults"); }
                            }
                        }
                    }
                }
            }

            if (DatabaseModified != null)
                DatabaseModified(null, new ResultsUpdatedEventArgs(results, Session));
        }

        private static string GetSetStatement(Result result, Session Session)
        {
            //Set statement is in the form of key/value pairs, i.e. DriverNumber = '44', etc.
            //Driver number and race calendar index will already be set, since this record was found.
            var setStatement = "";
            switch(Session)
            {
                case Session.FP1:
                    setStatement += "FirstPractice='" + result.position.ToString() + "'";
                    break;
                case Session.FP2:
                    setStatement += "SecondPractice='" + result.position.ToString() + "'";
                    break;
                case Session.FP3:
                    setStatement += "ThirdPractice='" + result.position.ToString() + "'";
                    break;
                case Session.Qualifying:
                    setStatement += "QualiPosition='" + result.position.ToString() + "'" + ", ";
                    setStatement += "QualiFinishState='" + ((int)result.finishState).ToString() + "'";
                    break;
                case Session.SpeedTrap:
                    setStatement += "SpeedTrap='" + result.position.ToString() + "'";
                    break;
                case Session.Grid:
                    setStatement += "Grid='" + result.position.ToString() + "'";
                    break;
                case Session.Race:
                    setStatement += "Position='" + result.position.ToString() + "'" + ", ";
                    setStatement += "FinishState='" + ((int)result.finishState).ToString() + "'";
                    break;
            }
            return setStatement;
        }

        private static string GetFieldsToUpdate(Session Session)
        {
            //This is a new row so the driverNumber, raceCalendarIndex, and session need to be updated.
            string fields = "DriverNumber, RaceCalendarIndex, ";
            switch (Session)
            {
                case Session.FP1:
                    fields += "FirstPractice";
                    break;
                case Session.FP2:
                    fields += "SecondPractice";
                    break;
                case Session.FP3:
                    fields += "ThirdPractice";
                    break;
                case Session.Qualifying:
                    fields += "QualiPosition, ";
                    fields += "QualiFinishState";
                    break;
                case Session.SpeedTrap:
                    fields += "SpeedTrap";
                    break;
                case Session.Grid:
                    fields += "Grid";
                    break;
                case Session.Race:
                    fields += "Position, ";
                    fields += "FinishState";
                    break;
            }
            return fields;
        }

        private static string GetValuesToInsert(Result result, Session Session, int driverNumber, int raceCalendarIndex)
        {
            string values = "'" + driverNumber.ToString() + "', ";
            values += "'" + raceCalendarIndex.ToString() + "', ";
            switch (Session)
            {
                case Session.FP1:
                case Session.FP2:
                case Session.FP3:
                case Session.SpeedTrap:
                case Session.Grid:
                    values += "'" + result.position.ToString() + "'";
                    break;
                case Session.Qualifying:
                case Session.Race:
                    values += "'" + result.position.ToString() + "', ";
                    values += "'" + ((int)result.finishState).ToString() + "'";
                    break;
            }
            return values;
        }

        /// <summary>
        /// Note that the results only contain the updated results, not a complete list
        /// </summary>
        public static event EventHandler<ResultsUpdatedEventArgs> DatabaseModified;
    }
}
