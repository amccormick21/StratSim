using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StratSim;
using DataSources;
using System.Data.OleDb;

namespace StratSim.Model
{
    public class Team
    {
        public string TeamName { get; set; }
        public string EngineSupplier { get; set; }

        public Team(string name, string supplier)
        {
            TeamName = name;
            EngineSupplier = supplier;
        }

        public static Team[] InitialiseTeams(int currentYear)
        {
            Team[] arrayOfTeams;
            List<Team> teamList = new List<Team>();

            string sqlConnectionString = DataSources.Program.GetConnectionString();
            using (OleDbConnection myConn = new OleDbConnection(sqlConnectionString))
            {
                myConn.Open();
                string commandText = "SELECT TeamName, EngineSupplier " +
                    "FROM Teams, TeamYears " +
                    "WHERE TeamIndex = TeamID " +
                    "AND TeamYear = @RaceYear";
                var comm = myConn.CreateCommand();
                comm.CommandType = System.Data.CommandType.Text;
                comm.CommandText = commandText;
                comm.Parameters.AddWithValue("RaceYear", currentYear);
                using (var reader = comm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        teamList.Add(new Team(Convert.ToString(reader[0]), Convert.ToString(reader[1])));
                    }
                }

                myConn.Close();
            }

            arrayOfTeams = teamList.ToArray();
            return arrayOfTeams;
        }

    }
}
