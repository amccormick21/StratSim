using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamStats.Statistics
{
    /// <summary>
    /// Defines the methods and fields required for a statistic element in a statistic class.
    /// </summary>
    public interface IStatisticElement
    {
        int CompetitorIndex { get; set; }
        float CompareTo(IStatisticElement element, SortType sortType, OrderType orderType);
        void Reset(int index);
    }
}
