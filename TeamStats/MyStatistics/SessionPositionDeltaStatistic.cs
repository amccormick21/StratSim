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
    public class SessionPositionDeltaStatistic : DualSessionSingleCompetitorStatistic, IDualSessionStatistic
    {
        public SessionPositionDeltaStatistic()
            : base()
        {

        }

        public override void SetupStatistics()
        {
            base.SetupStatistics();
            DriverStats = new SessionPositionDeltaDataElement[Data.NumberOfDrivers];
            TeamStats = new SessionPositionDeltaDataElement[Data.NumberOfDrivers / 2];
            ComparisonStats = new SessionPositionDeltaDataElement[Data.NumberOfDrivers];
        }
        public override void InitialiseStatistics()
        {
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                DriverStats[driverIndex] = new SessionPositionDeltaDataElement(driverIndex);
                ComparisonStats[driverIndex] = new SessionPositionDeltaDataElement(driverIndex);
            }
            for (int teamIndex = 0; teamIndex < Data.NumberOfDrivers / 2; teamIndex++)
            {
                TeamStats[teamIndex] = new SessionPositionDeltaDataElement(teamIndex);
            }
        }

        protected override void ProcessResult(Result firstSessionResult, Result secondSessionResult, int driverIndex, int raceIndex)
        {
            base.ProcessResult(firstSessionResult, secondSessionResult, driverIndex, raceIndex);
            if (firstSessionResult.modified && secondSessionResult.modified)
            {
                ((SessionPositionDeltaDataElement)DriverStats[driverIndex]).TotalPositionDelta += (firstSessionResult.position - secondSessionResult.position);
                ((SessionPositionDeltaDataElement)DriverStats[driverIndex]).SessionsCompared++;
                ((SessionPositionDeltaDataElement)TeamStats[Driver.GetTeamIndex(driverIndex)]).TotalPositionDelta += (firstSessionResult.position - secondSessionResult.position);
                ((SessionPositionDeltaDataElement)TeamStats[Driver.GetTeamIndex(driverIndex)]).SessionsCompared++;
            }
        }

        public override void Sort(OrderType orderType, SortType sortType)
        {
            IStatisticElement[] tempStatistics = DriverStats;
            Sorts.QuickSort(ref tempStatistics, 0, DriverStats.Length - 1, orderType, sortType);
            DriverStats = (SessionPositionDeltaDataElement[])tempStatistics;

            tempStatistics = TeamStats;
            Sorts.QuickSort(ref tempStatistics, 0, TeamStats.Length - 1, orderType, sortType);
            TeamStats = (SessionPositionDeltaDataElement[])tempStatistics;

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
            return "Position Deltas";
        }

        public override string GetStatValueText(int position, Competitor competitorType, DisplayType displayType)
        {
            if (displayType == DisplayType.Value)
            {
                float delta;
                switch (competitorType)
                {
                    case Competitor.Driver:
                        delta = ((SessionPositionDeltaDataElement)DriverStats[position]).AveragePositionDelta;
                        return Convert.ToString(Math.Round(delta, 1));
                    case Competitor.Team:
                        delta = ((SessionPositionDeltaDataElement)TeamStats[position]).AveragePositionDelta;
                        return Convert.ToString(Math.Round(delta, 1));
                    case Competitor.Comparison:
                        delta = ((SessionPositionDeltaDataElement)ComparisonStats[position]).AveragePositionDelta;
                        return Convert.ToString(Math.Round(delta, 1));
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
