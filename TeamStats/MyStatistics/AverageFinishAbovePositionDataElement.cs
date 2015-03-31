using StratSim.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStats.Statistics;

namespace TeamStats.MyStatistics
{
    public class AverageFinishAbovePositionDataElement : DataElement, IStatisticElement
    {
        private int totalFinishesAbovePosition;
        private int countFinishesAbovePosition;

        public AverageFinishAbovePositionDataElement(int competitorIndex)
            :base(competitorIndex)
        {
            totalFinishesAbovePosition = 0;
            countFinishesAbovePosition = 0;
        }

        public int TotalFinishesAbovePosition
        {
            get { return totalFinishesAbovePosition; }
            set { totalFinishesAbovePosition = value; }
        }
        public int CountFinishesAbovePosition
        {
            get { return countFinishesAbovePosition; }
            set { countFinishesAbovePosition = value; }
        }

        public float AverageFinishAbovePosition
        {
            get
            {
                if (countFinishesAbovePosition == 0)
                    return 0F;
                else
                    return ((float)TotalFinishesAbovePosition / (float)CountFinishesAbovePosition);
            }
        }

        public override void Reset(int index)
        {
            base.Reset(index);
            totalFinishesAbovePosition = 0;
            countFinishesAbovePosition = 0;
        }

        public override float CompareByValue(IStatisticElement element)
        {
            AverageFinishAbovePositionDataElement other = (AverageFinishAbovePositionDataElement)element;
            if (this.AverageFinishAbovePosition == other.AverageFinishAbovePosition) { return 0F; }
            else if (this.AverageFinishAbovePosition == 0) { return Data.NumberOfDrivers; }
            else if (other.AverageFinishAbovePosition == 0) { return -Data.NumberOfDrivers; }
            
            return this.AverageFinishAbovePosition - other.AverageFinishAbovePosition;
        }
    }
}
