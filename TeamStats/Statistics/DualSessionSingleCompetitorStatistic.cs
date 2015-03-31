using DataSources.DataConnections;
using StratSim.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamStats.Statistics
{
    public class DualSessionSingleCompetitorStatistic : SessionComparisonStatistic
    {
        public DualSessionSingleCompetitorStatistic()
            :base()
        {

        }

        public override void CalculateStatistics()
        {
            for (int teamIndex = 0; teamIndex < Data.NumberOfDrivers / 2; teamIndex++)
            {
                TeamStats[teamIndex].Reset(teamIndex);
            }

            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                DriverStats[driverIndex].Reset(driverIndex);
                for (int raceIndex = 0; raceIndex < Data.NumberOfTracks; raceIndex++)
                {
                    ProcessResult(FirstSessionResults[driverIndex, raceIndex], SecondSessionResults[driverIndex, raceIndex], driverIndex, raceIndex);
                }
            }

            OnCalculationsComplete();
        }

        protected virtual void ProcessResult(Result firstSessionResult, Result secondSessionResult, int driverIndex, int raceIndex)
        {

        }
    }
}
