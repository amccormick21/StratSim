using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StratSim.Model;

namespace TeamStats.Statistics
{
    public class DataElement : IStatisticElement
    {
        int competitorIndex;

        public DataElement(int competitorIndex)
        {
            this.competitorIndex = competitorIndex;
        }

        public virtual void Reset(int index)
        {
            this.competitorIndex = index;
        }

        public int CompetitorIndex
        {
            get { return competitorIndex; }
            set { competitorIndex = value; }
        }
        public string CompetitorName
        { get { return Data.Drivers[competitorIndex].DriverName; } }

        /// <summary>
        /// <para>Compares another instance of data element with this element.</para>
        /// <para>Returns a larger number if this should be ordered first in an ascending list.</para>
        /// </summary>
        /// <param name="element">The statistic element to compare to this</param>
        /// <param name="sortType">Represents the way that the array is sorted</param>
        /// <param name="ascending">True if the list is ascending</param>
        /// <exception cref="CannotSortByThisParameterException">Throws this exception if the data element cannot be sorted by this sort type.</exception>
        public float CompareTo(IStatisticElement element, SortType sortType, OrderType orderType)
        {
            float returnValue = 0;
            switch (sortType)
            {
                case SortType.Index:
                    returnValue = (this.CompetitorIndex - element.CompetitorIndex);
                    break;
                case SortType.Percentage:
                    returnValue = CompareByPercentage(element);
                    break;
                case SortType.Value:
                    returnValue = CompareByValue(element);
                    break;
                default: throw new CannortSortByThisParameterException(sortType);
            }

            return (orderType == OrderType.Ascending ? 1 : -1) * returnValue;
        }

        public virtual float CompareByPercentage(IStatisticElement element)
        {
            throw new CannortSortByThisParameterException(SortType.Percentage);
        }

        public virtual float CompareByValue(IStatisticElement element)
        {
            throw new CannortSortByThisParameterException(SortType.Value);
        }

        float IStatisticElement.CompareTo(IStatisticElement element, SortType sortType, OrderType orderType)
        {
            return CompareTo(element, sortType, orderType);
        }
    }

}
