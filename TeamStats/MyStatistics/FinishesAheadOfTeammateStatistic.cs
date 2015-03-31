using DataSources.DataConnections;
using StratSim.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStats.Functions;
using TeamStats.Statistics;

namespace TeamStats.MyStatistics
{
    class FinishesAheadOfTeammateStatistic : SingleSessionTeammateComparisonStatistic
    {
        public FinishesAheadOfTeammateStatistic()
            :base()
        {

        }

        public override void ProcessResult(Result firstDriverResult, Result secondDriverResult, int firstDriverIndex, int raceIndex)
        {
            base.ProcessResult(firstDriverResult, secondDriverResult, firstDriverIndex, raceIndex);

            if (firstDriverResult.position < secondDriverResult.position)
                ((TeammateComparisonDataElement)DriverStats[firstDriverIndex]).FinishesAheadOfTeammate++;
            else if (firstDriverResult.position > secondDriverResult.position)
                ((TeammateComparisonDataElement)DriverStats[firstDriverIndex + 1]).FinishesAheadOfTeammate++;
            ((TeammateComparisonDataElement)TeamStats[firstDriverIndex / 2]).FinishesAheadOfTeammate++;
        }

        public override string GetStatisticName()
        {
            return "Finishes Ahead Of Teammate";
        }
    }
}
