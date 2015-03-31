using DataSources.DataConnections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamStats.Statistics
{
    public interface IPositionFilterStatistic : ISingleSessionStatistic
    {
        int PositionLimit { get; }
        FinishingState FinishStateLimit { get; }

        void SetPositionLimit(int positionLimit);
        void SetFinishStateLimit(FinishingState finishStateLimit);
    }
}
