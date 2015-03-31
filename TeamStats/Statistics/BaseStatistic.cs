using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StratSim.Model;
using TeamStats.Functions;
using System.Windows.Forms;
using MyFlowLayout;
using TeamStats.MyFlowLayout;

namespace TeamStats.Statistics
{
    public class BaseStatistic : IStatistic
    {
        IStatisticElement[] driverStats;
        IStatisticElement[] teamStats;
        IStatisticElement[] comparisonStats;

        public BaseStatistic()
        {
            SetupStatistics();
            InitialiseStatistics();
        }

        public virtual void SetupStatistics()
        {
            driverStats = new DataElement[Data.NumberOfDrivers];
            teamStats = new DataElement[Data.NumberOfDrivers / 2];
            comparisonStats = new DataElement[Data.NumberOfDrivers];
        }
        public virtual void InitialiseStatistics()
        {
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                DriverStats[driverIndex] = new DataElement(driverIndex);
                ComparisonStats[driverIndex] = new DataElement(driverIndex);
            }
            for (int teamIndex = 0; teamIndex < Data.NumberOfDrivers / 2; teamIndex++)
            {
                TeamStats[teamIndex] = new DataElement(teamIndex);
            }
        }
        public virtual void CalculateStatistics()
        {
            OnCalculationsComplete();
        }

        protected virtual void OnCalculationsComplete()
        {
        }

        public virtual void Sort(OrderType orderType, SortType sortType)
        {
            IStatisticElement[] tempStatistics = DriverStats;
            Sorts.QuickSort(ref tempStatistics, 0, DriverStats.Length - 1, orderType, sortType);
            DriverStats = (DataElement[])tempStatistics;

            tempStatistics = TeamStats;
            Sorts.QuickSort(ref tempStatistics, 0, TeamStats.Length - 1, orderType, sortType);
            TeamStats = (DataElement[])tempStatistics;

            ComparisonStats = CalculateComparisonStatistics();
        }

        public IStatisticElement[] CalculateComparisonStatistics()
        {
            IStatisticElement[] comparisonArray = new IStatisticElement[Data.NumberOfDrivers];

            //An array of the position in the comparison array at which the first found driver
            //in the team is located.
            //Indexed by team index.
            int[] teamIndices = new int[Data.NumberOfDrivers / 2];
            //Set all to -1 because no teams have been found yet.
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers / 2; driverIndex++)
            {
                teamIndices[driverIndex] = -1;
            }

            int indexToInsertNextStatistic = 0;
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                if (teamIndices[Driver.GetTeamIndex(DriverStats[driverIndex].CompetitorIndex)] == -1)
                {
                    //The team has not yet been seen so this is the highest position in the team.
                    //The element can be inserted at the next available position in the array.
                    comparisonArray[indexToInsertNextStatistic] = DriverStats[driverIndex];

                    //Update the team indices array to reflect where the first driver in the team was located.
                    teamIndices[Driver.GetTeamIndex(DriverStats[driverIndex].CompetitorIndex)] = indexToInsertNextStatistic;

                    //Update the next available position in the array.
                    indexToInsertNextStatistic += 2;
                }
                else
                {
                    //The team has already been seen so this driver goes next to his teammate.
                    comparisonArray[teamIndices[Driver.GetTeamIndex(DriverStats[driverIndex].CompetitorIndex)] + 1] = DriverStats[driverIndex];
                }
            }

            return comparisonArray;
        }

        public virtual string GetCompetitorNumberText(int position, Competitor competitorType)
        {
            switch (competitorType)
            {
                case Competitor.Driver:
                    return Convert.ToString(Data.Drivers[DriverStats[position].CompetitorIndex].DriverNumber);
                case Competitor.Team:
                    return Convert.ToString(Data.Teams[TeamStats[position].CompetitorIndex].TeamName);
                case Competitor.Comparison:
                    return Convert.ToString(Data.Drivers[ComparisonStats[position].CompetitorIndex].DriverNumber);
            }
            return "";
        }
        public virtual string GetCompetitorNameText(int position, Competitor competitorType)
        {
            switch (competitorType)
            {
                case Competitor.Driver:
                    return Convert.ToString(Data.Drivers[DriverStats[position].CompetitorIndex].DriverName);
                case Competitor.Team:
                    return Convert.ToString(Data.Teams[TeamStats[position].CompetitorIndex].TeamName);
                case Competitor.Comparison:
                    return Convert.ToString(Data.Drivers[ComparisonStats[position].CompetitorIndex].DriverName);
            }
            return "";
        }
        public virtual string GetStatValueText(int position, Competitor competitorType, DisplayType displayType)
        {
            if (displayType == DisplayType.Value)
            {
                switch (competitorType)
                {
                    case Competitor.Driver:
                        return Convert.ToString(DriverStats[position].CompetitorIndex);
                    case Competitor.Comparison:
                        return Convert.ToString(ComparisonStats[position].CompetitorIndex);
                    case Competitor.Team:
                        return Convert.ToString(TeamStats[position].CompetitorIndex);
                }
                return "";
            }
            else
            {
                throw new CannortDisplayByThisParameterException(displayType);
            }
        }

        public virtual string GetStatisticName()
        {
            return "Base Statistic";
        }
        public virtual string GetStatisticMetadata(OrderType orderType, SortType sortType)
        {
            string metadata = "";
            metadata += "Sorted By: " + Convert.ToString(sortType) + '\n';
            metadata += "Ordered: " + Convert.ToString(orderType) + '\n';
            return metadata;
        }

        internal static int[] GetDriverDeltasFrom(IStatistic originalStatistic, IStatistic newStatistic)
        {
            int[] originalPositions = new int[Data.NumberOfDrivers];

            for (int position = 0; position < Data.NumberOfDrivers; position++)
            {
                originalPositions[originalStatistic.DriverStats[position].CompetitorIndex] = position;
            }

            int[] deltas = new int[Data.NumberOfDrivers];

            for (int position = 0; position < Data.NumberOfDrivers; position++)
            {
                deltas[position] = originalPositions[newStatistic.DriverStats[position].CompetitorIndex] - position;
            }

            return deltas;
        }
        internal static int[] GetTeamDeltasFrom(IStatistic originalStatistic, IStatistic newStatistic)
        {
            int[] originalPositions = new int[Data.NumberOfDrivers / 2];

            for (int position = 0; position < Data.NumberOfDrivers / 2; position++)
            {
                originalPositions[originalStatistic.TeamStats[position].CompetitorIndex] = position;
            }

            int[] deltas = new int[Data.NumberOfDrivers / 2];

            for (int position = 0; position < Data.NumberOfDrivers / 2; position++)
            {
                deltas[position] = originalPositions[newStatistic.TeamStats[position].CompetitorIndex] - position;
            }

            return deltas;
        }

        public virtual SortType DefaultSortType
        {
            get { return SortType.Index; }
        }
        public virtual OrderType DefaultOrderType
        {
            get { return OrderType.Ascending; }
        }
        public virtual Competitor DefaultDisplayCompetitor
        {
            get { return Competitor.Driver; }
        }
        public virtual DisplayType DefaultDisplayType
        {
            get { return DisplayType.Value; }
        }

        public virtual IStatisticElement[] DriverStats
        {
            get
            {
                return driverStats;
            }
            set
            {
                driverStats = value;
            }
        }
        public virtual IStatisticElement[] TeamStats
        {
            get
            {
                return teamStats;
            }
            set
            {
                teamStats = value;
            }
        }
        public virtual IStatisticElement[] ComparisonStats
        {
            get
            {
                return comparisonStats;
            }
            set
            {
                comparisonStats = value;
            }
        }

        public virtual void DisplayStatistic(TeamStatsPanelControlEvents events, MainForm form)
        {
            CalculateStatistics();
            events.OnShowStatsPanel(form, this, Competitors.All());
        }
    }
}
