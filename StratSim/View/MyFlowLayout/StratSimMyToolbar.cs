using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StratSim.View.UserControls;
using DataSources;
using StratSim.Model;
using StratSim.ViewModel;

using MyFlowLayout;

namespace StratSim.View.MyFlowLayout
{
    public class StratSimMyToolbar : MyToolbar
    {
        ToolStripButton VersionInfo;
        ToolStripButton Instructions;
        ToolStripDropDownButton QuickStart;
        ToolStripDropDownButton Parameters, Strategies;
        ToolStripButton Race, Archives;

        ToolStripDropDownButton OpenWindows;
        ToolStripButton Info, Settings, Main, ParameterViewer, StrategyViewer, ArchiveViewer, Graph, Axes, DriverViewer, RaceViewer, HistoryViewer;

        StratSimPanelControlEvents events;

        public StratSimMyToolbar(WindowFlowPanel Parent, StratSimPanelControlEvents Events)
            : base(Parent, Events)
        {
            this.events = Events;
            MyEvents.FinishedLoadingParameters += OnPaceParametersFinishedLoading;
            MyEvents.FinishedLoadingStrategies += OnStrategiesFinishedLoading;
        }

        public override void AddButtonsToToolBar()
        {
            base.AddButtonsToToolBar();
            //QuickStart
            QuickStart = new ToolStripDropDownButton();
            base.MenuBar.Items.Add(QuickStart);
            QuickStart.Text = "Startup";

            Parameters = new ToolStripDropDownButton("Parameters", Properties.Resources.Pace_Parameters);
            QuickStart.DropDownItems.Add(Parameters);
            ToolStripDropDownButton FromRaceData = new ToolStripDropDownButton("From Race Data");
            Parameters.DropDownItems.Add(FromRaceData);
            FromRaceData.AutoSize = true;
            for (int trackIndex = 0; trackIndex < Data.NumberOfTracks; trackIndex++)
            {
                RaceDropDown button = new RaceDropDown(trackIndex);
                FromRaceData.DropDownItems.Add(button);
                button.ButtonClicked += RaceIndexSelected;
            }
            ToolStripButton FromFile = new ToolStripButton("From File");
            Parameters.DropDownItems.Add(FromFile);
            FromFile.Click += FromFile_Click;

            Strategies = new ToolStripDropDownButton("Strategies", Properties.Resources.Strategies);
            Strategies.Visible = false;
            QuickStart.DropDownItems.Add(Strategies);
            ToolStripButton StrategiesFromFile = new ToolStripButton("From File");
            Strategies.DropDownItems.Add(StrategiesFromFile);
            StrategiesFromFile.Click += StrategiesFromFile_Click;
            ToolStripButton StrategiesFromData = new ToolStripButton("From Pace Data");
            Strategies.DropDownItems.Add(StrategiesFromData);
            StrategiesFromData.Click += StrategiesFromData_Click;

            Race = new ToolStripButton("Race", Properties.Resources.Race);
            Race.Visible = false;
            Race.Click += Race_Click;
            QuickStart.DropDownItems.Add(Race);

            Archives = new ToolStripButton("Archives", Properties.Resources.Archives);
            Archives.Click += ArchivesReady;
            QuickStart.DropDownItems.Add(Archives);

            ToolStripButton DataInput = new ToolStripButton("Input Data", Properties.Resources.Input);
            DataInput.Click += DataInput_Click;
            QuickStart.DropDownItems.Add(DataInput);
            //Finish Quickstart

            //Open Windows
            ToolStripButton tempButton;

            OpenWindows = new ToolStripDropDownButton();
            OpenWindows.Text = "Open Windows";
            base.MenuBar.Items.Add(OpenWindows);

            //Adds the buttons and event handlers for all panels that could be opened by the user
            tempButton = new ToolStripButton("Info", Properties.Resources.Info);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowInfoPanel(base.Form);
            Info = tempButton;
            OpenWindows.DropDownItems.Add(Info);
            tempButton = new ToolStripButton("Settings", Properties.Resources.Settings);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowSettingsPanel(base.Form);
            Settings = tempButton;
            OpenWindows.DropDownItems.Add(Settings);
            tempButton = new ToolStripButton("Main", Properties.Resources.Main);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowContentTabControl(base.Form);
            Main = tempButton;
            OpenWindows.DropDownItems.Add(Main);
            tempButton = new ToolStripButton("Data Input", Properties.Resources.Input);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowDataInput(base.Form);
            DataInput = tempButton;
            OpenWindows.DropDownItems.Add(DataInput);
            tempButton = new ToolStripButton("Parameter Viewer", Properties.Resources.Pace_Parameters);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowPaceParameters(base.Form);
            ParameterViewer = tempButton;
            OpenWindows.DropDownItems.Add(ParameterViewer);
            tempButton = new ToolStripButton("Strategy Viewer", Properties.Resources.Strategies);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowStrategies(base.Form);
            StrategyViewer = tempButton;
            OpenWindows.DropDownItems.Add(StrategyViewer);
            tempButton = new ToolStripButton("Archives", Properties.Resources.Archives);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowArchives(base.Form);
            ArchiveViewer = tempButton;
            OpenWindows.DropDownItems.Add(ArchiveViewer);
            tempButton = new ToolStripButton("Graph", Properties.Resources.Graph);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowGraph(base.Form);
            Graph = tempButton;
            OpenWindows.DropDownItems.Add(Graph);
            tempButton = new ToolStripButton("Axes", Properties.Resources.Axes);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowAxes(base.Form);
            Axes = tempButton;
            OpenWindows.DropDownItems.Add(Axes);
            tempButton = new ToolStripButton("Drivers", Properties.Resources.Driver_Select);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowDriverSelectPanel(base.Form);
            DriverViewer = tempButton;
            OpenWindows.DropDownItems.Add(DriverViewer);
            tempButton = new ToolStripButton("Race", Properties.Resources.Race);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowRacePanel(base.Form);
            RaceViewer = tempButton;
            OpenWindows.DropDownItems.Add(RaceViewer);
            tempButton = new ToolStripButton("Histories", Properties.Resources.Archives);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowRaceHistoryPanel(base.Form);
            HistoryViewer = tempButton;
            OpenWindows.DropDownItems.Add(HistoryViewer);
            //Finish Windows

            //Version info
            VersionInfo = new ToolStripButton();
            VersionInfo.Text = "Version";
            VersionInfo.Click += VersionInfo_Click;
            base.MenuBar.Items.Add(VersionInfo);
            //Finish version info

            //Instructions
            Instructions = new ToolStripButton();
            Instructions.Text = "Instructions";
            Instructions.Click += Instructions_Click;
            base.MenuBar.Items.Add(Instructions);
        }

        void Instructions_Click(object sender, EventArgs e)
        {
            StratSimPanelControlEvents.OnShowInstructions();
        }

        void OnPaceParametersFinishedLoading()
        {
            Strategies.Visible = true;
        }
        void OnStrategiesFinishedLoading()
        {
            Race.Visible = true;
        }

        void DataInput_Click(object sender, EventArgs e)
        {
            PanelControlEvents.OnShowDataInput(base.Form);
        }
        void VersionInfo_Click(object sender, EventArgs e)
        {
            StratSimPanelControlEvents.OnShowVersionInfo();
        }

        void ArchivesReady(object sender, EventArgs e)
        {
            PanelControlEvents.OnShowArchives(base.Form);
        }
        void FromFile_Click(object sender, EventArgs e)
        {
            PanelControlEvents.OnLoadPaceParametersFromFile();
            PanelControlEvents.OnShowPaceParameters(base.Form);
        }
        void RaceIndexSelected(int buttonIndex, Type buttonType)
        {
            Data.RaceIndex = buttonIndex;
            PanelControlEvents.OnLoadPaceParametersFromRace();
            PanelControlEvents.OnShowPaceParameters(base.Form);
        }
        void StrategiesFromFile_Click(object sender, EventArgs e)
        {
            PanelControlEvents.OnLoadStrategiesFromFile();
            PanelControlEvents.OnShowStrategies(base.Form);
        }
        void StrategiesFromData_Click(object sender, EventArgs e)
        {
            PanelControlEvents.OnLoadStrategiesFromData();
            PanelControlEvents.OnShowStrategies(base.Form);
        }

        void Race_Click(object sender, EventArgs e)
        {
            PanelControlEvents.OnShowRacePanel(base.Form);
        }

        public StratSimPanelControlEvents PanelControlEvents
        {
            get { return events; }
        }
    }
}
