using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamStats.Statistics
{
    public interface IStatistic
    {
        void Sort(OrderType orderType, SortType sortType);
        void CalculateStatistics();
        string GetCompetitorNumberText(int position, Competitor competitorType);
        string GetCompetitorNameText(int position, Competitor competitorType);
        string GetStatValueText(int position, Competitor competitorType, DisplayType displayType);
        string GetStatisticName();
        string GetStatisticMetadata(OrderType orderType, SortType sortType);
        SortType DefaultSortType { get; }
        OrderType DefaultOrderType { get; }
        DisplayType DefaultDisplayType { get; }
        Competitor DefaultDisplayCompetitor { get; }
        IStatisticElement[] DriverStats { get; set; }
        IStatisticElement[] TeamStats { get; set; }
    }
}
