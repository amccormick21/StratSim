using DataSources.DataConnections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStats.Statistics;

namespace TeamStats.MyStatistics
{
    class ProgressAgainstTeammateStatistic : DualSessionTeammateComparisonStatistic
    {
        public ProgressAgainstTeammateStatistic()
            : base()
        {
             
        }

        public override void ProcessResult(Result FirstDriverFirstSessionResult, Result FirstDriverSecondSessionResult, Result SecondDriverFirstSessionResult, Result SecondDriverSecondSessionResult, int firstDriverIndex, int raceIndex)
        {
            base.ProcessResult(FirstDriverFirstSessionResult, FirstDriverSecondSessionResult, SecondDriverFirstSessionResult, SecondDriverSecondSessionResult, firstDriverIndex, raceIndex);

            int firstDriverPositionDelta = FirstDriverSecondSessionResult.position - FirstDriverFirstSessionResult.position;
            int secondDriverPositionDelta = SecondDriverSecondSessionResult.position - SecondDriverFirstSessionResult.position;

            if (secondDriverPositionDelta > firstDriverPositionDelta)
                ((TeammateComparisonDataElement)DriverStats[firstDriverIndex]).FinishesAheadOfTeammate++;
            else if (secondDriverPositionDelta < firstDriverPositionDelta)
                ((TeammateComparisonDataElement)DriverStats[firstDriverIndex + 1]).FinishesAheadOfTeammate++;
            ((TeammateComparisonDataElement)TeamStats[firstDriverIndex / 2]).FinishesAheadOfTeammate++;
        }

        public override string GetStatisticName()
        {
            return "Progress Versus Teammate";
        }
    }
}
