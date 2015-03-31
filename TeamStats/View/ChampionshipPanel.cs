using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyFlowLayout;
using TeamStats;
using System.Windows.Forms;
using StratSim.Model;
using TeamStats.ViewModel;
using TeamStats.MyStatistics;
using System.Drawing;
using DataSources.DataConnections;
using TeamStats.MyFlowLayout;

namespace TeamStats.View
{
    public class ChampionshipPanel : MyPanel
    {
        ChampionshipData DataController;

        TabControl Championships;
        TabPage Drivers;
        TabPage Teams;

        ComboBox PointSystemSelectBox, SessionSelectBox;
        CheckBox DoublePointsSelectBox;
        Button EnterData;
        Button ClearComparison;

        Label[] Positions; Label[] TeamPositions;
        Label[] Number;
        Label[] Names; Label[] TeamNames;
        Label[] Points; Label[] TeamPoints;
        Label[] PointsDelta; Label[] TeamPointsDelta;
        Label[] PositionDelta; Label[] TeamPositionDelta;

        public ChampionshipPanel(MainForm ParentForm)
            : base(325, 600, "Championships", ParentForm, Properties.Resources.Championships)
        {
            InitialiseControls();
            AddControls();

            DataController = new ChampionshipData(PointScoringSystem.Post2009, true, Session.Race);
            DataController.ChampionshipsModified += DataController_ChampionshipsModified;
            DataController.DriverDeltasUpdated += DataController_DriverDeltasUpdated;
            DataController.TeamDeltasUpdated += DataController_TeamDeltasUpdated;
            DataController.ComparisonCleared += DataController_ComparisonCleared;
            DataController.SessionChanged += DataController_SessionChanged;

            UpdateDriverPoints(DataController.Statistic);

            SetPanelProperties(DockTypes.Right, AutosizeTypes.AutoHeight, FillStyles.FullHeight, this.Size);
        }

        void DataController_SessionChanged(object sender, Session e)
        {
            SessionSelectBox.SelectedIndexChanged -= SessionSelectBox_SelectedIndexChanged;
            SessionSelectBox.SelectedIndex = (int)e;
            SessionSelectBox.SelectedIndexChanged += SessionSelectBox_SelectedIndexChanged;
        }

        void DataController_ComparisonCleared(object sender, EventArgs e)
        {
            ClearComparisonData();
        }

        private void ClearComparisonData()
        {
            for (int row = 0; row < Data.NumberOfDrivers; row++)
            {
                PositionDelta[row].Text = "";
            }
            for (int row = 0; row < Data.NumberOfDrivers; row++)
            {
                TeamPositionDelta[row / 2].Text = "";
            }
        }

        void DataController_TeamDeltasUpdated(object sender, int[] e)
        {
            int delta;
            for (int row = 0; row < Data.NumberOfDrivers/2; row++)
            {
                delta = e[row];
                TeamPositionDelta[row].Text = Convert.ToString(delta);
                if (delta > 0)
                {
                    TeamPositionDelta[row].ForeColor = Color.Green;
                }
                else if (delta < 0)
                {
                    TeamPositionDelta[row].ForeColor = Color.Red;
                }
                else
                {
                    TeamPositionDelta[row].ForeColor = Color.Black;
                }
            }
        }

        void DataController_DriverDeltasUpdated(object sender, int[] e)
        {
            int delta;
            for (int row = 0; row < Data.NumberOfDrivers; row++)
            {
                delta = e[row];
                PositionDelta[row].Text = Convert.ToString(delta);
                if (delta > 0)
                {
                    PositionDelta[row].ForeColor = Color.Green;
                }
                else if (delta < 0)
                {
                    PositionDelta[row].ForeColor = Color.Red;
                }
                else
                {
                    PositionDelta[row].ForeColor = Color.Black;
                }
            }
        }

        void DataController_ChampionshipsModified(object sender, ChampionshipStatistic e)
        {
            UpdateDriverPoints(e);
        }

        protected override void PositionComponents()
        {
            base.PositionComponents();
            Championships.Width = this.Width - 20;
            Championships.Height = this.Height - 125;
            EnterData.Top = this.Height - 40;
            EnterData.Left = this.Width - 110;
            ClearComparison.Left = this.Width - 220;
            ClearComparison.Top = this.Height - 40;
        }

        void InitialiseControls()
        {
            PointSystemSelectBox = new ComboBox
            {
                Width = 100,
                Left = 10,
                Top = 25,
                FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle,
                DropDownStyle = ComboBoxStyle.DropDownList,
            };

            PointSystemSelectBox.Items.Add(PointScoringSystem.Post1990);
            PointSystemSelectBox.Items.Add(PointScoringSystem.Post2002);
            PointSystemSelectBox.Items.Add(PointScoringSystem.Post2009);
            PointSystemSelectBox.SelectedIndex = 2;
            PointSystemSelectBox.SelectedIndexChanged += PointSystemSelectBox_SelectedIndexChanged;

            SessionSelectBox = new ComboBox
            {
                Width = 100,
                Left = 10,
                Top = 50,
                FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            foreach (var session in (Session[])Enum.GetValues(typeof(Session)))
            {
                SessionSelectBox.Items.Add(Convert.ToString(session));
            }
            SessionSelectBox.SelectedIndex = SessionSelectBox.Items.Count - 1;
            SessionSelectBox.SelectedIndexChanged += SessionSelectBox_SelectedIndexChanged;

            DoublePointsSelectBox = new CheckBox
            {
                Left = 145,
                Top = 25,
                FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle,
                Text = "Double Points",
                Checked = true,
            };
            DoublePointsSelectBox.CheckedChanged += DoublePointsSelectBox_CheckedChanged;

            int topPositionStep = 23;

            Positions = new Label[Data.NumberOfDrivers];
            Number = new Label[Data.NumberOfDrivers];
            Names = new Label[Data.NumberOfDrivers];
            Points = new Label[Data.NumberOfDrivers];
            PointsDelta = new Label[Data.NumberOfDrivers];
            PositionDelta = new Label[Data.NumberOfDrivers];
            TeamPositions = new Label[Data.NumberOfDrivers / 2];
            TeamNames = new Label[Data.NumberOfDrivers / 2];
            TeamPoints = new Label[Data.NumberOfDrivers / 2];
            TeamPointsDelta = new Label[Data.NumberOfDrivers / 2];
            TeamPositionDelta = new Label[Data.NumberOfDrivers / 2];

            Championships = new TabControl()
            {
                Left = 10,
                Top = 80,
                Width = this.Width - 20,
                Height = this.Height - 125
            };

            Drivers = new TabPage
            {
                Text = "Drivers",
                BackColor = System.Drawing.Color.White,
                AutoScroll = true
            };


            Teams = new TabPage
            {
                Text = "Teams",
                BackColor = System.Drawing.Color.White,
            };

            InitialiseDrivers(topPositionStep);
            InitialiseTeams(topPositionStep);

            EnterData = new Button
            {
                Width = 100,
                Height = 25,
                Top = this.Height - 40,
                Left = this.Width - 110,
                FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle,
                Text = "Enter Results"
            };
            EnterData.Click += EnterData_Click;

            ClearComparison = new Button
            {
                Width = 100,
                Height = 25,
                Top = this.Height - 40,
                Left = this.Width - 220,
                FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle,
                Text = "Clear Comparison"
            };
            ClearComparison.Click += ClearComparison_Click;
        }

        void SessionSelectBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataController.SetSession((Session)SessionSelectBox.SelectedIndex);
        }

        void ClearComparison_Click(object sender, EventArgs e)
        {
            ClearComparisonData();
        }

        void EnterData_Click(object sender, EventArgs e)
        {
            ((TeamStatsPanelControlEvents)PanelControlEvents).OnShowResultGrid(this.ParentForm, DataController.GetSession());
        }

        private void InitialiseTeams(int topPositionStep)
        {
            int topPosition = 10;
            for (int teamIndex = 0; teamIndex < Data.NumberOfDrivers / 2; teamIndex++)
            {
                TeamPositions[teamIndex] = new Label
                {
                    Text = Convert.ToString(teamIndex + 1),
                    Width = 30,
                    Left = 10,
                    Top = topPosition
                };

                TeamNames[teamIndex] = new Label
                {
                    Width = 120,
                    Left = 60,
                    Top = topPosition
                };

                TeamPoints[teamIndex] = new Label
                {
                    Width = 30,
                    Left = 185,
                    Top = topPosition
                };

                TeamPointsDelta[teamIndex] = new Label
                {
                    Width = 30,
                    Left = 220,
                    Top = topPosition
                };

                TeamPositionDelta[teamIndex] = new Label
                {
                    Width = 30,
                    Left = 255,
                    Top = topPosition
                };

                topPosition += topPositionStep;
            }
        }

        private void InitialiseDrivers(int topPositionStep)
        {
            int topPosition = 10;
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                Positions[driverIndex] = new Label
                {
                    Text = Convert.ToString(driverIndex + 1),
                    Width = 20,
                    Left = 10,
                    Top = topPosition
                };

                Number[driverIndex] = new Label
                {
                    Width = 20,
                    Left = 35,
                    Top = topPosition
                };

                Names[driverIndex] = new Label
                {
                    Width = 120,
                    Left = 60,
                    Top = topPosition
                };

                Points[driverIndex] = new Label
                {
                    Width = 30,
                    Left = 185,
                    Top = topPosition
                };

                PointsDelta[driverIndex] = new Label
                {
                    Width = 30,
                    Left = 220,
                    Top = topPosition
                };

                PositionDelta[driverIndex] = new Label
                {
                    Width = 30,
                    Left = 255,
                    Top = topPosition
                };

                topPosition += topPositionStep;
            }
        }

        void DoublePointsSelectBox_CheckedChanged(object sender, EventArgs e)
        {
            if (DataController != null)
                DataController.DoublePoints = DoublePointsSelectBox.Checked;
        }

        void PointSystemSelectBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DataController != null)
                DataController.PointSystem = (PointScoringSystem)PointSystemSelectBox.SelectedIndex;
        }

        void AddControls()
        {
            this.Controls.Add(Championships);

            Championships.TabPages.Add(Drivers);
            Championships.TabPages.Add(Teams);

            this.Controls.Add(PointSystemSelectBox);
            this.Controls.Add(SessionSelectBox);
            this.Controls.Add(DoublePointsSelectBox);
            this.Controls.Add(EnterData);
            this.Controls.Add(ClearComparison);

            Drivers.Controls.AddRange(Positions);
            Drivers.Controls.AddRange(Number);
            Drivers.Controls.AddRange(Names);
            Drivers.Controls.AddRange(Points);
            Drivers.Controls.AddRange(PointsDelta);
            Drivers.Controls.AddRange(PositionDelta);
            Teams.Controls.AddRange(TeamPositions);
            Teams.Controls.AddRange(TeamNames);
            Teams.Controls.AddRange(TeamPoints);
            Teams.Controls.AddRange(TeamPointsDelta);
            Teams.Controls.AddRange(TeamPositionDelta);
        }

        void UpdateDriverPoints(ChampionshipStatistic statistic)
        {
            Driver driver;
            int pointsDelta = 0;
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                driver = DataController.GetDriverAtPosition(statistic, driverIndex);
                Number[driverIndex].Text = Convert.ToString(driver.DriverNumber);
                Names[driverIndex].Text = Convert.ToString(driver.DriverName);
                Names[driverIndex].ForeColor = driver.LineColour;

                Points[driverIndex].Text = Convert.ToString(DataController.GetDriverPoints(statistic, driverIndex));
                if (driverIndex != 0)
                {
                    pointsDelta = DataController.GetDriverPointsDelta(statistic, driverIndex);
                    PointsDelta[driverIndex].Text = Convert.ToString(pointsDelta);
                }
            }

            Team team;
            for (int teamIndex = 0; teamIndex < Data.NumberOfDrivers / 2; teamIndex++)
            {
                team = DataController.GetTeamAtPosition(statistic, teamIndex);
                TeamNames[teamIndex].Text = Convert.ToString(team.TeamName);
                TeamPoints[teamIndex].Text = Convert.ToString(DataController.GetTeamPoints(statistic, teamIndex));
                if (teamIndex != 0)
                {
                    pointsDelta = DataController.GetTeamPointsDelta(statistic, teamIndex);
                    TeamPointsDelta[teamIndex].Text = Convert.ToString(pointsDelta);
                }
            }
        }
    }
}
