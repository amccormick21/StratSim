using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStats.Statistics;
using StratSim.Model;
using DataSources.DataConnections;
using TeamStats.Functions;

namespace TeamStats.MyStatistics
{
    public class SessionPointsDeltaStatistic : SessionPositionDeltaStatistic, IDualSessionStatistic
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

        public SessionPointsDeltaStatistic()
            : base()
        {
            PointSystem = PointScoringSystem.Post2009;
            DoublePoints = true;
        }

        public override void SetupStatistics()
        {
            base.SetupStatistics();
            DriverStats = new SessionPointsDeltaDataElement[Data.NumberOfDrivers];
            TeamStats = new SessionPointsDeltaDataElement[Data.NumberOfDrivers / 2];
            ComparisonStats = new SessionPointsDeltaDataElement[Data.NumberOfDrivers];
        }

        public override void InitialiseStatistics()
        {
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                DriverStats[driverIndex] = new SessionPointsDeltaDataElement(driverIndex);
                ComparisonStats[driverIndex] = new SessionPointsDeltaDataElement(driverIndex);
            }
            for (int teamIndex = 0; teamIndex < Data.NumberOfDrivers / 2; teamIndex++)
            {
                TeamStats[teamIndex] = new SessionPointsDeltaDataElement(teamIndex);
            }
        }

        protected override void ProcessResult(Result firstSessionResult, Result secondSessionResult, int driverIndex, int raceIndex)
        {
            base.ProcessResult(firstSessionResult, secondSessionResult, driverIndex, raceIndex);

            int firstSessionPoints = this.PointSystem.GetPoints(firstSessionResult.position, (DoublePoints && raceIndex == Data.NumberOfTracks - 1));
            int secondSessionPoints = this.PointSystem.GetPoints(secondSessionResult.position, (DoublePoints && raceIndex == Data.NumberOfTracks - 1));
            if (firstSessionResult.modified && secondSessionResult.modified)
            {
                ((SessionPointsDeltaDataElement)DriverStats[driverIndex]).SessionPointsDelta += (secondSessionPoints - firstSessionPoints);
                ((SessionPointsDeltaDataElement)TeamStats[Driver.GetTeamIndex(driverIndex)]).SessionPointsDelta += (secondSessionPoints - firstSessionPoints);
            }
        }

        protected override void OnCalculationsComplete()
        {
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                //Catch a div by zero
                if (((SessionPointsDeltaDataElement)TeamStats[Driver.GetTeamIndex(driverIndex)]).SessionPointsDelta != 0)
                {
                    ((SessionPointsDeltaDataElement)DriverStats[driverIndex]).PointsDeltaPercentage = (float)((SessionPointsDeltaDataElement)DriverStats[driverIndex]).SessionPointsDelta / (float)Math.Abs(((SessionPointsDeltaDataElement)TeamStats[Driver.GetTeamIndex(driverIndex)]).SessionPointsDelta);
                }
                else
                {
                    ((SessionPointsDeltaDataElement)DriverStats[driverIndex]).PointsDeltaPercentage = 0F;
                }
            }
            base.OnCalculationsComplete();
        }

        public override void Sort(OrderType orderType, SortType sortType)
        {
            IStatisticElement[] tempStatistics = DriverStats;
            Sorts.QuickSort(ref tempStatistics, 0, DriverStats.Length - 1, orderType, sortType);
            DriverStats = (SessionPointsDeltaDataElement[])tempStatistics;

            tempStatistics = TeamStats;
            Sorts.QuickSort(ref tempStatistics, 0, TeamStats.Length - 1, orderType, sortType);
            TeamStats = (SessionPointsDeltaDataElement[])tempStatistics;

            ComparisonStats = CalculateComparisonStatistics();
        }

        public override string GetStatisticName()
        {
            return "Session Points Delta";
        }
        public override string GetStatValueText(int position, Competitor competitorType, DisplayType displayType)
        {
            float delta;
            switch (displayType)
            {
                case DisplayType.Value:
                    {
                        switch (competitorType)
                        {
                            case Competitor.Driver:
                                delta = ((SessionPointsDeltaDataElement)DriverStats[position]).AveragePointsDelta;
                                return Convert.ToString(Math.Round(delta, 1));
                            case Competitor.Team:
                                delta = ((SessionPointsDeltaDataElement)TeamStats[position]).AveragePointsDelta;
                                return Convert.ToString(Math.Round(delta, 1));
                            case Competitor.Comparison:
                                delta = ((SessionPointsDeltaDataElement)ComparisonStats[position]).AveragePointsDelta;
                                return Convert.ToString(Math.Round(delta, 1));
                        };
                        break;
                    }
                case DisplayType.Percentage:
                    {
                        switch (competitorType)
                        {
                            case Competitor.Driver:
                                return Convert.ToString(((SessionPointsDeltaDataElement)DriverStats[position]).PointsDeltaPercentage);
                            case Competitor.Team:
                                delta = ((SessionPointsDeltaDataElement)TeamStats[position]).AveragePointsDelta;
                                return Convert.ToString(Math.Round(delta, 1));
                            case Competitor.Comparison:
                                return Convert.ToString(((SessionPointsDeltaDataElement)ComparisonStats[position]).PointsDeltaPercentage);
                        };
                        break;
                    }
            }
            return "";
        }

    }
}
