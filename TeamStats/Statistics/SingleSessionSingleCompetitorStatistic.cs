using DataSources.DataConnections;
using StratSim.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamStats.Statistics
{
    public class SingleSessionSingleCompetitorStatistic : ResultBasedStatistic
    {
        public SingleSessionSingleCompetitorStatistic()
            :base()
        {

        }

        public override void CalculateStatistics()
        {
            //Initialise teams
            for (int teamIndex = 0; teamIndex < Data.NumberOfDrivers / 2; teamIndex++)
            {
                TeamStats[teamIndex].Reset(teamIndex);
            }

            //Drivers:
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                //Initialise
                DriverStats[driverIndex].Reset(driverIndex);

                for (int roundIndex = 0; roundIndex < Data.NumberOfTracks; roundIndex++)
                {
                    if (Results[driverIndex, roundIndex].position != 0 && Results[driverIndex, roundIndex].finishState != FinishingState.DSQ)
                    {
                        ProcessResult(Results[driverIndex, roundIndex], driverIndex, roundIndex);
                    }
                }
            }

            OnCalculationsComplete();
        }

        public virtual void ProcessResult(Result result, int driverIndex, int roundIndex)
        {

        }

    }
}
