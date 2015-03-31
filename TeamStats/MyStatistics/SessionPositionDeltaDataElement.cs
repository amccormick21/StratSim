using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStats.Statistics;

namespace TeamStats.MyStatistics
{
    public class SessionPositionDeltaDataElement : DataElement, IStatisticElement
    {
        int totalPositionDelta;
        public int TotalPositionDelta
        {
            get { return totalPositionDelta; }
            set { totalPositionDelta = value; }
        }

        int sessionsCompared;
        public int SessionsCompared
        {
            get { return sessionsCompared; }
            set { sessionsCompared = value; }
        }

        public float AveragePositionDelta
        {
            get
            {
                if (SessionsCompared == 0) //Catch a div by zero exception.
                    return 0F;
                else
                    return (float)TotalPositionDelta / (float)SessionsCompared;
            }
        }

        public SessionPositionDeltaDataElement(int competitorIndex)
            : base(competitorIndex)
        {
            totalPositionDelta = 0;
            sessionsCompared = 0;
        }

        public override void Reset(int index)
        {
            base.Reset(index);
            totalPositionDelta = 0;
            sessionsCompared = 0;
        }

        public override float CompareByPercentage(IStatisticElement element)
        {
            return base.CompareByPercentage(element);
        }

        public override float CompareByValue(IStatisticElement element)
        {
            SessionPositionDeltaDataElement other = (SessionPositionDeltaDataElement)element;

            return (float)(this.AveragePositionDelta - other.AveragePositionDelta);
        }
    }
}
