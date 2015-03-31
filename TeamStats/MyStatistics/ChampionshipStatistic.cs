using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStats.Statistics;
using StratSim.Model;
using TeamStats.Functions;
using DataSources.DataConnections;

namespace TeamStats.MyStatistics
{
    public class ChampionshipStatistic : FinishingPositionChampionship, ISingleSessionStatistic
    {
        PointScoringSystem pointSystem;
        bool doublePoints;
        public PointScoringSystem PointSystem
        {
            get { return pointSystem; }
            set { pointSystem = value; }
        }
        public bool DoublePoints
        {
            get { return doublePoints; }
            set { doublePoints = value; }
        }

        public ChampionshipStatistic()
            : base()
        {
            PointSystem = PointScoringSystem.Post2009;
            DoublePoints = true;
        }

        public override void SetupStatistics()
        {
            DriverStats = new ChampionshipDataElement[Data.NumberOfDrivers];
            TeamStats = new ChampionshipDataElement[Data.NumberOfDrivers / 2];
            ComparisonStats = new ChampionshipDataElement[Data.NumberOfDrivers];
        }
        public override void InitialiseStatistics()
        {
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                DriverStats[driverIndex] = new ChampionshipDataElement(driverIndex);
                ComparisonStats[driverIndex] = new ChampionshipDataElement(driverIndex);
            }
            for (int teamIndex = 0; teamIndex < Data.NumberOfDrivers / 2; teamIndex++)
            {
                TeamStats[teamIndex] = new ChampionshipDataElement(teamIndex);
            }
        }

        public override void ProcessResult(Result result, int driverIndex, int roundIndex)
        {
            base.ProcessResult(result, driverIndex, roundIndex);

            int points = this.PointSystem.GetPoints(result.position, (DoublePoints && roundIndex == Data.NumberOfTracks - 1));

            ((ChampionshipDataElement)DriverStats[driverIndex]).Points += points;
            ((ChampionshipDataElement)TeamStats[Driver.GetTeamIndex(driverIndex)]).Points += points;
        }


        protected override void OnCalculationsComplete()
        {
            //Calculate the percentages.
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                //Catch a div by zero.
                if (((ChampionshipDataElement)TeamStats[Driver.GetTeamIndex(driverIndex)]).Points != 0)
                {
                    ((ChampionshipDataElement)DriverStats[driverIndex]).Percentage = (float)((ChampionshipDataElement)DriverStats[driverIndex]).Points / (float)((ChampionshipDataElement)TeamStats[Driver.GetTeamIndex(driverIndex)]).Points;
                }
                else
                {
                    ((ChampionshipDataElement)DriverStats[driverIndex]).Percentage = 0F;
                }
            }
            base.OnCalculationsComplete();
        }

        public override void Sort(OrderType orderType, SortType sortType)
        {
            IStatisticElement[] tempStatistics = DriverStats;
            Sorts.QuickSort(ref tempStatistics, 0, DriverStats.Length - 1, orderType, sortType);
            DriverStats = (ChampionshipDataElement[])tempStatistics;

            tempStatistics = TeamStats;
            Sorts.QuickSort(ref tempStatistics, 0, TeamStats.Length - 1, orderType, sortType);
            TeamStats = (ChampionshipDataElement[])tempStatistics;

            ComparisonStats = CalculateComparisonStatistics();
        }

        public override string GetStatisticName()
        {
            return "Championships";
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
                                return Convert.ToString(((ChampionshipDataElement)DriverStats[position]).Points);
                            case Competitor.Team:
                                return Convert.ToString(((ChampionshipDataElement)TeamStats[position]).Points);
                            case Competitor.Comparison:
                                return Convert.ToString(((ChampionshipDataElement)ComparisonStats[position]).Points);
                        };
                        break;
                    }
                case DisplayType.Percentage:
                    {
                        switch (competitorType)
                        {
                            case Competitor.Driver:
                                return Convert.ToString(((ChampionshipDataElement)DriverStats[position]).Percentage);
                            case Competitor.Team:
                                return Convert.ToString(((ChampionshipDataElement)TeamStats[position]).Points);
                            case Competitor.Comparison:
                                return Convert.ToString(((ChampionshipDataElement)ComparisonStats[position]).Percentage);
                        };
                        break;
                    }
            };
            return "";
        }
    }
}
