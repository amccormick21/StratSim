using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStats.Statistics;

namespace TeamStats.MyStatistics
{
    public class SessionPointsDeltaDataElement : SessionPositionDeltaDataElement, IStatisticElement
    {
        public int SessionPointsDelta { get; set; }
        public float PointsDeltaPercentage { get; set; }

        public float AveragePointsDelta
        {
            get
            {
                if (SessionsCompared == 0) //Catch a div by zero exception.
                    return 0;
                else
                    return (float)SessionPointsDelta / (float)SessionsCompared;
            }
        }

        public SessionPointsDeltaDataElement(int competitorIndex)
            :base(competitorIndex)
        {
            SessionPointsDelta = 0;
            PointsDeltaPercentage = 0;
        }

        public override void Reset(int index)
        {
            base.Reset(index);
            SessionPointsDelta = 0;
            PointsDeltaPercentage = 0;
        }

        public override float CompareByPercentage(IStatisticElement element)
        {
            SessionPointsDeltaDataElement other = (SessionPointsDeltaDataElement)element;

            if (this.PointsDeltaPercentage == other.PointsDeltaPercentage)
            {
                return CompareByValue(element);
            }
            else
            {
                return (this.PointsDeltaPercentage - other.PointsDeltaPercentage);
            }
        }

        public override float CompareByValue(IStatisticElement element)
        {
            SessionPointsDeltaDataElement other = (SessionPointsDeltaDataElement)element;

            return (float)(this.AveragePointsDelta - other.AveragePointsDelta);
        }

    }
}
