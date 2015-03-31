using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStats.Statistics;
using StratSim.Model;

namespace TeamStats.MyStatistics
{
    public class ChampionshipDataElement : FinishingPositionChampionshipElement, IStatisticElement
    {
        int points;
        float percentage;

        public ChampionshipDataElement(int competitorIndex)
            : base(competitorIndex)
        {
            points = 0;
            percentage = 0;
        }

        public int Points
        {
            get { return points; }
            set { points = value; }
        }
        public float Percentage
        {
            get { return percentage; }
            set { percentage = value; }
        }

        public override void Reset(int index)
        {
            base.Reset(index);
            Points = 0;
            Percentage = 0;
        }

        public override float CompareByValue(IStatisticElement element)
        {
            ChampionshipDataElement other = (ChampionshipDataElement)element;
            if (this.Points != other.Points)
            {
                return this.Points - other.Points;
            }
            else
            {
                return base.CompareByValue(element);
            }
        }

        public override float CompareByPercentage(IStatisticElement element)
        {
            ChampionshipDataElement other = (ChampionshipDataElement)element;
            if (this.Percentage != other.Percentage)
            {
                return this.Percentage - other.Percentage;
            }
            else
            {
                return CompareByValue(element);
            }
        }
    }
}
