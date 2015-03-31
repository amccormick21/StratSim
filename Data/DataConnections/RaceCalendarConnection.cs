using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSources.DataConnections
{
    public class RaceCalendarConnection
    {
        public static int GetRaceCalendarID(int round, int year)
        {
            int raceCalendarID;

            var sqlConnectionString = Program.GetConnectionString();
            using (OleDbConnection myConn = new OleDbConnection(sqlConnectionString))
            {
                myConn.Open();
                string commandText = "SELECT RaceCalendarID " +
                    "FROM RaceCalendar " +
                    "WHERE TrackYear = @RaceYear " +
                    "AND Round = @Round";
                var comm = myConn.CreateCommand();
                comm.CommandText = commandText;
                comm.CommandType = System.Data.CommandType.Text;
                comm.Parameters.AddWithValue("RaceYear", year);
                comm.Parameters.AddWithValue("Round", round);

                raceCalendarID = Convert.ToInt32(comm.ExecuteScalar());
            }
            return raceCalendarID;
        }

        public static int GetRoundNumber(int raceCalendarId)
        {
            int roundNumber;

            var sqlConnectionString = Program.GetConnectionString();
            using (OleDbConnection myConn = new OleDbConnection(sqlConnectionString))
            {
                myConn.Open();
                string commandText = "SELECT Round " +
                    "FROM RaceCalendar " +
                    "WHERE RaceCalendarID = @ID";
                var comm = myConn.CreateCommand();
                comm.CommandText = commandText;
                comm.CommandType = System.Data.CommandType.Text;
                comm.Parameters.AddWithValue("ID", raceCalendarId);

                roundNumber = Convert.ToInt32(comm.ExecuteScalar());
            }
            return roundNumber;
        }

        public static int GetRaceYear(int raceCalendarId)
        {
            int raceYear;

            var sqlConnectionString = Program.GetConnectionString();
            using (OleDbConnection myConn = new OleDbConnection(sqlConnectionString))
            {
                myConn.Open();
                string commandText = "SELECT TrackYear " +
                    "FROM RaceCalendar " +
                    "WHERE RaceCalendarID = @ID";
                var comm = myConn.CreateCommand();
                comm.CommandText = commandText;
                comm.CommandType = System.Data.CommandType.Text;
                comm.Parameters.AddWithValue("ID", raceCalendarId);

                raceYear = Convert.ToInt32(comm.ExecuteScalar());
            }
            return raceYear;
        }
    }
}
