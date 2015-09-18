using DataSources;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;

namespace StratSim.Model
{
    /// <summary>
    /// Contains fields representing data about how the driver will perform in a race,
    /// and methods for calculating this data from raw lap times.
    /// </summary>
    public class Driver : DriverData
    {
        PracticeTimesCollection practiceTimes;
        PaceParameterCollection paceParameters;
        Strategy selectedStrategy;
        RaceStrategy raceStrategy;

        /// <summary>
        /// Creates a new instance of the Driver class
        /// </summary>
        /// <param name="DriverIndex">The index of the driver to be created</param>
        /// <param name="properties">A string array containing the driver properties, loaded
        /// from the driver text file.</param>
        public Driver(int DriverIndex, string DriverName, string DriverTeam, int DriverNumber, Color LineColour, string Abbreviation)
            : base(DriverIndex, DriverName, DriverTeam, DriverNumber, LineColour, Abbreviation)
        {

        }

        public Driver()
            : base()
        { }

        /// <summary>
        /// Loads all drivers from the database
        /// </summary>
        /// <param name="numberOfDriversInSeason">A variable to which the number of drivers found is assigned</param>
        /// <param name="currentYear">The year from which the list of drivers is required</param>
        /// <returns>The populated array of drivers</returns>
        public static Driver[] InitialiseDrivers(out int numberOfDriversInSeason, int currentYear)
        {
            Driver[] arrayOfDrivers;

            string name, teamName, abbreviation;
            int driverNumber;
            Color lineColour;

            //Query to get all data from database
            var sqlConnectionString = DataSources.Program.GetConnectionString();
            using (OleDbConnection myConn = new OleDbConnection(sqlConnectionString))
            {
                myConn.Open();
                string commandText = "SELECT DriverName, DriverNumber, Team, LineColour, Abbreviation " +
                    "FROM Drivers, DriverTeams, TeamColours WHERE DriverYearNumber = DriverNumber AND TeamColourID = TeamPositionIndex " +
                    " AND RaceYear = @RaceYear";
                var comm = myConn.CreateCommand();
                comm.CommandText = commandText;
                comm.Parameters.Add(new OleDbParameter("RaceYear", currentYear));
                comm.CommandType = System.Data.CommandType.Text;
                using (var reader = comm.ExecuteReader())
                {
                    var driverList = new List<Driver>();
                    int driverIndex = 0;
                    while (reader.Read())
                    {
                        name = Convert.ToString(reader[0]);
                        driverNumber = Convert.ToInt32(reader[1]);
                        teamName = Convert.ToString(reader[2]);
                        lineColour = Color.FromName(Convert.ToString(reader[3]));
                        abbreviation = Convert.ToString(reader[4]);
                        driverList.Add(new Driver(driverIndex, name, teamName, driverNumber, lineColour, abbreviation));
                        driverIndex++;
                    }
                    numberOfDriversInSeason = driverIndex;
                    arrayOfDrivers = driverList.ToArray();
                }
            }
            return arrayOfDrivers;
        }

        public void SetPracticeTimes(List<Stint>[] PracticeSessionStints, float TopSpeed, int GridPosition)
        {
            PracticeTimes = new PracticeTimesCollection(PracticeSessionStints, TopSpeed, GridPosition);
        }
        public void SetPaceParameters()
        {
            PaceParameters = new PaceParameterCollection(this.PracticeTimes);
        }
        public void SetPaceParameters(PracticeTimesCollection PracticeTimes)
        {
            this.PracticeTimes = PracticeTimes;
            PaceParameters = new PaceParameterCollection(PracticeTimes);
        }
        public void SetSelectedStrategy(int raceIndex)
        {
            SelectedStrategy = OptimiseStrategy(this.DriverIndex, raceIndex);
        }
        public void SetSelectedStrategy(Strategy Strategy)
        {
            SelectedStrategy = Strategy;
        }
        public void SetRaceStrategy()
        {
            RaceStrategy = new RaceStrategy(SelectedStrategy, PracticeTimes.GridPosition);
        }
        public void SetRaceStrategy(Strategy Strategy)
        {
            this.SelectedStrategy = Strategy;
            RaceStrategy = new RaceStrategy(Strategy, PracticeTimes.GridPosition);
        }

        /// <summary>
        /// Averages all relevant driver parameters across their teams to improve
        /// data accuracy
        /// </summary>
        public static void AverageBetweenTeammates()
        {
            float teamFuelEffect, teamPrimeDeg, teamOptionDeg;
            int nonDefaultFuel, nonDefaultPrime, nonDefaultOption;

            //Cycles through every other driver. driverIndex + 1 denotes the teammate.
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex += 2)
            {
                teamFuelEffect = 0;
                teamPrimeDeg = 0;
                teamOptionDeg = 0;
                nonDefaultFuel = 0;
                nonDefaultPrime = 0;
                nonDefaultOption = 0;

                //If the driver's fuel effect is not the default value, the cumulative total is increased and the number of valid data points is incremented.
                if (Data.Drivers[driverIndex].PaceParameters.PaceParameters[PaceParameterType.FuelEffect] != Data.Settings.DefaultFuelEffect) { teamFuelEffect += Data.Drivers[driverIndex].PaceParameters.PaceParameters[PaceParameterType.FuelEffect]; nonDefaultFuel++; }
                if (Data.Drivers[driverIndex + 1].PaceParameters.PaceParameters[PaceParameterType.FuelEffect] != Data.Settings.DefaultFuelEffect) { teamFuelEffect += Data.Drivers[driverIndex + 1].PaceParameters.PaceParameters[PaceParameterType.FuelEffect]; nonDefaultFuel++; }

                if (nonDefaultFuel != 0) //Prevents dividing by zero
                {
                    teamFuelEffect /= nonDefaultFuel; //calculates the average fuel effect
                    Data.Drivers[driverIndex].PaceParameters.PaceParameters[PaceParameterType.FuelEffect] = teamFuelEffect; //sets both drivers' effects to this value
                    Data.Drivers[driverIndex + 1].PaceParameters.PaceParameters[PaceParameterType.FuelEffect] = teamFuelEffect;
                }

                //As above for tyre type.
                if (Data.Drivers[driverIndex].PaceParameters.PaceParameters[PaceParameterType.PrimeDegradation] != Data.Settings.DefaultPrimeDegradation)
                {
                    teamPrimeDeg += Data.Drivers[driverIndex].PaceParameters.PaceParameters[PaceParameterType.PrimeDegradation];
                    nonDefaultPrime++;
                }
                if (Data.Drivers[driverIndex + 1].PaceParameters.PaceParameters[PaceParameterType.PrimeDegradation] != Data.Settings.DefaultPrimeDegradation)
                {
                    teamPrimeDeg += Data.Drivers[driverIndex + 1].PaceParameters.PaceParameters[PaceParameterType.PrimeDegradation];
                    nonDefaultPrime++;
                }
                if (nonDefaultPrime != 0)
                {
                    teamPrimeDeg /= nonDefaultPrime;
                    Data.Drivers[driverIndex].PaceParameters.PaceParameters[PaceParameterType.PrimeDegradation] = teamPrimeDeg;
                    Data.Drivers[driverIndex + 1].PaceParameters.PaceParameters[PaceParameterType.PrimeDegradation] = teamPrimeDeg;
                }

                if (Data.Drivers[driverIndex].PaceParameters.PaceParameters[PaceParameterType.OptionDegradation] != Data.Settings.DefaultOptionDegradation)
                {
                    teamOptionDeg += Data.Drivers[driverIndex].PaceParameters.PaceParameters[PaceParameterType.OptionDegradation];
                    nonDefaultOption++;
                }
                if (Data.Drivers[driverIndex + 1].PaceParameters.PaceParameters[PaceParameterType.OptionDegradation] != Data.Settings.DefaultOptionDegradation)
                {
                    teamOptionDeg += Data.Drivers[driverIndex + 1].PaceParameters.PaceParameters[PaceParameterType.OptionDegradation];
                    nonDefaultOption++;
                }
                if (nonDefaultOption != 0)
                {
                    teamOptionDeg /= nonDefaultOption;
                    Data.Drivers[driverIndex].PaceParameters.PaceParameters[PaceParameterType.OptionDegradation] = teamOptionDeg;
                    Data.Drivers[driverIndex + 1].PaceParameters.PaceParameters[PaceParameterType.OptionDegradation] = teamOptionDeg;
                }
            }
        }

        /// <summary>
        /// Calculates the optimum strategy for the to use to complete the race
        /// </summary>
        /// <param name="driverIndex">The index of the driver being calculated</param>
        /// <param name="trackIndex">The index of the track that the race will be run on</param>
        /// <returns>The optimised strategy with lap times and pit stops calculated</returns>
        public Strategy OptimiseStrategy(int driverIndex, int trackIndex)
        {
            float currentStrategyTime = 0;
            float bestStrategyTime = 0;
            int strategyIteration = 0;
            Strategy bestStrategy = null;
            Strategy currentStrategy;

            //find shortest time throughout this loop
            for (int stints = 2; stints <= 4; stints++)
            {
                for (int primeStints = 1; primeStints < stints; primeStints++)
                {
                    //get the currently defined strategy
                    currentStrategy = new Strategy(stints, primeStints, this.PaceParameters, driverIndex);
                    currentStrategyTime = currentStrategy.TotalTime;

                    //set the initial value for best time
                    if (strategyIteration == 0)
                    {
                        bestStrategyTime = currentStrategyTime;
                        bestStrategy = currentStrategy;
                    }
                    //uses a most wanted holder to hold the best strategy calculated so far.
                    if (currentStrategyTime < bestStrategyTime)
                    {
                        bestStrategyTime = currentStrategyTime;
                        bestStrategy = currentStrategy;
                    }

                    strategyIteration++;
                }
            }

            return bestStrategy;
        }

        /// <summary>
        /// Clears data from the sessions completed by the driver.
        /// Used when changing the race to be calculated.
        /// </summary>
        public void ClearSessions()
        {
            foreach (var list in PracticeTimes.PracticeSessionStints)
            {
                list.Clear();
            }
        }

        /// <summary>
        /// Gets or sets the driver's currently stored strategy
        /// </summary>
        public Strategy SelectedStrategy
        {
            get { return selectedStrategy; }
            set { selectedStrategy = value; }
        }
        public RaceStrategy RaceStrategy
        {
            get { return raceStrategy; }
            set { raceStrategy = value; }
        }

        public PracticeTimesCollection PracticeTimes
        {
            get { return practiceTimes; }
            set { practiceTimes = value; }
        }

        public PaceParameterCollection PaceParameters
        {
            get { return paceParameters; }
            set { paceParameters = value; }
        }

        public static int GetTeamIndex(int driverIndex)
        {
            return GetTeamIndex(driverIndex, Data.Drivers, Data.Teams);
        }

        public static int GetTeamIndex(int driverIndex, Driver[] drivers, Team[] teams)
        {
            string teamName = drivers[driverIndex].Team;
            int teamIndexFound = -1;
            int teamIndex = 0;
            do
            {
                if (teams[teamIndex].TeamName.Equals(teamName))
                {
                    teamIndexFound = teamIndex;
                }
                teamIndex++;
            } while (teamIndex <= teams.Length && teamIndexFound == -1);
            return teamIndexFound;
        }

        public static Dictionary<int, int> GetDriverIndexDictionary()
        {
            var driverIndices = new Dictionary<int, int>();

            foreach (var driver in Data.Drivers)
            {
                driverIndices[driver.DriverNumber] = driver.DriverIndex;
            }

            return driverIndices;
        }

        public static int[] GetDriverNumberArray()
        {
            var driverNumbers = new int[Data.Drivers.Length];

            int driverIndex = 0;
            foreach (var driver in Data.Drivers)
            {
                driverNumbers[driverIndex++] = driver.DriverNumber;
            }

            return driverNumbers;
        }
    }
}
