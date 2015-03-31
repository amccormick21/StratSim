using StratSim.Model;
using StratSim.View.MyFlowLayout;
using StratSim.View.UserControls;
using StratSim.ViewModel;
using System;
using System.Drawing;
using System.Windows.Forms;
using MyFlowLayout;

namespace StratSim.View.Panels
{
    /// <summary>
    /// Represents a panel which displays the driver's pace paramters on a panel.
    /// The data is tabulated and contains formatting and validation information.
    /// </summary>
    public class PaceParameters : MyPanel
    {
        const int leftBorder = 15;
        const int horizontalBorder = 10;
        const int defaultHeight = 15;

        Label[] headingsLabels = new Label[11];
        string[] headingsText = { "Number", "Team", "Name", "Top Speed", "Tyre Delta", "Prime Deg", "Option Deg", "Pace", "Fuel Effect", "Fuel Consumption", "P2 fuel" };
        string[] toolTipText = { "Driver Number", "Team Number", "Driver Name", "Top Speed", "Difference between tyre speeds",
                                   "Degradation per lap on prime tyre", "Degradation per lap on option tyre", "Qualifying lap time",
                                   "seconds per kilogram loss due to fuel", "kg per lap fuel consumption",
                                   "Fuel used for P2 fastest lap time"};
        int[] widths = { 50, 70, 110, 60, 50, 50, 50, 50, 70, 100, 90 };
        ParameterTextBox[,] parameterBoxes = new ParameterTextBox[8, Data.NumberOfDrivers];
        Label[,] nameBoxes = new Label[3, Data.NumberOfDrivers];

        PaceParameterData paceParameterData;

        ToolStripDropDownButton thisToolStripDropDown;
        ToolStripButton save, load, resetDropDown, updateDropDown;
        ToolStripDropDownButton goToStrategiesDropDown;

        bool isInitialised;

        /// <summary>
        /// Creates a new instance of the Pace Parameters class.
        /// </summary>
        /// <param name="FormToAdd">The form on which the panel will be added</param>
        /// <param name="loadFromFile">Represents whether the data is loaded from a file saved in main memory,
        /// or is processed from current timing data</param>
        public PaceParameters(MainForm FormToAdd, bool loadFromFile)
            : base(800, 700, "Parameters", FormToAdd, Properties.Resources.Pace_Parameters)
        {
            isInitialised = false;
            paceParameterData = new PaceParameterData(PaceParameters_DataLoaded);
            paceParameterData.InitialiseData(loadFromFile);

            OpenedInNewForm += PaceParameters_OpenedInNewForm;
            PanelClosed += PaceParameters_PanelClosed;

            AutoScroll = true;

            OriginalSize = this.Size;
        }

        void PaceParameters_OpenedInNewForm(MainForm NewForm)
        {
            AddToolStrip();
        }

        public void AddToolStrip()
        {
            PanelControlEvents.OnShowToolStrip(thisToolStripDropDown);
        }

        protected override void LinkToForm(MainForm Form)
        {
            base.LinkToForm(Form);
            paceParameterData.LinkToEvents((StratSimPanelControlEvents)PanelControlEvents);
        }

        protected override void PositionComponents()
        {
            base.PositionComponents();
            int topBorder = MyPadding.Top + 10;
        }

        void InitialiseControls()
        {
            int leftPosition = leftBorder;
            int topBorder = MyPadding.Top + 10;

            Label tempLabel;
            MyToolTip myToolTip;
            for (int column = 0; column <= 10; column++)
            {
                tempLabel = new Label();
                tempLabel.Name = "heading" + column;
                tempLabel.Text = headingsText[column];

                tempLabel.Location = new Point(leftPosition, topBorder);
                leftPosition += widths[column] + leftBorder;
                tempLabel.Size = new Size(widths[column], defaultHeight);
                myToolTip = new MyToolTip(tempLabel, toolTipText[column]);

                headingsLabels[column] = tempLabel;
                this.Controls.Add(headingsLabels[column]);
            }

            Label tempLabel2;
            ParameterTextBox tempTextBox;

            leftPosition = leftBorder;

            for (int columnIndex = 0; columnIndex <= 2; columnIndex++)
            {
                for (int rowIndex = 0; rowIndex < Data.NumberOfDrivers; rowIndex++)
                {
                    tempLabel2 = new Label();
                    tempLabel2.Location = new Point(leftPosition, topBorder + (defaultHeight * (rowIndex + 1)) + (horizontalBorder * (rowIndex + 1)));
                    tempLabel2.Size = new Size(widths[columnIndex], defaultHeight);
                    tempLabel2.Text = paceParameterData.GetData(columnIndex, rowIndex);

                    nameBoxes[columnIndex, rowIndex] = tempLabel2;
                    this.Controls.Add(nameBoxes[columnIndex, rowIndex]);
                }
                leftPosition += widths[columnIndex] + leftBorder;
            }

            for (int columnIndex = 3; columnIndex <= 10; columnIndex++)
            {
                for (int rowIndex = 0; rowIndex < Data.NumberOfDrivers; rowIndex++)
                {
                    if (columnIndex == 5) { tempTextBox = new ParameterTextBox(rowIndex, columnIndex, paceParameterData.GetPaceData(columnIndex - 3, rowIndex), 0, paceParameterData.GetPaceData(3, rowIndex)); }
                    else
                    {
                        if (columnIndex == 4) { tempTextBox = new ParameterTextBox(rowIndex, columnIndex, paceParameterData.GetPaceData(columnIndex - 3, rowIndex), 0, true); }
                        else
                        {
                            if (columnIndex == 6) { tempTextBox = new ParameterTextBox(rowIndex, columnIndex, paceParameterData.GetPaceData(columnIndex - 3, rowIndex), paceParameterData.GetPaceData(2, rowIndex), false); }
                            else
                            {
                                tempTextBox = new ParameterTextBox(rowIndex, columnIndex, paceParameterData.GetPaceData(columnIndex - 3, rowIndex), 0, false);
                            }
                        }
                    }

                    tempTextBox.Location = new Point(leftPosition, topBorder + (defaultHeight * (rowIndex + 1)) + (horizontalBorder * (rowIndex + 1)));
                    tempTextBox.Size = new Size(widths[columnIndex], defaultHeight);

                    parameterBoxes[columnIndex - 3, rowIndex] = tempTextBox;
                    parameterBoxes[columnIndex - 3, rowIndex].OnParameterChanged += paceParameterData.UpdateParameter;
                    parameterBoxes[columnIndex - 3, rowIndex].OnParameterChanged += PaceParameters_OnParameterChanged;

                    this.Controls.Add(parameterBoxes[columnIndex - 3, rowIndex]);
                }
                leftPosition += widths[columnIndex] + leftBorder;
            }

            thisToolStripDropDown = new ToolStripDropDownButton("Pace Parameters");

            save = new ToolStripButton("Save", Properties.Resources.Save);
            save.Click += save_Click;
            thisToolStripDropDown.DropDownItems.Add(save);

            load = new ToolStripButton("Load", Properties.Resources.Open);
            load.Click += load_Click;
            thisToolStripDropDown.DropDownItems.Add(load);

            resetDropDown = new ToolStripButton("Reset", Properties.Resources.Reset);
            resetDropDown.Click += resetDropDown_Click;
            thisToolStripDropDown.DropDownItems.Add(resetDropDown);

            updateDropDown = new ToolStripButton("Update", Properties.Resources.Update);
            updateDropDown.Click += updateDropDown_Click;
            thisToolStripDropDown.DropDownItems.Add(updateDropDown);

            goToStrategiesDropDown = new ToolStripDropDownButton("Start Strategies", Properties.Resources.Strategies);
            RaceDropDown FromFile = new RaceDropDown(0);
            FromFile.ButtonClicked += goToStrategies_Click;
            FromFile.Text = "From File";
            goToStrategiesDropDown.DropDownItems.Add(FromFile);
            RaceDropDown FromData = new RaceDropDown(1);
            FromData.ButtonClicked += goToStrategies_Click;
            FromData.Text = "From Data";
            goToStrategiesDropDown.DropDownItems.Add(FromData);

            thisToolStripDropDown.DropDownItems.Add(goToStrategiesDropDown);
            thisToolStripDropDown.DropDown.Width = 200;
            goToStrategiesDropDown.DropDown.Width = 200;
        }

        void PaceParameters_OnParameterChanged(TextChangedEventArgs e)
        {
            if (e.ParameterIndex - 3 == 2)
            {
                parameterBoxes[3, e.DriverIndex].LowerBound = paceParameterData.GetPaceData(2, e.DriverIndex);
            }
            if (e.ParameterIndex - 3 == 3)
            {
                parameterBoxes[2, e.DriverIndex].UpperBound = paceParameterData.GetPaceData(3, e.DriverIndex);
            }
        }

        void goToStrategies_Click(int buttonIndex, Type buttonType)
        {
            if (buttonIndex == 0)
            {
                ((StratSimPanelControlEvents)PanelControlEvents).OnLoadStrategiesFromFile();
            }
            else
            {
                ((StratSimPanelControlEvents)PanelControlEvents).OnLoadStrategiesFromData();
            }
            ((StratSimPanelControlEvents)PanelControlEvents).OnShowStrategies(ParentForm);
        }
        void updateDropDown_Click(object sender, EventArgs e)
        {
            paceParameterData.SetDriverPaceData();
        }
        void save_Click(object sender, EventArgs e)
        {
            paceParameterData.SaveData();
        }
        void load_Click(object sender, EventArgs e)
        {
            LoadDataFromFile();
        }
        void resetDropDown_Click(object sender, EventArgs e)
        {
            paceParameterData.RefreshData();
            PopulatePaceData();
        }
        void PaceParameters_DataLoaded()
        {
            if (!isInitialised)
            {
                InitialiseControls();
                AddToolStrip();
                isInitialised = true;
            }
            PopulatePaceData();
        }
        void PaceParameters_PanelClosed(MainForm LeavingForm)
        {
            PanelControlEvents.OnRemoveToolStrip(thisToolStripDropDown);
        }
        public void LoadDataFromFile()
        {
            paceParameterData.LoadData();
        }

        void PopulatePaceData()
        {
            float parameter;
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                for (int dataIndex = 0; dataIndex < 8; dataIndex++)
                {
                    parameter = paceParameterData.GetPaceData(dataIndex, driverIndex);
                    parameterBoxes[dataIndex, driverIndex].SetProperties(parameter, paceParameterData.IsDefault(parameter, dataIndex, driverIndex), paceParameterData.IsAnomaly(parameter, dataIndex));
                }
            }
        }

        public bool IsInitialised
        { get { return isInitialised; } }
    }
}
