using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamStats.Statistics
{
    class TeammateComparisonDataElement : DataElement, IStatisticElement
    {
        int finishesAheadOfTeammate;
        float percentageFinishesAhead;

        public TeammateComparisonDataElement(int competitorIndex)
            :base(competitorIndex)
        {
            finishesAheadOfTeammate = 0;
            percentageFinishesAhead = 0F;
        }

        public int FinishesAheadOfTeammate
        {
            get { return finishesAheadOfTeammate; }
            set { finishesAheadOfTeammate = value; }
        }

        public float PercentageFinishesAhead
        {
            get { return percentageFinishesAhead; }
            set { percentageFinishesAhead = value; }
        }

        public override void Reset(int index)
        {
            base.Reset(index);
            finishesAheadOfTeammate = 0;
            percentageFinishesAhead = 0F;
        }

        public override float CompareByValue(IStatisticElement element)
        {
            TeammateComparisonDataElement other = (TeammateComparisonDataElement)element;
            return (this.finishesAheadOfTeammate - other.finishesAheadOfTeammate);
        }

        public override float CompareByPercentage(IStatisticElement element)
        {
            TeammateComparisonDataElement other = (TeammateComparisonDataElement)element;
            return (this.PercentageFinishesAhead - other.PercentageFinishesAhead);
        }
    }
}
