using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataSources;
using StratSim.Model.Files;
using DataSources.DataConnections;

namespace StratSim.Model.CalculationControllers
{
    public static class CalculationController
    {
        /// <summary>
        /// Loads driver timing data from the saved files automtically.
        /// </summary>
        /// <param name="raceIndex">The index of the race data to load</param>
        public static void PopulateDriverDataFromFiles(int raceIndex)
        {
            Data.RaceIndex = raceIndex;
            DataController dataController = new DataController();
            Dictionary<Session, ISessionData> sessionData = new Dictionary<Session, ISessionData>();

            //Populate the dictionary with file data:
            foreach (Session session in (Session[])Enum.GetValues(typeof(Session)))
            {
                sessionData[session] = dataController.GetDataType(session.GetTimingDataType());
                try
                {
                    sessionData[session].RetrieveArchiveData(session, raceIndex);
                    dataController.sessionDataLoaded[session] = true;
                }
                catch { dataController.sessionDataLoaded[session] = false; }
            }


            //Convert the file data into a usable format
            List<Stint>[] driverWeekendStints;
            float[] topSpeeds = ((SpeedData)sessionData[Session.SpeedTrap]).TopSpeeds;
            int[] positionsByDriverIndex = ((GridData)sessionData[Session.Grid]).GetPositionsByDriverIndex();

            //Set the driver data based on this.
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; ++driverIndex)
            {
                driverWeekendStints = new List<Stint>[] {
                                        ((LapData)sessionData[Session.FP1]).DriverWeekendStints[driverIndex],
                                        ((LapData)sessionData[Session.FP2]).DriverWeekendStints[driverIndex],
                                        ((LapData)sessionData[Session.FP3]).DriverWeekendStints[driverIndex],
                                        ((LapData)sessionData[Session.Qualifying]).DriverWeekendStints[driverIndex],
                                        ((LapData)sessionData[Session.Race]).DriverWeekendStints[driverIndex],
                                        };

                Data.Drivers[driverIndex].SetPracticeTimes(driverWeekendStints, topSpeeds[driverIndex], positionsByDriverIndex[driverIndex]);
            }
        }

        public static void CalculatePaceParameters()
        {
            foreach (Driver driver in Data.Drivers)
            {
                driver.SetPaceParameters();
            }
            Driver.AverageBetweenTeammates();
        }

        /// <summary>
        /// Optimises the strategies of all drivers.
        /// Sets the driver's strategy to the optimised strategy.
        /// </summary>
        public static void OptimiseAllStrategies(int raceIndex)
        {
            foreach (Driver driver in Data.Drivers)
            {
                driver.SetSelectedStrategy(raceIndex);
            }
        }

        public static void SetRaceStrategies()
        {
            foreach (Driver driver in Data.Drivers)
            {
                driver.SetRaceStrategy();
            }
        }

    }
}
