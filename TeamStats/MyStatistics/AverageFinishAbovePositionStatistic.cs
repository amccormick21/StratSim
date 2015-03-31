using DataSources.DataConnections;
using MyFlowLayout;
using StratSim.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStats.Functions;
using TeamStats.MyFlowLayout;
using TeamStats.Statistics;

namespace TeamStats.MyStatistics
{
    public class AverageFinishAbovePositionStatistic : SingleSessionSingleCompetitorStatistic, IPositionFilterStatistic
    {
        //TODO: override this class with average point score above a position
        public AverageFinishAbovePositionStatistic()
            :base()
        {
            SetFinishStateLimit(FinishingState.DSQ);
            SetPositionLimit(22);
        }

        public override void SetupStatistics()
        {
            base.SetupStatistics();
            DriverStats = new AverageFinishAbovePositionDataElement[Data.NumberOfDrivers];
            TeamStats = new AverageFinishAbovePositionDataElement[Data.NumberOfDrivers / 2];
            ComparisonStats = new AverageFinishAbovePositionDataElement[Data.NumberOfDrivers];
        }
        public override void InitialiseStatistics()
        {
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                DriverStats[driverIndex] = new AverageFinishAbovePositionDataElement(driverIndex);
                ComparisonStats[driverIndex] = new AverageFinishAbovePositionDataElement(driverIndex);
            }
            for (int teamIndex = 0; teamIndex < Data.NumberOfDrivers / 2; teamIndex++)
            {
                TeamStats[teamIndex] = new AverageFinishAbovePositionDataElement(teamIndex);
            }
        }
        public override void ProcessResult(Result result, int driverIndex, int roundIndex)
        {
            base.ProcessResult(result, driverIndex, roundIndex);

            if (result.modified)
            {
                //Apply filters:
                if (result.position <= PositionLimit && (int)result.finishState <= (int)FinishStateLimit)
                {
                    ((AverageFinishAbovePositionDataElement)DriverStats[driverIndex]).TotalFinishesAbovePosition += result.position;
                    ((AverageFinishAbovePositionDataElement)DriverStats[driverIndex]).CountFinishesAbovePosition++;
                    ((AverageFinishAbovePositionDataElement)TeamStats[Driver.GetTeamIndex(driverIndex)]).TotalFinishesAbovePosition += result.position;
                    ((AverageFinishAbovePositionDataElement)TeamStats[Driver.GetTeamIndex(driverIndex)]).CountFinishesAbovePosition++;
                }
            }
        }

        public override void Sort(OrderType orderType, SortType sortType)
        {
            IStatisticElement[] tempStatistics = DriverStats;
            Sorts.QuickSort(ref tempStatistics, 0, DriverStats.Length - 1, orderType, sortType);
            DriverStats = (AverageFinishAbovePositionDataElement[])tempStatistics;

            tempStatistics = TeamStats;
            Sorts.QuickSort(ref tempStatistics, 0, TeamStats.Length - 1, orderType, sortType);
            TeamStats = (AverageFinishAbovePositionDataElement[])tempStatistics;

            ComparisonStats = CalculateComparisonStatistics();
        }

        public override SortType DefaultSortType
        {
            get { return SortType.Value; }
        }
        public override OrderType DefaultOrderType
        {
            get { return OrderType.Ascending; }
        }
        public override Competitor DefaultDisplayCompetitor
        {
            get { return Competitor.Driver; }
        }

        public override string GetStatisticName()
        {
            return "Finishes Above Position";
        }

        public override string GetStatValueText(int position, Competitor competitorType, DisplayType displayType)
        {
            if (displayType == DisplayType.Value)
            {
                float delta = 0F;
                switch (competitorType)
                {
                    case Competitor.Driver:
                        delta = ((AverageFinishAbovePositionDataElement)DriverStats[position]).AverageFinishAbovePosition;
                        break;
                    case Competitor.Team:
                        delta = ((AverageFinishAbovePositionDataElement)TeamStats[position]).AverageFinishAbovePosition;
                        break;
                    case Competitor.Comparison:
                        delta = ((AverageFinishAbovePositionDataElement)ComparisonStats[position]).AverageFinishAbovePosition;
                        break;
                }
                if (delta == 0) { return "N/A"; }
                else { return Convert.ToString(Math.Round(delta, 1)); }
            }
            else
            {
                throw new CannortDisplayByThisParameterException(displayType);
            }
        }

        int positionLimit;
        public int PositionLimit
        {
            get { return positionLimit; }
        }

        FinishingState finishStateLimit;
        public FinishingState FinishStateLimit
        {
            get { return finishStateLimit; }
        }

        public void SetPositionLimit(int positionLimit)
        {
            this.positionLimit = positionLimit;
        }

        public void SetFinishStateLimit(FinishingState finishStateLimit)
        {
            this.finishStateLimit = finishStateLimit;
        }

        public override string GetStatisticMetadata(OrderType orderType, SortType sortType)
        {
            string metadata = "Finish State Limit: " + Convert.ToString(this.FinishStateLimit) + '\n';
            metadata += "Position Limtit: " + Convert.ToString(this.PositionLimit) + '\n';
            metadata += base.GetStatisticMetadata(orderType, sortType);
            return metadata;
        }

        public override void DisplayStatistic(TeamStatsPanelControlEvents events, MainForm form)
        {
            CalculateStatistics();
            events.OnShowPositionFilterStatsPanel(form, this, Competitors.All());
        }
    }
}
