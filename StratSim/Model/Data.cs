using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StratSim;
using DataSources.DataConnections;

namespace StratSim.Model
{
    /// <summary>
    /// Enumerated options for the type of tyre used by a driver
    /// </summary>
    public enum TyreType { Prime, Option }

    /// <summary>
    /// class containing static data elements used within the program
    /// </summary>
    public class Data
    {
        /// <summary>
        /// Initialises the data class and initialises elements within it.
        /// </summary>
        public Data()
        {
            settings = new Settings();
            drivers = Driver.InitialiseDrivers(out NumberOfDrivers, Properties.Settings.Default.CurrentYear);
            tracks = Track.InitialiseTracks(out NumberOfTracks, Properties.Settings.Default.CurrentYear);
            teams = Team.InitialiseTeams(Properties.Settings.Default.CurrentYear);
        }

        /// <summary>
        /// The number of drivers registered to take part in a race
        /// </summary>
        public static int NumberOfDrivers;
        /// <summary>
        /// The number of races in a season
        /// </summary>
        public static int NumberOfTracks;

        static Driver[] drivers;
        static Team[] teams;
        static Track[] tracks;
        static Settings settings;
        static Race race;

        static int raceIndex = 0;
        static int driverIndex = 0;

        /// <returns>The number of laps run in the race denoted by the current race index</returns>
        public static int GetRaceLaps()
        {
            return tracks[raceIndex].laps;
        }

        /// <summary>
        /// Gets or sets an array of the drivers currently taking part in an event.
        /// </summary>
        public static Driver[] Drivers
        {
            get { return drivers; }
            set { drivers = value; }
        }
        /// <summary>
        /// Gets or sets an array of the teams taking part in the event
        /// </summary>
        public static Team[] Teams
        {
            get { return teams; }
            set { teams = value; }
        }
        /// <summary>
        /// Gets or sets an array of the race tracks currently loaded
        /// </summary>
        public static Track[] Tracks
        {
            get { return tracks; }
            set { tracks = value; }
        }
        /// <summary>
        /// Gets or sets a collection of data used for generic settings and default information.
        /// </summary>
        public static Settings Settings
        {
            get { return settings; }
            set { settings = value; }
        }
        /// <summary>
        /// Gets or sets the race simulation used in the program.
        /// </summary>
        public static Race Race
        {
            get { return race; }
            set { race = value; }
        }

        /// <summary>
        /// Gets or sets the current race index. Setting the race index shows track information
        /// in the info panel.
        /// </summary>
        public static int RaceIndex
        {
            get { return raceIndex; }
            set
            {
                raceIndex = value;
                Program.InfoPanel.ShowTrackInfo = true;
            }
        }
        /// <summary>
        /// Gets the track represented by the current race index.
        /// </summary>
        public static Track CurrentTrack
        { get { return tracks[raceIndex]; } }
        /// <summary>
        /// Gets or sets the current driver index. This is the normalised driver on any graph
        /// and the driver whose details are displayed in the info panel.
        /// </summary>
        public static int DriverIndex
        {
            get { return driverIndex; }
            set
            {
                driverIndex = value;
                Program.InfoPanel.ShowDriverInfo = true;
            }
        }
        /// <summary>
        /// Gets the driver represented by the current driver index.
        /// </summary>
        public static Driver CurrentDriver
        { get { return drivers[driverIndex]; } }
    }
}
