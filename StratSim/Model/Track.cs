using DataSources.StratSimDataSetTableAdapters;
using DataSources;
using System.Data.OleDb;
using System.Collections.Generic;
using System;

namespace StratSim.Model
{
    public class Track
    {
        //properties
        public int roundNumber;
        public string name;
        public int laps;
        public float fuelPerLap;
        public float pitStopLoss;
        public string abbreviation;

        /// <summary>
        /// Creates a new instance of the track class
        /// </summary>
        /// <param name="Name">The name of the track (normally the country)</param>
        /// <param name="Laps">The laps in the race</param>
        /// <param name="fuelConsumption">The fuel use in kg/lap for the track</param>
        /// <param name="_pitStopLoss">The time loss due to a pit stop</param>
        /// <param name="index">The zero-based position in the year of the race</param>
        public Track(string Name, int Laps, float fuelConsumption, float _pitStopLoss, int index, string Abbreviation)
        {
            name = Name;
            laps = Laps;
            fuelPerLap = fuelConsumption;
            pitStopLoss = _pitStopLoss;
            roundNumber = index + 1;
            abbreviation = Abbreviation;
        }

        /// <summary>
        /// Loads all tracks from the database
        /// </summary>
        /// <param name="numberOfTracksInSeason">A variable to which the number of tracks found is assigned</param>
        /// <returns>The populated array of tracks</returns>
        public static Track[] InitialiseTracks(out int numberOfTracksInSeason, int currentYear)
        {
            Track[] arrayOfTracks;
            string name, abbreviation;
            int laps;
            float pitStopLoss, fuelConsumption;

            var sqlConnectionString = DataSources.Program.GetConnectionString();
            using (OleDbConnection myConn = new OleDbConnection(sqlConnectionString))
            {
                myConn.Open();
                var commandText = "SELECT * " +
                    "FROM Tracks, RaceCalendar " +
                    "WHERE TrackYear = @TrackYear " +
                    "AND TrackID = TrackIndex " +
                    "ORDER BY Round";
                var comm = myConn.CreateCommand();
                comm.CommandText = commandText;
                comm.Parameters.AddWithValue("TrackYear", currentYear);
                using (var reader = comm.ExecuteReader())
                {
                    var trackList = new List<Track>();
                    int trackIndex = 0;
                    while (reader.Read())
                    {
                        name = Convert.ToString(reader[1]);
                        laps = Convert.ToInt32(reader[2]);
                        pitStopLoss = float.Parse(Convert.ToString(reader[3]));
                        fuelConsumption = float.Parse(Convert.ToString(reader[4]));
                        abbreviation = Convert.ToString(reader[5]);
                        trackList.Add(new Track(name, laps, fuelConsumption, pitStopLoss, trackIndex, abbreviation));
                        trackIndex++;
                    }
                    numberOfTracksInSeason = trackIndex;
                    arrayOfTracks = trackList.ToArray();
                }
            }
            return arrayOfTracks;
        }
    }
}
