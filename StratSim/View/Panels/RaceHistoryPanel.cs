using DataSources.DataConnections;
using MyFlowLayout;
using StratSim.Model;
using StratSim.Model.Files;
using StratSim.Model.RaceHistory;
using StratSim.View.MyFlowLayout;
using StratSim.View.UserControls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StratSim.View.Panels
{
    public class RaceHistoryPanel : MyPanel
    {
        int raceLaps;

        //Properties
        const int leftBorder = 10;
        const int topBorder = 10;
        const int defaultHeight = 25;
        const int defaultWidth = 100;
        Size cbxDefault = new Size(defaultWidth, defaultHeight);
        Size btnDefault = new Size(defaultWidth, defaultHeight);

        //Controls
        ComboBox selectYear = new ComboBox();
        ComboBox selectRace = new ComboBox();
        Button startSimulation = new Button();
        Button loadFromFile = new Button();
        Panel simulationPanel = new Panel();
        ListBox lapList = new ListBox();
        Panel stintPanels = new Panel();
        ListBox actionsList = new ListBox();
        ComboBox selectDriver = new ComboBox();
        Button changeAction = new Button();
        Button changeLap = new Button();
        TextBox newLapTime = new TextBox();
        ComboBox newLapAction = new ComboBox();
        Button updateSimulation = new Button();

        ToolStripDropDownButton thisToolStripDropDown;
        ToolStripButton save, load;

        bool dataModified;
        RaceHistorySimulation simulation;

        public RaceHistoryPanel(MainForm formToAdd)
            : base(600, 620, "Race History", formToAdd, Properties.Resources.Archives)
        {
            InitialiseObjects();
            InitialiseToolbar();
            LoadControls();
            PanelClosed += RaceHistoryPanel_PanelClosed;
            OpenedInNewForm += RaceHistoryPanel_OpenedInNewForm;
            dataModified = false;
        }

        private void RaceHistoryPanel_OpenedInNewForm(MainForm NewForm)
        {
            AddToolStrip();
        }

        private void RaceHistoryPanel_PanelClosed(MainForm LeavingForm)
        {
            PanelControlEvents.OnRemoveToolStrip(thisToolStripDropDown);
        }

        private void LoadYearComboBox()
        {
            selectYear.SelectedIndexChanged -= SelectYear_SelectedIndexChanged;
            selectYear.Items.Clear();
            for (int year = 2014; year <= DateTime.Now.Year; year++)
            {
                selectYear.Items.Add(year);
            }
            selectYear.SelectedIndexChanged += SelectYear_SelectedIndexChanged;
        }

        private void LoadTracksComboBox()
        {
            selectRace.SelectedIndexChanged -= SelectRace_SelectedIndexChanged;
            selectRace.Items.Clear();
            for (int raceIndex = 0; raceIndex < Data.NumberOfTracks; raceIndex++)
            {
                selectRace.Items.Add(Data.Tracks[raceIndex].name);
            }
            selectRace.Text = "Select Race";
            selectRace.SelectedIndexChanged += SelectRace_SelectedIndexChanged;
        }

        private void LoadDriversComboBox()
        {
            selectDriver.SelectedIndexChanged -= SelectDriver_SelectedIndexChanged;
            selectDriver.Items.Clear();
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                selectDriver.Items.Add(Data.Drivers[driverIndex].DriverName);
            }
            selectDriver.SelectedIndexChanged += SelectDriver_SelectedIndexChanged;
            selectDriver.Text = "Select Driver";
        }

        private void LoadActionsComboBox()
        {
            foreach (var action in (RaceLapAction[])Enum.GetValues(typeof(RaceLapAction)))
            {
                newLapAction.Items.Add(action);
            }
        }

        private void LoadControls()
        {
            LoadYearComboBox();
            LoadTracksComboBox();
            LoadDriversComboBox();
            LoadActionsComboBox();
        }

        private void InitialiseObjects()
        {
            MyToolTip toolTip;

            selectYear.Location = new Point(leftBorder, topBorder + 25);
            selectYear.Size = cbxDefault;
            selectYear.Text = "Select Year";
            selectYear.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            Controls.Add(selectYear);

            selectRace.Location = new Point(leftBorder * 2 + cbxDefault.Width, topBorder + 25);
            selectRace.Size = cbxDefault;
            selectRace.Text = "Select Race";
            selectRace.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            Controls.Add(selectRace);

            startSimulation.Location = new Point(leftBorder * 3 + cbxDefault.Width * 2, topBorder + 25);
            startSimulation.Size = btnDefault;
            startSimulation.Text = "Start Simulation";
            toolTip = new MyToolTip(startSimulation, "Start a history simulation");
            startSimulation.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            startSimulation.Click += StartSimulation_Click;
            Controls.Add(startSimulation);

            loadFromFile.Location = new Point(leftBorder * 4 + cbxDefault.Width * 3, topBorder + 25);
            loadFromFile.Size = btnDefault;
            loadFromFile.Text = "Load Simulation";
            toolTip = new MyToolTip(loadFromFile, "Load a simulation from file");
            loadFromFile.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            loadFromFile.Click += LoadFromFile_Click;
            Controls.Add(loadFromFile);

            simulationPanel.Location = new Point(leftBorder, topBorder * 2 + cbxDefault.Height + 25);
            simulationPanel.Size = new Size(620 - 2 * leftBorder, 520);
            Controls.Add(simulationPanel);

            lapList.Location = new Point(leftBorder, topBorder);
            lapList.Size = new Size(160, 500);
            lapList.SelectedIndexChanged += LapList_SelectedIndexChanged;
            simulationPanel.Controls.Add(lapList);

            stintPanels.Location = new Point(190, topBorder);
            stintPanels.Size = new Size(280, 270);
            stintPanels.AutoScroll = true;
            simulationPanel.Controls.Add(stintPanels);

            actionsList.Location = new Point(190, topBorder * 2 + 270);
            actionsList.Size = new Size(280, 200);
            actionsList.SelectedIndexChanged += ActionsList_SelectedIndexChanged;
            simulationPanel.Controls.Add(actionsList);

            selectDriver.Location = new Point(490, topBorder);
            selectDriver.Size = cbxDefault;
            selectDriver.Text = "Select Driver";
            toolTip = new MyToolTip(selectDriver, "Select the driver to show details for");
            selectDriver.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            simulationPanel.Controls.Add(selectDriver);

            newLapAction.Location = new Point(490, topBorder * 2 + cbxDefault.Height);
            newLapAction.Size = cbxDefault;
            toolTip = new MyToolTip(newLapAction, "Select the action to occur at this lap");
            simulationPanel.Controls.Add(newLapAction);

            changeAction.Location = new Point(490, topBorder * 3 + cbxDefault.Height * 2);
            changeAction.Size = btnDefault;
            changeAction.Text = "Change Action";
            toolTip = new MyToolTip(changeAction, "Change the action that occurs at this lap");
            changeAction.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            changeAction.Click += ChangeAction_Click;
            simulationPanel.Controls.Add(changeAction);

            newLapTime.Location = new Point(490, topBorder * 4 + cbxDefault.Height * 2 + btnDefault.Height);
            newLapTime.Size = cbxDefault;
            toolTip = new MyToolTip(newLapTime, "Enter the desired lap time for this lap");
            newLapTime.KeyDown += NewLapTime_KeyDown;
            simulationPanel.Controls.Add(newLapTime);

            changeLap.Location = new Point(490, topBorder * 5 + cbxDefault.Height * 3 + btnDefault.Height);
            changeLap.Size = btnDefault;
            changeLap.Text = "Change Lap Time";
            toolTip = new MyToolTip(changeLap, "Change the lap time of this specific lap");
            changeLap.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            changeLap.Click += ChangeLap_Click;
            simulationPanel.Controls.Add(changeLap);

            updateSimulation.Location = new Point(490, topBorder * 6 + cbxDefault.Height * 3 + btnDefault.Height * 2);
            updateSimulation.Size = btnDefault;
            updateSimulation.Text = "Update";
            toolTip = new MyToolTip(updateSimulation, "Update and re-run simulation with the changes made");
            updateSimulation.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            updateSimulation.Click += UpdateSimulation_Click;
            simulationPanel.Controls.Add(updateSimulation);
        }

        private void InitialiseToolbar()
        {
            thisToolStripDropDown = new ToolStripDropDownButton("Strategy Data");

            save = new ToolStripButton("Save", Properties.Resources.Save);
            save.Click += save_Click;
            thisToolStripDropDown.DropDownItems.Add(save);

            load = new ToolStripButton("Load", Properties.Resources.Open);
            load.Click += load_Click;
            thisToolStripDropDown.DropDownItems.Add(load);
        }

        public void AddToolStrip()
        {
            PanelControlEvents.OnShowToolStrip(thisToolStripDropDown);
        }

        private void load_Click(object sender, EventArgs e)
        {
            if (selectRace.SelectedIndex >= 0 && selectRace.SelectedIndex < Data.NumberOfTracks)
                LoadSimulationFromFile(selectRace.SelectedIndex);
        }

        private void save_Click(object sender, EventArgs e)
        {
            simulation.SaveData();
        }

        private void LoadFromFile_Click(object sender, EventArgs e)
        {
            if (selectRace.SelectedIndex >= 0 && selectRace.SelectedIndex < Data.NumberOfTracks)
                LoadSimulationFromFile(selectRace.SelectedIndex);
        }

        private void LoadSimulationFromFile(int raceIndex)
        {
            raceLaps = Data.Tracks[raceIndex].laps;
            simulation = new RaceHistorySimulation(raceLaps, IOController.AssociatedForm);
            var graph = simulation.StartGraph();
            ((StratSimPanelControlEvents)IOController.PanelControlEvents).OnShowGraph(IOController.AssociatedForm);
            ((StratSimPanelControlEvents)IOController.PanelControlEvents).OnShowDriverSelectPanel(IOController.AssociatedForm);
        }

        private void LapList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int driverIndex = selectDriver.SelectedIndex;
            if (lapList.SelectedIndex >= 0 && lapList.SelectedIndex < simulation.raceLapCollection[driverIndex].Count)
            {
                float lapTime = simulation.raceLapCollection[driverIndex][lapList.SelectedIndex].LapTime;
                newLapTime.Text = lapTime.ToString();
            }
        }

        private void ActionsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int driverIndex = selectDriver.SelectedIndex;
            if (actionsList.SelectedIndex >= 0 && actionsList.SelectedIndex < simulation.raceLapCollection[driverIndex].Count)
            {
                RaceLapAction action = simulation.raceLapCollection[driverIndex][actionsList.SelectedIndex].LapAction;
                newLapAction.SelectedIndex = (int)action;
            }
        }

        private void UpdateSimulation_Click(object sender, EventArgs e)
        {
            if (dataModified)
            {
            }
        }

        private void NewLapTime_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                SetNewLapTime();
                if (e.Modifiers == Keys.Shift || e.Modifiers == Keys.Control)
                {
                    //Select the next lap in the list too
                    if (lapList.SelectedIndex < simulation.raceLapCollection[selectDriver.SelectedIndex].Count - 1)
                        lapList.SelectedIndex++;
                }
            }
        }

        private void ChangeLap_Click(object sender, EventArgs e)
        {
            SetNewLapTime();
        }

        private void SetNewLapTime()
        {
            int driverIndex = selectDriver.SelectedIndex;
            if (lapList.SelectedIndex >= 0 && lapList.SelectedIndex < simulation.raceLapCollection[driverIndex].Count)
            {
                int lapIndex = lapList.SelectedIndex;
                float lapTime;
                if (float.TryParse(newLapTime.Text, out lapTime))
                {
                    simulation.SetLapTime(driverIndex, lapIndex, lapTime);
                }
            }
            LoadSimulationData(simulation, selectDriver.SelectedIndex);
        }

        private void ChangeAction_Click(object sender, EventArgs e)
        {
            SetNewAction();
        }

        private void SetNewAction()
        {
            int driverIndex = selectDriver.SelectedIndex;
            if (actionsList.SelectedIndex >= 0 && actionsList.SelectedIndex < simulation.raceLapCollection[driverIndex].Count)
            {
                int lapIndex = actionsList.SelectedIndex;
                RaceLapAction newAction = (RaceLapAction)newLapAction.SelectedIndex;
                simulation.ChangeAction(driverIndex, lapIndex, newAction);
            }
            LoadSimulationData(simulation, selectDriver.SelectedIndex);
        }

        private void SelectDriver_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (simulation != null)
                LoadSimulationData(simulation, selectDriver.SelectedIndex);
        }

        private void StartSimulation_Click(object sender, EventArgs e)
        {
            if (selectRace.SelectedIndex >= 0 && selectRace.SelectedIndex < Data.NumberOfTracks)
                StartSimulation(selectRace.SelectedIndex);
        }

        private void SelectRace_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Set the number of laps in the race
            Data.RaceIndex = selectRace.SelectedIndex;
        }

        private void SelectYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            int year = Convert.ToInt32(selectYear.SelectedItem);
            Properties.Settings.Default.CurrentYear = year;
            //Re-initialise the tracks and drivers
            Program.SetupStaticClasses();

            //Re-populate tracks and drivers
            LoadTracksComboBox();
            LoadDriversComboBox();
        }

        private void StartSimulation(int raceIndex)
        {
            HistoryData data = new HistoryData();
            data.RetrieveArchiveData(Session.History, raceIndex);
            simulation = new RaceHistorySimulation(data.RaceLapCollection, raceIndex, IOController.AssociatedForm);

            //Draw the graph
            var graph = simulation.StartGraph();
            ((StratSimPanelControlEvents)IOController.PanelControlEvents).OnShowGraph(IOController.AssociatedForm);
            ((StratSimPanelControlEvents)IOController.PanelControlEvents).OnShowDriverSelectPanel(IOController.AssociatedForm);
        }

        private void LoadSimulationData(RaceHistorySimulation simulation, int driverIndex)
        {
            //Load the laps list box
            LoadLapListBox(simulation.raceLapCollection[driverIndex]);
            LoadStintBoxes(simulation, driverIndex);
            LoadActionDetails(simulation.raceLapCollection[driverIndex]);
        }

        private void LoadLapListBox(List<RaceHistoryLap> lapData)
        {
            string lapString;
            int lapDeficit;
            lapList.Items.Clear();
            foreach (var lap in lapData)
            {
                lapString = (lap.LapIndexInRace + 1).ToString() + '\t';
                lapDeficit = -(int)lap.LapDeficit;
                if (lap.LapDeficit == 0)
                    lapString += lap.LapTime.ToString();
                else
                    lapString += "+" + lapDeficit.ToString() + (lapDeficit == 1 ? " Lap" : " Laps");
                lapList.Items.Add(lapString);
            }
        }

        private void LoadStintBoxes(RaceHistorySimulation simulation, int driverIndex)
        {
            //Clear the current list of race stints
            foreach (Control control in stintPanels.Controls)
            {
                if (control is StintPanel)
                {
                    (control as StintPanel).StintLengthChanged -= StintPanel_StintLengthChanged;
                    (control as StintPanel).StintOrderChanged -= StintPanel_StintOrderChanged;
                    (control as StintPanel).TyreTypeChanged -= StintPanel_TyreTypeChanged;
                    stintPanels.Controls.Remove(control);
                }
            }
            stintPanels.Controls.Clear();

            //Add the race stints to the control
            var stints = simulation.GetRaceStints(driverIndex);
            StintPanel stintPanel;
            int stintLengthUpperBound;

            for (int stintIndex = 0; stintIndex < stints.Count; stintIndex++)
            {
                //Find the upper bound for the length of the stint: this is passed to the panel for validation
                if (stints.Count == 1)
                    stintLengthUpperBound = stints[stintIndex].StintLength;
                else if (stintIndex == stints.Count - 1) //if stint is last in strategy
                    stintLengthUpperBound = stints[stintIndex].StintLength + stints[stintIndex - 1].StintLength;
                else
                    stintLengthUpperBound = stints[stintIndex].StintLength + stints[stintIndex + 1].StintLength;

                stintPanel = new StintPanel(stintIndex, stints[stintIndex].StintLength, stints[stintIndex].TyreType, stints[stintIndex].TotalTime(), driverIndex, stintLengthUpperBound);
                stintPanel.StintLengthChanged += StintPanel_StintLengthChanged;
                stintPanel.StintOrderChanged += StintPanel_StintOrderChanged;
                stintPanel.TyreTypeChanged += StintPanel_TyreTypeChanged;
                stintPanel.Location = new Point(0, topBorder * (1 + stintIndex) + (80 * stintIndex));
                stintPanels.Controls.Add(stintPanel);
            }
        }

        private void StintPanel_TyreTypeChanged(object sender, TyreTypeChangedEventArgs e)
        {
            simulation.TyreTypeChanged(sender, e);
            LoadSimulationData(simulation, selectDriver.SelectedIndex);
        }

        private void StintPanel_StintOrderChanged(object sender, StintOperationEventArgs e)
        {
            simulation.StintOrderChanged(sender, e);
            LoadSimulationData(simulation, selectDriver.SelectedIndex);
        }

        private void StintPanel_StintLengthChanged(object sender, StintLengthChangedEventArgs e)
        {
            simulation.StintLengthChanged(sender, e);
            LoadSimulationData(simulation, selectDriver.SelectedIndex);
        }

        private void LoadActionDetails(List<RaceHistoryLap> lapData)
        {
            string actionString;
            actionsList.Items.Clear();
            foreach (var lap in lapData)
            {
                actionString = (lap.LapIndexInRace + 1).ToString() + '\t';
                actionString += lap.LapAction.ToString() + '\t';
                actionString += lap.LapStatus.ToString();
                actionsList.Items.Add(actionString);
            }
        }
    }
}
