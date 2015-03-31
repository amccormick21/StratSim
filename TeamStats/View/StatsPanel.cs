using MyFlowLayout;
using StratSim.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TeamStats.Statistics;

namespace TeamStats.View
{
    /// <summary>
    /// Represents a panel containing only two tabs, omitting the team panel.
    /// </summary>
    public class StatsPanel : MyPanel
    {
        protected Label[] rankings;
        protected Label[] competitorNumbers;
        protected Label[] competitorNames;
        protected Label[] statValues;

        protected TabControl driverTeamResults;
        Dictionary<Competitor, TabPage> dataTabs;
        ComboBox selectSortType;
        ComboBox selectSortOrder;
        ComboBox selectDisplayType;

        protected ToolStripDropDownButton thisToolStripDropDown;
        ToolStripButton writeToFile;

        protected const int LeftBuffer = 15;
        protected virtual int TopBuffer { get { return 25; } }
        protected const int ControlSpacing = 4;

        internal IStatistic Statistic { get; set; }

        SortType lastSortType;
        DisplayType lastDisplayType;
        protected ICollection<Competitor> TabsToShow;

        internal event EventHandler<SortType> SortTypeChanged;
        SortType sortType;
        internal SortType SortType
        {
            get { return sortType; }
            set
            {
                sortType = value;
                if (SortTypeChanged != null)
                    SortTypeChanged(this, sortType);
            }
        }
        internal event EventHandler<OrderType> OrderTypeChanged;
        OrderType orderType;
        internal OrderType OrderType
        {
            get { return orderType; }
            set
            {
                orderType = value;
                if (OrderTypeChanged != null)
                    OrderTypeChanged(this, orderType);
            }
        }
        internal event EventHandler<DisplayType> DisplayTypeChanged;
        DisplayType displayType;
        internal DisplayType DisplayType
        {
            get { return displayType; }
            set
            {
                displayType = value;
                if (DisplayTypeChanged != null)
                    DisplayTypeChanged(this, displayType);
            }
        }
        internal event EventHandler<Competitor> CompetitorChanged;
        Competitor competitor;
        internal Competitor Competitor
        {
            get { return competitor; }
            set
            {
                competitor = value;
                if (CompetitorChanged != null)
                    CompetitorChanged(this, competitor);
            }
        }

        public StatsPanel(MainForm parentForm, IStatistic Statistic, Competitor[] TabsToShow)
            : base(500, 650, Statistic.GetStatisticName(), parentForm, Properties.Resources.Statistics)
        {
            this.Statistic = Statistic;
            SetSortParameters();

            InitialiseControls(TabsToShow);
            PositionControls();
            AddControls(TabsToShow);
            this.TabsToShow = TabsToShow;

            SortTypeChanged += StatsPanel_SortTypeChanged;
            OrderTypeChanged += StatsPanel_SortOrderChanged;
            DisplayTypeChanged += TeamComparisonPanel_DisplayTypeChanged;
            CompetitorChanged += StatsPanel_CompetitorChanged;
            OpenedInNewForm += StatsPanel_OpenedInNewForm;
            PanelClosed += StatsPanel_PanelClosed;

            Statistic.Sort(OrderType, SortType);
            DisplayControls(Competitor);
            PopulateStatistics();

            SetPanelProperties(DockTypes.TopLeft, AutosizeTypes.AutoHeight, FillStyles.FullHeight, this.Size);
        }

        void StatsPanel_PanelClosed(MainForm LeavingForm)
        {
            PanelControlEvents.OnRemoveToolStrip(thisToolStripDropDown);
        }
        void StatsPanel_OpenedInNewForm(MainForm NewForm)
        {
            AddToolStrip();
        }

        void StatsPanel_CompetitorChanged(object sender, Competitor e)
        {
            if (driverTeamResults.SelectedIndex != (int)e)
                driverTeamResults.SelectedIndex = (int)e;
            PositionControls();
            DisplayControls(e);
            PopulateStatistics();
        }
        void TeamComparisonPanel_DisplayTypeChanged(object sender, DisplayType e)
        {
            if (selectDisplayType.SelectedIndex != (int)e)
                selectDisplayType.SelectedIndex = (int)e;

            try
            {
                PopulateStatistics();
                lastDisplayType = e;
            }
            catch (CannortDisplayByThisParameterException ex)
            {
                MessageBox.Show(ex.Message);
                selectDisplayType.SelectedIndex = (int)ex.FailedDisplayType;
                selectDisplayType.SelectedIndex = (int)lastDisplayType;
            }
        }
        void StatsPanel_SortOrderChanged(object sender, OrderType e)
        {
            if (selectSortOrder.SelectedIndex != (int)e)
                selectSortOrder.SelectedIndex = (int)e;
            Statistic.Sort(OrderType, SortType);
            PopulateStatistics();
        }
        void StatsPanel_SortTypeChanged(object sender, SortType e)
        {
            if (selectSortType.SelectedIndex != (int)e)
                selectSortType.SelectedIndex = (int)e;
            try
            {
                Statistic.Sort(OrderType, SortType);
                lastSortType = e;
            }
            catch (CannortSortByThisParameterException ex)
            {
                MessageBox.Show(ex.MessageBoxString);
                selectSortType.SelectedIndex = (int)ex.FailedSortType;
                selectSortType.SelectedIndex = (int)lastSortType;
            }
            PositionControls();
            PopulateStatistics();
        }

        protected virtual void DisplayControls(Competitor competitorType)
        {
            switch (competitorType)
            {
                case Competitor.Driver:
                    {
                        for (int position = Data.NumberOfDrivers / 2; position < Data.NumberOfDrivers; position++)
                        {
                            rankings[position].Visible = true;
                            competitorNames[position].Visible = true;
                            statValues[position].Visible = true;
                        }
                        dataTabs[competitorType].Controls.AddRange(competitorNumbers);
                        break;
                    }
                case Competitor.Comparison:
                    {
                        for (int position = Data.NumberOfDrivers / 2; position < Data.NumberOfDrivers; position++)
                        {
                            rankings[position].Visible = false;
                            competitorNames[position].Visible = true;
                            statValues[position].Visible = true;
                        }
                        dataTabs[competitorType].Controls.AddRange(competitorNumbers);
                        break;
                    }
                case Competitor.Team:
                    {
                        for (int position = Data.NumberOfDrivers / 2; position < Data.NumberOfDrivers; position++)
                        {
                            rankings[position].Visible = false;
                            competitorNames[position].Visible = false;
                            statValues[position].Visible = false;
                        }
                        break;
                    }

            }
            dataTabs[competitorType].Controls.AddRange(rankings);
            dataTabs[competitorType].Controls.AddRange(competitorNames);
            dataTabs[competitorType].Controls.AddRange(statValues);
        }

        protected virtual void AddControls(ICollection<Competitor> tabsToShow)
        {
            this.Controls.Add(selectSortOrder);
            this.Controls.Add(selectSortType);
            this.Controls.Add(selectDisplayType);
            foreach (Competitor competitor in tabsToShow)
            {
                driverTeamResults.TabPages.Add(dataTabs[competitor]);
            }
            this.Controls.Add(driverTeamResults);
        }
        protected virtual void InitialiseControls(ICollection<Competitor> tabsToShow)
        {
            InitialiseToolStrip();

            int leftPosition = LeftBuffer;
            int topPosition = TopBuffer;

            selectSortType = new ComboBox
            {
                Left = leftPosition,
                Top = topPosition,
                Width = 125,
                FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (var sortingType in (SortType[])Enum.GetValues(typeof(SortType)))
            {
                selectSortType.Items.Add(sortingType);
            }
            selectSortType.SelectedIndex = (int)Statistic.DefaultSortType;
            selectSortType.SelectedIndexChanged += sortType_SelectedIndexChanged;
            leftPosition += selectSortType.Width + 10;

            selectSortOrder = new ComboBox
            {
                Left = leftPosition,
                Top = topPosition,
                Width = 125,
                FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            selectSortOrder.Items.AddRange(new object[] { OrderType.Ascending, OrderType.Descending });
            selectSortOrder.SelectedIndex = (int)Statistic.DefaultOrderType;
            selectSortOrder.SelectedIndexChanged += selectSortOrder_SelectedIndexChanged;
            leftPosition += selectSortOrder.Width + 10;

            selectDisplayType = new ComboBox
            {
                Left = leftPosition,
                Top = topPosition,
                Width = 125,
                FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            selectDisplayType.Items.AddRange(new object[] { DisplayType.Value, DisplayType.Percentage });
            selectDisplayType.SelectedIndex = (int)Statistic.DefaultDisplayType;
            selectDisplayType.SelectedIndexChanged += selectDisplayType_SelectedIndexChanged;

            topPosition += selectSortType.Height + ControlSpacing;

            driverTeamResults = new TabControl
            {
                Left = LeftBuffer,
                Top = topPosition,
                Width = this.Width - LeftBuffer - LeftBuffer,
                Height = this.Height - topPosition - 10,
            };

            dataTabs = new Dictionary<Statistics.Competitor, TabPage>();
            foreach (Competitor competitor in tabsToShow)
            {
                dataTabs[competitor] = new TabPage
                {
                    Text = competitor.ToString(),
                    BackColor = Color.White
                };
            }

            driverTeamResults.SelectedIndex = (int)Statistic.DefaultDisplayCompetitor;
            driverTeamResults.SelectedIndexChanged += driverTeamResults_SelectedIndexChanged;

            topPosition = 10;
            leftPosition = LeftBuffer;
            rankings = new Label[Data.NumberOfDrivers];
            competitorNumbers = new Label[Data.NumberOfDrivers];
            competitorNames = new Label[Data.NumberOfDrivers];
            statValues = new Label[Data.NumberOfDrivers];

            for (int positionIndex = 0; positionIndex < Data.NumberOfDrivers; positionIndex++)
            {
                rankings[positionIndex] = new Label
                {
                    Width = 30
                };
                competitorNumbers[positionIndex] = new Label
                {
                    Width = 30
                };
                competitorNames[positionIndex] = new Label
                {
                    Width = 100
                };
                statValues[positionIndex] = new Label
                {
                    Width = 50
                };
            }

        }

        protected virtual void PositionControls()
        {
            int leftPosition = LeftBuffer;
            int topPosition = ControlSpacing;

            if (Competitor == Competitor.Comparison)
            {
                //Grid is staggered, i.e.:
                // 0 - 1
                // 2 - 3 etc.
                for (int positionIndex = 0; positionIndex < Data.NumberOfDrivers; positionIndex += 2)
                {
                    //rankings[positionIndex].Left = leftPosition;
                    //rankings[positionIndex].Top = topPosition;
                    leftPosition += rankings[positionIndex].Width + ControlSpacing;
                    competitorNumbers[positionIndex].Left = leftPosition;
                    competitorNumbers[positionIndex].Top = topPosition;
                    leftPosition += competitorNumbers[positionIndex].Width + ControlSpacing;
                    competitorNames[positionIndex].Left = leftPosition;
                    competitorNames[positionIndex].Top = topPosition;
                    leftPosition += competitorNames[positionIndex].Width + ControlSpacing;
                    statValues[positionIndex].Left = leftPosition;
                    statValues[positionIndex].Top = topPosition;
                    leftPosition += statValues[positionIndex].Width + ControlSpacing;
                    statValues[positionIndex + 1].Left = leftPosition;
                    statValues[positionIndex + 1].Top = topPosition;
                    leftPosition += statValues[positionIndex + 1].Width + ControlSpacing;
                    competitorNames[positionIndex + 1].Left = leftPosition;
                    competitorNames[positionIndex + 1].Top = topPosition;
                    leftPosition += competitorNames[positionIndex + 1].Width + ControlSpacing;
                    competitorNumbers[positionIndex + 1].Left = leftPosition;
                    competitorNumbers[positionIndex + 1].Top = topPosition;

                    leftPosition = LeftBuffer;
                    topPosition += rankings[0].Height;
                }
            }
            else
            {
                //Move back down to bottom.
                for (int positionIndex = 0; positionIndex < Data.NumberOfDrivers; positionIndex++)
                {
                    rankings[positionIndex].Left = leftPosition;
                    rankings[positionIndex].Top = topPosition;
                    leftPosition += rankings[positionIndex].Width + ControlSpacing;
                    competitorNumbers[positionIndex].Left = leftPosition;
                    competitorNumbers[positionIndex].Top = topPosition;
                    leftPosition += competitorNumbers[positionIndex].Width + ControlSpacing;
                    competitorNames[positionIndex].Left = leftPosition;
                    competitorNames[positionIndex].Top = topPosition;
                    leftPosition += competitorNames[positionIndex].Width + ControlSpacing;
                    statValues[positionIndex].Left = leftPosition;
                    statValues[positionIndex].Top = topPosition;

                    leftPosition = LeftBuffer;
                    topPosition += rankings[0].Height;
                }
            }
        }
        protected virtual void InitialiseToolStrip()
        {
            thisToolStripDropDown = new ToolStripDropDownButton(Statistic.GetStatisticName());

            writeToFile = new ToolStripButton("WriteToFile");
            writeToFile.Click += writeToFile_Click;

            thisToolStripDropDown.DropDownItems.Add(writeToFile);

        }
        public void AddToolStrip()
        {
            PanelControlEvents.OnShowToolStrip(thisToolStripDropDown);
            thisToolStripDropDown.DropDown.Width = 150;
        }
        protected virtual void SetSortParameters()
        {
            Competitor = Statistic.DefaultDisplayCompetitor;
            SortType = Statistic.DefaultSortType;
            OrderType = Statistic.DefaultOrderType;
            Statistic.Sort(OrderType, SortType);
        }

        protected virtual void driverTeamResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            Competitor = (Competitor)driverTeamResults.SelectedIndex;
        }
        private void selectDisplayType_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayType = (DisplayType)selectDisplayType.SelectedIndex;
        }
        protected virtual void selectSortOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            OrderType = (OrderType)selectSortOrder.SelectedIndex;
        }
        protected virtual void sortType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SortType = (SortType)selectSortType.SelectedIndex;
        }
        protected virtual void writeToFile_Click(object sender, EventArgs e)
        {
            WriteStatisticsToFile();
        }

        protected virtual void PopulateStatistics()
        {
            int numberOfStatsToDisplay = GetNumberOfStatsToDisplay(Competitor);

            for (int position = 0; position < numberOfStatsToDisplay; position++)
            {
                rankings[position].Text = GetPositionText(position);
                competitorNumbers[position].Text = GetCompetitorNumberText(position);
                competitorNames[position].Text = GetCompetitorNameText(position);
                statValues[position].Text = GetStatisticText(position);
            }
        }
        protected virtual int GetNumberOfStatsToDisplay(Competitor competitor)
        {
            int numberOfStatsToDisplay;
            switch (competitor)
            {
                case Competitor.Driver:
                case Competitor.Comparison:
                    numberOfStatsToDisplay = Data.NumberOfDrivers; break;
                case Competitor.Team:
                    numberOfStatsToDisplay = Data.NumberOfDrivers / 2; break;
                default:
                    numberOfStatsToDisplay = 0; break;
            }
            return numberOfStatsToDisplay;
        }
        protected virtual void WriteStatisticsToFile()
        {
            //Could be user defined:
            string fileName = Statistic.GetStatisticName();
            fileName += " " + Convert.ToString(DateTime.Now.TimeOfDay).Replace(':', '.');
            fileName += ".csv";

            string filePath = Data.Settings.DataFilePath + "Statistics/";
            string fullFilePath = Path.Combine(filePath, fileName);

            using (StreamWriter sw = new StreamWriter(fullFilePath))
            {
                foreach (var competitorType in TabsToShow)
                {
                    if (competitorType == Competitor.Comparison)
                    {
                        for (int position = 0; position < Data.NumberOfDrivers / 2; position++)
                        {
                            sw.Write(GetPositionText(position));
                            sw.Write(",");
                            sw.Write(Statistic.GetCompetitorNumberText(position, competitorType));
                            sw.Write(",");
                            sw.Write(Statistic.GetCompetitorNameText(position, competitorType));
                            sw.Write(",");
                            sw.Write(Statistic.GetStatValueText(position, competitorType, DisplayType));
                            sw.Write(",");
                            sw.Write(Statistic.GetStatValueText(position + 1, competitorType, DisplayType));
                            sw.Write(",");
                            sw.Write(Statistic.GetCompetitorNameText(position + 1, competitorType));
                            sw.Write(",");
                            sw.Write(Statistic.GetCompetitorNumberText(position + 1, competitorType));
                            sw.WriteLine();
                        }
                    }
                    else
                    {
                        for (int position = 0; position < GetNumberOfStatsToDisplay(competitorType); position++)
                        {
                            sw.Write(GetPositionText(position));
                            sw.Write(",");
                            sw.Write(Statistic.GetCompetitorNumberText(position, competitorType));
                            sw.Write(",");
                            sw.Write(Statistic.GetCompetitorNameText(position, competitorType));
                            sw.Write(",");
                            sw.Write(Statistic.GetStatValueText(position, competitorType, DisplayType));
                            sw.WriteLine();
                        }
                    }
                }
                sw.WriteLine(Statistic.GetStatisticMetadata(OrderType, SortType));
            }
        }

        protected virtual string GetPositionText(int position)
        {
            return Convert.ToString(position + 1);
        }
        protected virtual string GetCompetitorNumberText(int position)
        {
            return Statistic.GetCompetitorNumberText(position, Competitor);
        }
        protected virtual string GetCompetitorNameText(int position)
        {
            return Statistic.GetCompetitorNameText(position, Competitor);
        }
        protected virtual string GetStatisticText(int position)
        {
            return Statistic.GetStatValueText(position, Competitor, DisplayType);
        }

    }
}
