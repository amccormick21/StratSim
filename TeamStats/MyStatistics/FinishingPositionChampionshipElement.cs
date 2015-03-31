using StratSim.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStats.Statistics;

namespace TeamStats.MyStatistics
{
    public class FinishingPositionChampionshipElement : DataElement, IStatisticElement
    {
        int[] numberOfResults;

        public FinishingPositionChampionshipElement(int competitorIndex)
            : base(competitorIndex)
        {
            numberOfResults = new int[Data.NumberOfDrivers];
        }

        public int[] NumberOfResults
        {
            get { return numberOfResults; }
            set { numberOfResults = value; }
        }

        public override void Reset(int index)
        {
            base.Reset(index);
            for (int position = 0; position < Data.NumberOfDrivers; position++)
            {
                NumberOfResults[position] = 0;
            }
        }

        public override float CompareByValue(IStatisticElement element)
        {
            FinishingPositionChampionshipElement other = (FinishingPositionChampionshipElement)element;
            for (int positionIndex = 0; positionIndex < Data.NumberOfDrivers; positionIndex++)
            {
                if (this.NumberOfResults[positionIndex] != other.NumberOfResults[positionIndex])
                {
                    return (this.NumberOfResults[positionIndex] - other.NumberOfResults[positionIndex]);
                }
            }
            return 0;
        }

        public int BestFinish()
        {
            for (int position = 0; position < Data.NumberOfDrivers; position++)
            {
                if (NumberOfResults[position] > 0)
                    return position + 1;
            }

            return 0;
        }
    }
}
