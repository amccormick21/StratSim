using DataSources.DataConnections;
using MyFlowLayout;
using StratSim.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStats.Functions;
using TeamStats.Statistics;

namespace TeamStats.MyStatistics
{
    public class FinishingPositionChampionship : SingleSessionSingleCompetitorStatistic, ISingleSessionStatistic
    {
        public FinishingPositionChampionship()
            :base()
        {

        }

        public override void SetupStatistics()
        {
            DriverStats = new FinishingPositionChampionshipElement[Data.NumberOfDrivers];
            TeamStats = new FinishingPositionChampionshipElement[Data.NumberOfDrivers / 2];
            ComparisonStats = new FinishingPositionChampionshipElement[Data.NumberOfDrivers];
        }
        public override void InitialiseStatistics()
        {
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                DriverStats[driverIndex] = new FinishingPositionChampionshipElement(driverIndex);
                ComparisonStats[driverIndex] = new FinishingPositionChampionshipElement(driverIndex);
            }
            for (int teamIndex = 0; teamIndex < Data.NumberOfDrivers / 2; teamIndex++)
            {
                TeamStats[teamIndex] = new FinishingPositionChampionshipElement(teamIndex);
            }
        }

        public override void ProcessResult(Result result, int driverIndex, int roundIndex)
        {
            base.ProcessResult(result, driverIndex, roundIndex);

            //Positions are indexed at 0:
            ((FinishingPositionChampionshipElement)DriverStats[driverIndex]).NumberOfResults[result.position - 1]++;
            ((FinishingPositionChampionshipElement)TeamStats[Driver.GetTeamIndex(driverIndex)]).NumberOfResults[result.position - 1]++;
        }

        public override void Sort(OrderType orderType, SortType sortType)
        {
            IStatisticElement[] tempStatistics = DriverStats;
            Sorts.QuickSort(ref tempStatistics, 0, DriverStats.Length - 1, orderType, sortType);
            DriverStats = (FinishingPositionChampionshipElement[])tempStatistics;

            tempStatistics = TeamStats;
            Sorts.QuickSort(ref tempStatistics, 0, TeamStats.Length - 1, orderType, sortType);
            TeamStats = (FinishingPositionChampionshipElement[])tempStatistics;

            ComparisonStats = CalculateComparisonStatistics();
        }

        public override SortType DefaultSortType
        {
            get { return SortType.Value; }
        }
        public override OrderType DefaultOrderType
        {
            get { return OrderType.Descending; }
        }
        public override Competitor DefaultDisplayCompetitor
        {
            get { return Competitor.Driver; }
        }

        public override string GetStatisticName()
        {
            return "Finishing Position Championships";
        }
        public override string GetStatValueText(int position, Competitor competitorType, DisplayType displayType)
        {
            if (displayType == DisplayType.Value)
            {
                switch (competitorType)
                {
                    case Competitor.Driver:
                        return Convert.ToString(((FinishingPositionChampionshipElement)DriverStats[position]).BestFinish());
                    case Competitor.Team:
                        return Convert.ToString(((FinishingPositionChampionshipElement)TeamStats[position]).BestFinish());
                    case Competitor.Comparison:
                        return Convert.ToString(((FinishingPositionChampionshipElement)ComparisonStats[position]).BestFinish());
                }
                return "";
            }
            else
            {
                throw new CannortDisplayByThisParameterException(displayType);
            }
        }
    }
}
