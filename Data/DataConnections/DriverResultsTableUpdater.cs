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
                    "WHERE DriverResults.RaceCalendarIndex = RaceCalendar.RaceCalendarID " +
                    "AND RaceCalendar.TrackYear = @RaceYear";
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

        private static OleDbCommand GetSQLUpdateCommand(OleDbConnection connection, Result result, Session session, int driverNumber, int trackID)
        {
            OleDbCommand comm = connection.CreateCommand();
            comm.CommandType = CommandType.Text;
            string sqlStatement = "UPDATE DriverResults SET ";
            comm.Parameters.AddWithValue("Result", result.position);
            switch (session)
            {
                case Session.FP1:
                    sqlStatement += "FirstPractice=@Result ";
                    break;
                case Session.FP2:
                    sqlStatement += "SecondPractice=@Result ";
                    break;
                case Session.FP3:
                    sqlStatement += "ThirdPractice=@Result ";
                    break;
                case Session.Qualifying:
                    sqlStatement += "QualiPosition=@Result, ";
                    sqlStatement += "QualiFinishState=@FinishState ";
                    comm.Parameters.AddWithValue("FinishState", Convert.ToInt32(result.finishState));
                    break;
                case Session.SpeedTrap:
                    sqlStatement += "SpeedTrap=@Result ";
                    break;
                case Session.Grid:
                    sqlStatement += "Grid=@Result ";
                    break;
                case Session.Race:
                    sqlStatement += "Position=@Result, ";
                    sqlStatement += "FinishState=@FinishState ";
                    comm.Parameters.AddWithValue("FinishState", Convert.ToInt32(result.finishState));
                    break;
            }
            sqlStatement += "WHERE DriverResults.DriverNumber = @DriverNumber ";
            sqlStatement += "AND DriverResults.RaceCalendarIndex = @TrackID";
            comm.CommandText = sqlStatement;
            comm.Parameters.AddWithValue("DriverNumber", driverNumber);
            comm.Parameters.AddWithValue("TrackID", trackID);
            return comm;
        }

        private static void UpdateRow(OleDbConnection myConn, Result result, Session session, int driverNumber, int trackID)
        {
            OleDbCommand comm = GetSQLUpdateCommand(myConn, result, session, driverNumber, trackID);
            comm.ExecuteNonQuery();
        }

        private static string SessionColumns(Session session)
        {
            string columnNames = "";
            switch (session)
            {
                case Session.FP1:
                    columnNames += "FirstPractice";
                    break;
                case Session.FP2:
                    columnNames += "SecondPractice";
                    break;
                case Session.FP3:
                    columnNames += "ThirdPractice";
                    break;
                case Session.Qualifying:
                    columnNames += "QualiPosition, ";
                    columnNames += "QualiFinishState";
                    break;
                case Session.SpeedTrap:
                    columnNames += "SpeedTrap";
                    break;
                case Session.Grid:
                    columnNames += "Grid";
                    break;
                case Session.Race:
                    columnNames += "Position, ";
                    columnNames += "FinishState";
                    break;
            }
            return columnNames;
        }

        private static string SessionParameters(Result result, Session session, ref OleDbCommand command)
        {
            string parameters = "@Result";
            command.Parameters.AddWithValue("Result", result.position);
            switch (session)
            {
                case Session.Qualifying:
                case Session.Race:
                    parameters += ", @FinishState";
                    command.Parameters.AddWithValue("FinishState", result.finishState);
                    break;
            }
            return parameters;
        }

        private static OleDbCommand GetSQLInsertCommand(OleDbConnection connection, Result result, Session session, int driverNumber, int raceCalendarID, int resultID)
        {
            OleDbCommand comm = connection.CreateCommand();
            comm.CommandType = CommandType.Text;
            string sqlStatement = "INSERT INTO DriverResults (RaceCalendarIndex,DriverResultID,DriverNumber,";
            sqlStatement += SessionColumns(session);
            sqlStatement += ") VALUES (@CalendarIndex,@ResultID,@DriverNumber,";
            comm.Parameters.AddWithValue("CalendarIndex", raceCalendarID);
            comm.Parameters.AddWithValue("ResultID", resultID);
            comm.Parameters.AddWithValue("DriverNumber", driverNumber);
            sqlStatement += SessionParameters(result, session, ref comm);
            sqlStatement += ")";
            comm.CommandText = sqlStatement;
            return comm;
        }

        private static void InsertRow(OleDbConnection myConn, Result result, Session session, int driverNumber, int calendarID, int resultID)
        {
            OleDbCommand comm = GetSQLInsertCommand(myConn, result, session, driverNumber, calendarID, resultID);
            comm.ExecuteNonQuery();
        }

        private static int GetTrackID(OleDbConnection myConn, int trackIndex, int currentYear)
        {
            int calendarID;
            OleDbCommand comm = myConn.CreateCommand();
            comm.CommandType = CommandType.Text;
            comm.CommandText = "SELECT RaceCalendar.RaceCalendarID FROM RaceCalendar WHERE [Round] = @RaceRound AND TrackYear = @RaceYear";
            comm.Parameters.AddWithValue("RaceRound", trackIndex + 1);
            comm.Parameters.AddWithValue("RaceYear", currentYear);
            calendarID = (int)comm.ExecuteScalar();
            return calendarID;
        }

        private static int GetResultID(int numberOfDrivers, int numberOfTracks, int year, int driverIndex, int trackIndex)
        {
            //TODO: check number of drivers in every previous year.
            int resultID = (year - 2014) * 19 * 22;
            resultID += trackIndex * numberOfDrivers;
            resultID += driverIndex + 1;
            return resultID;
        }

        public static void SetResults(Result[,] results, Session session, int numberOfDrivers, int numberOfTracks, int currentYear, Dictionary<int, int> driverIndexDictionary, int[] driverNumbers)
        {
            int trackCalendarID, resultID;
            var sqlConnectionString = Program.GetConnectionString();
            using (OleDbConnection myConn = new OleDbConnection(sqlConnectionString))
            {
                myConn.Open();
                for (int trackIndex = 0; trackIndex < numberOfTracks; trackIndex++)
                {
                    trackCalendarID = GetTrackID(myConn, trackIndex, currentYear);
                    for (int driverIndex = 0; driverIndex < numberOfDrivers; driverIndex++)
                    {
                        //Set the result ID
                        resultID = GetResultID(numberOfDrivers, numberOfTracks, currentYear, driverIndex, trackIndex);

                        //If any change needs to be made at all
                        if (results[driverIndex, trackIndex].modified)
                        {
                            //Run a query to check the row exists
                            if (DriverResultsRowExists(myConn, resultID))
                            {
                                //The record exists and has been loaded
                                UpdateRow(myConn, results[driverIndex, trackIndex], session, driverNumbers[driverIndex], trackCalendarID);
                            }
                            else
                            {
                                //The record does not exist and must be created
                                InsertRow(myConn, results[driverIndex, trackIndex], session, driverNumbers[driverIndex], trackCalendarID, resultID);
                            }
                        }
                    }
                }
            }

            /*
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
                        ModifyRow(ref row, results[databaseDriverIndex, databaseTrackIndex], session);
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
                        ModifyRow(ref row, results[driverIndex, raceIndex], session);
                        stratSimDataSet.DriverResults.AddDriverResultsRow(row);
                        driverResultsTableAdapter.Update(row);
                    }
                }
            }
            */
            if (DatabaseModified != null)
                DatabaseModified(null, new ResultsUpdatedEventArgs(results, session));
        }

        private static bool DriverResultsRowExists(OleDbConnection myConn, int resultID)
        {
            //Check the number of responses to a query.
            bool rowExists = false;
            var sqlConnectionString = Program.GetConnectionString();
            OleDbCommand comm = myConn.CreateCommand();
            comm.CommandType = CommandType.Text;
            comm.CommandText = "SELECT DriverNumber FROM DriverResults WHERE [DriverResultID] = @ResultID";
            comm.Parameters.AddWithValue("ResultID", resultID);
            rowExists = (comm.ExecuteScalar() != null);
            return rowExists;
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
