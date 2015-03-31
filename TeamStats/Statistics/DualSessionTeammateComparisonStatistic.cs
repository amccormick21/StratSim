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

namespace TeamStats.Statistics
{
    public class DualSessionTeammateComparisonStatistic : SessionComparisonStatistic
    {
        public DualSessionTeammateComparisonStatistic()
            : base()
        {
        }

        public override void SetupStatistics()
        {
            DriverStats = new TeammateComparisonDataElement[Data.NumberOfDrivers];
            TeamStats = new TeammateComparisonDataElement[Data.NumberOfDrivers / 2];
            ComparisonStats = new TeammateComparisonDataElement[Data.NumberOfDrivers];
        }
        public override void InitialiseStatistics()
        {
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                DriverStats[driverIndex] = new TeammateComparisonDataElement(driverIndex);
                ComparisonStats[driverIndex] = new TeammateComparisonDataElement(driverIndex);
            }
            for (int teamIndex = 0; teamIndex < Data.NumberOfDrivers / 2; teamIndex++)
            {
                TeamStats[teamIndex] = new TeammateComparisonDataElement(teamIndex);
            }
        }


        public override void CalculateStatistics()
        {
            //Initialise teams
            for (int teamIndex = 0; teamIndex < Data.NumberOfDrivers / 2; teamIndex++)
            {
                TeamStats[teamIndex].Reset(teamIndex);
            }

            //Drivers:
            //Teammate is driverIndex + 1
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex += 2)
            {
                //Initialise
                DriverStats[driverIndex].Reset(driverIndex);
                DriverStats[driverIndex + 1].Reset(driverIndex + 1);

                for (int roundIndex = 0; roundIndex < Data.NumberOfTracks; roundIndex++)
                {
                    if (FirstSessionResults[driverIndex, roundIndex].position != 0 && FirstSessionResults[driverIndex + 1, roundIndex].position != 0
                        && SecondSessionResults[driverIndex, roundIndex].position != 0 && SecondSessionResults[driverIndex + 1, roundIndex].position != 0)
                    {
                        ProcessResult(FirstSessionResults[driverIndex, roundIndex], FirstSessionResults[driverIndex + 1, roundIndex], SecondSessionResults[driverIndex, roundIndex], SecondSessionResults[driverIndex + 1, roundIndex], driverIndex, roundIndex);
                    }
                }
            }

            OnCalculationsComplete();
        }

        public virtual void ProcessResult(Result FirstDriverFirstSessionResult, Result FirstDriverSecondSessionResult, Result SecondDriverFirstSessionResult, Result SecondDriverSecondSessionResult, int firstDriverIndex, int raceIndex)
        {

        }

        public override void DisplayStatistic(TeamStatsPanelControlEvents events, MainForm form)
        {
            CalculateStatistics();
            events.OnShowDualSessionStatsPanel(form, this, new Competitor[] { Competitor.Driver, Competitor.Comparison });
        }

        public override void Sort(OrderType orderType, SortType sortType)
        {
            IStatisticElement[] tempStatistics = DriverStats;
            Sorts.QuickSort(ref tempStatistics, 0, DriverStats.Length - 1, orderType, sortType);
            DriverStats = (TeammateComparisonDataElement[])tempStatistics;

            tempStatistics = TeamStats;
            Sorts.QuickSort(ref tempStatistics, 0, TeamStats.Length - 1, orderType, sortType);
            TeamStats = (TeammateComparisonDataElement[])tempStatistics;

            ComparisonStats = CalculateComparisonStatistics();
        }

        protected override void OnCalculationsComplete()
        {
            //Calculate the percentages.
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                //Catch a div by zero.
                if (((TeammateComparisonDataElement)TeamStats[Driver.GetTeamIndex(driverIndex)]).FinishesAheadOfTeammate != 0)
                {
                    ((TeammateComparisonDataElement)DriverStats[driverIndex]).PercentageFinishesAhead = (float)((TeammateComparisonDataElement)DriverStats[driverIndex]).FinishesAheadOfTeammate / (float)((TeammateComparisonDataElement)TeamStats[Driver.GetTeamIndex(driverIndex)]).FinishesAheadOfTeammate;
                }
                else
                {
                    ((TeammateComparisonDataElement)DriverStats[driverIndex]).PercentageFinishesAhead = 0F;
                }
            }
            base.OnCalculationsComplete();
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
            get { return Competitor.Comparison; }
        }

        public override string GetStatValueText(int position, Competitor competitorType, DisplayType displayType)
        {
            switch (displayType)
            {
                case DisplayType.Value:
                    {
                        switch (competitorType)
                        {
                            case Competitor.Driver:
                                return Convert.ToString(((TeammateComparisonDataElement)DriverStats[position]).FinishesAheadOfTeammate);
                            case Competitor.Team:
                                return Convert.ToString(((TeammateComparisonDataElement)TeamStats[position]).FinishesAheadOfTeammate);
                            case Competitor.Comparison:
                                return Convert.ToString(((TeammateComparisonDataElement)ComparisonStats[position]).FinishesAheadOfTeammate);
                        };
                        break;
                    }
                case DisplayType.Percentage:
                    {
                        switch (competitorType)
                        {
                            case Competitor.Driver:
                                return Convert.ToString(((TeammateComparisonDataElement)DriverStats[position]).PercentageFinishesAhead);
                            case Competitor.Team:
                                return Convert.ToString(((TeammateComparisonDataElement)TeamStats[position]).PercentageFinishesAhead);
                            case Competitor.Comparison:
                                return Convert.ToString(((TeammateComparisonDataElement)ComparisonStats[position]).PercentageFinishesAhead);
                        };
                        break;
                    }
            };
            return "";
        }
    }
}
