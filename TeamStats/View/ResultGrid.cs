using System;
using MyFlowLayout;
using System.Windows.Forms;
using StratSim.Model;
using TeamStats.ViewModel;
using DataSources.DataConnections;

namespace TeamStats.View
{
    public class ResultGrid : MyPanel
    {
        internal enum ResultDisplayType { ByPosition, ByDriver };

        IndexedLabel[] Rounds;
        Label[] RowLabels;
        ResultTextBox[,] DataBoxes;
        Button Confirm, Reject;

        ResultData resultData;

        ToolStripDropDownButton thisToolStripDropDown;
        ToolStripButton confirmDropDown, rejectDropDown, showChanges, hideChanges;
        ToolStripDropDownButton displayBy, selectSession;

        ResultDisplayType displayType;

        public ResultGrid(MainForm ParentForm, Session session)
            : base(600, 700, "Results", ParentForm, Properties.Resources.Results)
        {
            this.displayType = ResultDisplayType.ByPosition;

            resultData = new ResultData(session);

            InitialiseControls();
            AddControls();
            PopulateData(this.DisplayType);

            DisplayTypeChanged += ResultGrid_DisplayTypeChanged;
            OpenedInNewForm += ResultGrid_OpenedInNewForm;
            PanelClosed += ResultGrid_PanelClosed;
            resultData.ResultsModified += resultData_ResultsModified;

            SetPanelProperties(DockTypes.Top, AutosizeTypes.Free, FillStyles.None, this.Size);
        }

        void resultData_ResultsModified(object sender, EventArgs e)
        {
            PopulateData(this.DisplayType);
        }

        void ResultGrid_PanelClosed(MainForm LeavingForm)
        {
            PanelControlEvents.OnRemoveToolStrip(thisToolStripDropDown);
        }

        void ResultGrid_OpenedInNewForm(MainForm NewForm)
        {
            AddToolStrip();
        }

        public void AddToolStrip()
        {
            PanelControlEvents.OnShowToolStrip(thisToolStripDropDown);
            thisToolStripDropDown.DropDown.Width = 150;
        }

        private void PopulateData(ResultGrid.ResultDisplayType displayType)
        {
            Result[] columnData = new Result[Data.NumberOfDrivers];

            for (int row = 0; row < Data.NumberOfDrivers; row++)
            {
                if (displayType == ResultDisplayType.ByDriver)
                {
                    RowLabels[row].Text = Convert.ToString(Data.Drivers[row].DriverNumber);
                    RowLabels[row].ForeColor = Data.Drivers[row].LineColour;
                }
                if (displayType == ResultDisplayType.ByPosition)
                {
                    RowLabels[row].Text = Convert.ToString(row + 1);
                    RowLabels[row].ForeColor = System.Drawing.Color.Black;
                }
            }

            for (int round = 0; round < Data.NumberOfTracks; round++)
            {
                if (displayType == ResultDisplayType.ByDriver)
                {
                    columnData = resultData.GetResultsForRoundByDriver(round);
                }
                if (displayType == ResultDisplayType.ByPosition)
                {
                    columnData = resultData.GetResultsForRoundByPosition(round);
                }
                for (int row = 0; row < Data.NumberOfDrivers; row++)
                {
                    DataBoxes[row, round].Text = Convert.ToString(columnData[row].position);
                    DataBoxes[row, round].ResetParameters();
                    DataBoxes[row, round].SetFinishingState(columnData[row].finishState);
                }
            }
        }

        void SaveData()
        {
            Result[,] results = ParseTextBoxes(true,false,true);
            resultData.SetResultsInDatabase(results);
        }

        private Result[,] ParseTextBoxes(bool resetParameters, bool checkNonZero, bool checkModified)
        {
            Result[,] results = new Result[Data.NumberOfDrivers, Data.NumberOfTracks]; // = position
            int thisDriverIndex;

            for (int row = 0; row < Data.NumberOfDrivers; row++)
            {
                for (int round = 0; round < Data.NumberOfTracks; round++)
                {
                    if ((!checkModified  || DataBoxes[row, round].textChanged)
                        && (!checkNonZero || Convert.ToInt32(DataBoxes[row, round].Text) == 0))
                    {
                        if (DisplayType == ResultDisplayType.ByDriver)
                        {
                            results[row, round] = new Result
                            {
                                position = Convert.ToInt32(DataBoxes[row, round].Text),
                                modified = true,
                                finishState = DataBoxes[row, round].FinishingState
                            };
                        }
                        if (DisplayType == ResultDisplayType.ByPosition)
                        {
                            thisDriverIndex = Driver.ConvertToDriverIndex(Convert.ToInt32(DataBoxes[row, round].Text));
                            if (thisDriverIndex != -1)
                            {
                                results[Driver.ConvertToDriverIndex(Convert.ToInt32(DataBoxes[row, round].Text)), round] = new Result
                                {
                                    position = row + 1,
                                    modified = true,
                                    finishState = DataBoxes[row, round].FinishingState
                                };
                            }
                        }
                        
                        if (resetParameters)
                            DataBoxes[row, round].ResetParameters();
                    }
                }
            }
            return results;
        }

        void ResultGrid_DisplayTypeChanged(object sender, EventArgs e)
        {
            PopulateData(DisplayType);
        }

        protected override void PositionComponents()
        {
            base.PositionComponents();
            Confirm.Location = new System.Drawing.Point(this.Width - 220, this.Height - 40);
            Reject.Location = new System.Drawing.Point(this.Width - 110, this.Height - 40);
        }

        void InitialiseControls()
        {
            InitialiseToolStrip();

            int rowSpacing = 26;
            int columnSpacing = 40;

            DataBoxes = new ResultTextBox[Data.NumberOfDrivers, Data.NumberOfTracks];
            Rounds = new IndexedLabel[Data.NumberOfTracks];
            RowLabels = new Label[Data.NumberOfDrivers];

            int topPosition = 50;
            int leftPosition = 40;
            int tabIndex = 0;

            for (int racePosition = 0; racePosition < Data.NumberOfDrivers; racePosition++)
            {
                RowLabels[racePosition] = new Label
                {
                    Left = 10,
                    Width = 30,
                    Top = topPosition,
                };

                leftPosition = 40;
                tabIndex = racePosition;
                for (int roundIndex = 0; roundIndex < Data.NumberOfTracks; roundIndex++)
                {
                    if (racePosition == 0)
                    {
                        Rounds[roundIndex] = new IndexedLabel(roundIndex)
                        {
                            Left = leftPosition,
                            Top = 30,
                            Width = columnSpacing
                        };
                        Rounds[roundIndex].ResetColumn += ResultGrid_ResetColumn;
                    }

                    DataBoxes[racePosition, roundIndex] = new ResultTextBox(racePosition, roundIndex)
                    {
                        Width = columnSpacing - 6,
                        Height = rowSpacing - 8,
                        Left = leftPosition,
                        Top = topPosition,
                        TabIndex = tabIndex
                    };
                    DataBoxes[racePosition, roundIndex].FinishingStateChanged += ResultGrid_FinishingStateChanged;
                    DataBoxes[racePosition, roundIndex].MoveToNext += ResultGrid_MoveToNext;

                    tabIndex += Data.NumberOfDrivers;
                    leftPosition += columnSpacing;
                }
                topPosition += rowSpacing;
            }

            Confirm = new Button
            {
                FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle,
                Text = "Confirm",
                Width = 100,
                Height = 25,
                Left = this.Width - 220,
                Top = this.Height - 40
            };
            Confirm.Click += Confirm_Click;

            Reject = new Button
            {
                FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle,
                Text = "Reject",
                Width = 100,
                Height = 25,
                Left = this.Width - 110,
                Top = this.Height - 40
            };
            Reject.Click += Reject_Click;
        }

        void ResultGrid_FinishingStateChanged(object sender, FinishingState e)
        {
            if (DisplayType == ResultDisplayType.ByPosition)
            {
                ResultTextBox textBox = (ResultTextBox)sender;

                int round = textBox.Column;
                int changedRow = textBox.Row;

                for (int row = changedRow + 1; row < Data.NumberOfDrivers; row++)
                {
                    DataBoxes[row, round].SetFinishingState(e);
                }
            }
        }

        private void InitialiseToolStrip()
        {
            thisToolStripDropDown = new ToolStripDropDownButton("Results");

            confirmDropDown = new ToolStripButton("Confirm");
            confirmDropDown.Click += confirmDropDown_Click;

            rejectDropDown = new ToolStripButton("Reject");
            rejectDropDown.Click += rejectDropDown_Click;

            showChanges = new ToolStripButton("Show Changes");
            showChanges.Click += showChanges_Click;

            hideChanges = new ToolStripButton("Hide Changes");
            hideChanges.Click += hideChanges_Click;

            displayBy = new ToolStripDropDownButton("Display By");
            selectSession = new ToolStripDropDownButton("Select Session");
            displayBy.DropDown.Width = 200;
            selectSession.DropDown.Width = 200;

            IndexedToolStripButton button;
            foreach (var displayType in (ResultDisplayType[])Enum.GetValues(typeof(ResultDisplayType)))
            {
                button = new IndexedToolStripButton((int)displayType);
                button.Text = Convert.ToString(displayType);
                button.CheckOnClick = false;
                button.ButtonClicked += displayType_ButtonClicked;
                displayBy.DropDownItems.Add(button);
            }

            foreach (var session in (Session[])Enum.GetValues(typeof(Session)))
            {
                button = new IndexedToolStripButton((int)session);
                button.Text = Convert.ToString(session);
                button.CheckOnClick = false;
                button.ButtonClicked += selectSession_ButtonClicked;
                selectSession.DropDownItems.Add(button);
            }

            thisToolStripDropDown.DropDownItems.Add(confirmDropDown);
            thisToolStripDropDown.DropDownItems.Add(rejectDropDown);
            thisToolStripDropDown.DropDownItems.Add(showChanges);
            thisToolStripDropDown.DropDownItems.Add(hideChanges);
            thisToolStripDropDown.DropDownItems.Add(displayBy);
            thisToolStripDropDown.DropDownItems.Add(selectSession);
        }

        void ResetColumn(int raceIndexToReset)
        {
            Result[,] results = ParseTextBoxes(true, true, true);
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                results[driverIndex, raceIndexToReset] = new Result
                {
                    position = 0,
                    finishState = FinishingState.Finished,
                    modified = true
                };
            }
            resultData.SetResultsInDatabase(results);
        }

        void ResultGrid_ResetColumn(object sender, int e)
        {
            ResetColumn(e);
        }

        private void selectSession_ButtonClicked(object sender, int e)
        {
            resultData.SetSession((Session)e);
        }

        private void hideChanges_Click(object sender, EventArgs e)
        {
            resultData.HideChangesToResults();
        }

        void showChanges_Click(object sender, EventArgs e)
        {
            Result[,] newResults = ParseTextBoxes(false,false,false);
            resultData.ShowChangesToResults(newResults);
        }

        void displayType_ButtonClicked(object sender, int e)
        {
            this.DisplayType = (ResultDisplayType)e;
        }

        void rejectDropDown_Click(object sender, EventArgs e)
        {
            PopulateData(DisplayType);
        }

        void confirmDropDown_Click(object sender, EventArgs e)
        {
            SaveData();
        }

        void Reject_Click(object sender, EventArgs e)
        {
            PopulateData(DisplayType);
        }

        void Confirm_Click(object sender, EventArgs e)
        {
            SaveData();
        }

        void ResultGrid_MoveToNext(object sender, EventArgs e)
        {
            ResultTextBox t = (ResultTextBox)sender;
            if (t.Row == Data.NumberOfDrivers - 1)
                Confirm.Focus();
            else
                DataBoxes[t.Row + 1, t.Column].Focus();
        }

        private void AddControls()
        {
            for (int row = 0; row < Data.NumberOfDrivers; row++)
            {
                for (int round = 0; round < Data.NumberOfTracks; round++)
                {
                    this.Controls.Add(DataBoxes[row, round]);
                }
            }
            this.Controls.AddRange(RowLabels);
            this.Controls.AddRange(Rounds);
            this.Controls.Add(Confirm);
            this.Controls.Add(Reject);
        }

        internal event EventHandler DisplayTypeChanged;
        internal ResultDisplayType DisplayType
        {
            get { return displayType; }
            set { displayType = value; DisplayTypeChanged(this, new EventArgs()); }
        }
    }
}
