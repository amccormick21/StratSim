using MyFlowLayout;
using StratSim.Model;
using StratSim.View.Panels;
using StratSim.ViewModel;
using System;
using System.Collections.Generic;

namespace StratSim.View.MyFlowLayout
{
    public class StratSimFormIOController : MainFormIOController, IDisposable
    {
        StratSimPanelControlEvents events;

        public StratSimFormIOController(StratSimWindowFlowPanel MainPanel, StratSimMyToolbar Toolbar, StratSimPanelControlEvents Events, string Title)
            : base(MainPanel, Toolbar, Events, Title)
        {
        }

        public override void SetAssociatedForm(MainForm Form)
        {
            base.SetAssociatedForm(Form);
            AssociatedForm.MyKeyPress += StratSimMainPanel.AlexMcCormickEasterEgg;
        }

        public override void SetupControls()
        {
            base.SetupControls();
            InitialiseControls();
        }

        public override void SetEvents(PanelControlEvents events)
        {
            base.SetEvents(events);
            this.events = (StratSimPanelControlEvents)events;
        }

        public override void UnsubscribeFromEvents()
        {
            base.UnsubscribeFromEvents();
            events.LoadPaceParametersFromFile -= PanelControlEvents_LoadPaceParametersFromFile;
            events.LoadStrategiesFromFile -= PanelControlEvents_LoadStrategiesFromFile;
            events.LoadPaceParametersFromRace -= PanelControlEvents_LoadPaceParametersFromRace;
            events.LoadStrategiesFromData -= PanelControlEvents_LoadStrategiesFromData;
            events.StartRaceFromStrategies -= PanelControlEvents_StartRaceFromStrategies;
            events.StartRacePanel -= PanelControlEvents_StartRacePanel;
            events.ShowInfoPanel -= PanelControlEvents_ShowInfoPanel;
            events.ShowSettingsPanel -= PanelControlEvents_ShowSettingsPanel;
            events.ShowDriverSelectPanel -= PanelControlEvents_ShowDriverSelectPanel;
            events.ShowGraph -= PanelControlEvents_ShowGraph;
            events.ShowAxes -= PanelControlEvents_ShowAxes;
            events.ShowPaceParameters -= PanelControlEvents_ShowPaceParameters;
            events.ShowStrategies -= PanelControlEvents_ShowStrategies;
            events.ShowArchives -= PanelControlEvents_ShowArchives;
            events.ShowDataInput -= PanelControlEvents_ShowDataInput;
            events.StartGraph -= StartGraph;
            events.RemoveGraphPanels -= PanelControlEvents_RemoveGraphPanels;
            events.ShowRacePanel -= PanelControlEvents_ShowRacePanel;
            events.ShowGridPanel -= events_ShowGridPanel;
            events.StartGridPanel -= events_StartGridPanel;
            events.ShowRaceHistoryPanel -= events_ShowRaceHistoryPanel;
        }

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();
            events.LoadPaceParametersFromFile += PanelControlEvents_LoadPaceParametersFromFile;
            events.StrategiesLoaded += PanelControlEvents_LoadStrategiesFromFile;
            events.LoadPaceParametersFromRace += PanelControlEvents_LoadPaceParametersFromRace;
            events.LoadStrategiesFromData += PanelControlEvents_LoadStrategiesFromData;
            events.StartRaceFromStrategies += PanelControlEvents_StartRaceFromStrategies;
            events.StartRacePanel += PanelControlEvents_StartRacePanel;
            events.ShowInfoPanel += PanelControlEvents_ShowInfoPanel;
            events.ShowSettingsPanel += PanelControlEvents_ShowSettingsPanel;
            events.ShowDriverSelectPanel += PanelControlEvents_ShowDriverSelectPanel;
            events.ShowGraph += PanelControlEvents_ShowGraph;
            events.ShowAxes += PanelControlEvents_ShowAxes;
            events.ShowPaceParameters += PanelControlEvents_ShowPaceParameters;
            events.ShowStrategies += PanelControlEvents_ShowStrategies;
            events.ShowArchives += PanelControlEvents_ShowArchives;
            events.ShowDataInput += PanelControlEvents_ShowDataInput;
            events.StartGraph += StartGraph;
            events.RemoveGraphPanels += PanelControlEvents_RemoveGraphPanels;
            events.ShowRacePanel += PanelControlEvents_ShowRacePanel;
            events.ShowGridPanel += events_ShowGridPanel;
            events.StartGridPanel += events_StartGridPanel;
            events.ShowRaceHistoryPanel += events_ShowRaceHistoryPanel;
        }

        private void events_ShowRaceHistoryPanel(MainForm callingForm)
        {
            if (callingForm == base.AssociatedForm)
            {
                RemoveGraphPanels();
                StartHistoryPanel();
                ShowDriverList(true);
                ShowGraph();
                FinishedAdding();
            }
        }

        void events_StartGridPanel(int[] gridOrder)
        {
            StartGridPanel(gridOrder);
        }

        void events_ShowGridPanel(MainForm callingForm)
        {
            if (callingForm == base.AssociatedForm)
            {
                ShowGridPanel();
                FinishedAdding();
            }
        }

        void PanelControlEvents_StartRacePanel(int[] gridOrder)
        {
            StartRacePanel(gridOrder);
        }

        void PanelControlEvents_ShowRacePanel(MainForm callingForm)
        {
            if (callingForm == base.AssociatedForm)
            {
                ShowRacePanel();
                FinishedAdding();
            }
        }

        void PanelControlEvents_RemoveGraphPanels(MainForm callingForm)
        {
            if (callingForm == base.AssociatedForm)
            {
                RemoveGraphPanels();
            }
        }
        void PanelControlEvents_ShowDataInput(MainForm callingForm)
        {
            if (callingForm == base.AssociatedForm)
            {
                StartDataInput();
            }
        }
        void PanelControlEvents_ShowArchives(MainForm callingForm)
        {
            if (callingForm == base.AssociatedForm)
            {
                StartTimingArchives();
            }
        }
        void PanelControlEvents_ShowStrategies(MainForm callingForm)
        {
            //Shows all controls required for the strategies to be displayed
            if (callingForm == base.AssociatedForm)
            {
                RemoveGraphPanels();
                ShowStrategyViewer();
                ShowDriverList(false);
                ShowGraph();
                FinishedAdding();
            }
        }
        void PanelControlEvents_ShowPaceParameters(MainForm callingForm)
        {
            if (callingForm == base.AssociatedForm)
            {
                ShowTimingDataPanel();
                FinishedAdding();
            }
        }
        void PanelControlEvents_ShowAxes(MainForm callingForm)
        {
            if (callingForm == base.AssociatedForm)
            {
                ShowAxes();
                FinishedAdding();
            }
        }
        void PanelControlEvents_ShowGraph(MainForm callingForm)
        {
            if (callingForm == base.AssociatedForm)
            {
                ShowGraph();
                FinishedAdding();
            }
        }
        void PanelControlEvents_ShowDriverSelectPanel(MainForm callingForm)
        {
            if (callingForm == base.AssociatedForm)
            {
                ShowDriverList();
                FinishedAdding();
            }
        }

        void PanelControlEvents_ShowSettingsPanel(MainForm callingForm)
        {
            if (callingForm == base.AssociatedForm)
            {
                ShowSettingsPanel();
                FinishedAdding();
            }
        }
        void PanelControlEvents_ShowInfoPanel(MainForm callingForm)
        {
            if (callingForm == base.AssociatedForm)
            {
                ShowInfoPanel();
                FinishedAdding();
            }
        }
        void PanelControlEvents_StartRaceFromStrategies()
        {
            StartRaceSimulation();
            ShowDriverList(true);
            ShowGraph();
            FinishedAdding();
        }
        void PanelControlEvents_LoadStrategiesFromData()
        {
            StartStrategyProcessing(false);
        }
        void PanelControlEvents_LoadPaceParametersFromRace()
        {
            StartTimingDataPanel(false);
        }
        void PanelControlEvents_LoadStrategiesFromFile()
        {
            StartStrategyProcessing(true);
        }
        void PanelControlEvents_LoadPaceParametersFromFile()
        {
            StartTimingDataPanel(true);
        }

        public override MainFormIOController GetNew()
        {
            StratSimWindowFlowPanel MainPanel = new StratSimWindowFlowPanel();
            StratSimPanelControlEvents Events = new StratSimPanelControlEvents();
            StratSimMyToolbar Toolbar = new StratSimMyToolbar(MainPanel, Events);
            return new StratSimFormIOController(MainPanel, Toolbar, Events, "StratSim");
        }

        void InitialiseControls()
        {
            Program.InfoPanel = new InfoPanel(base.AssociatedForm);
            Program.SettingsPanel = new SettingsPanel(base.AssociatedForm);
        }

        public void ShowStartPanel()
        {
            AddPanel(base.ContentTabControl);
            Program.NewStartPanel = new NewStartPanel(base.AssociatedForm);
            AddContentPanel(Program.NewStartPanel);
            FinishedAdding();
        }

        void StartDataInput()
        {
            Program.DataInput = new DataInput(base.AssociatedForm);
            AddContentPanel(Program.DataInput);
        }
        void StartTimingDataPanel(bool loadFromFile)
        {
            Model.CalculationControllers.CalculationController.PopulateDriverDataFromFiles(Data.RaceIndex);
            Model.CalculationControllers.CalculationController.CalculatePaceParameters();
            if (Program.TimeAnalysis != null)
            {
                RemoveContentPanel(Program.TimeAnalysis);

                //If the panel has been started and not initialised (i.e. data not loaded):
                if (Program.TimeAnalysis.Removed && Program.TimeAnalysis.IsInitialised)
                    Program.TimeAnalysis.OnPanelClosed(AssociatedForm);
            }

            Program.TimeAnalysis = new PaceParameters(base.AssociatedForm, loadFromFile);
            MyEvents.OnFinishedLoadingParameters();
        }
        void StartTimingArchives()
        {
            Program.TimingArchives = new TimingArchives(base.AssociatedForm);
            AddContentPanel(Program.TimingArchives);
        }
        void StartStrategyProcessing(bool loadFromFile)
        {
            Model.CalculationControllers.CalculationController.OptimiseAllStrategies(Data.RaceIndex);
            StartStrategyPanel(loadFromFile);
            int[] gridOrder = new int[Data.NumberOfDrivers];
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                if (Data.Drivers[driverIndex].PracticeTimes.GridPosition != -1)
                {
                    gridOrder[Data.Drivers[driverIndex].PracticeTimes.GridPosition] = driverIndex;
                }
            }

            StartRacePanel(gridOrder);
        }
        void StartStrategyPanel(bool loadFromFile)
        {
            if (Program.DriverSelectPanel == null)
                StartDriverList();

            if (Program.StrategyViewer != null)
            {
                RemoveContentPanel(Program.StrategyViewer);
                RemoveGraphPanels();

                //If the panel has been started and not initialised (i.e. data not loaded):
                if (Program.StrategyViewer.Removed && Program.StrategyViewer.IsInitialised)
                    Program.StrategyViewer.OnPanelClosed(AssociatedForm);
            }

            Program.StrategyViewer = new StrategyViewer(base.AssociatedForm, loadFromFile);
            Program.StrategyViewer.SetDriverPanel(Program.DriverSelectPanel);
        }
        public void StartGraph(NewGraph graph)
        {
            Program.AxesWindow = new AxesWindow(base.AssociatedForm);
            Program.Graph = graph;
            LinkGraphToDrivers();
        }
        void StartDriverList()
        {
            Program.DriverSelectPanel = new DriverSelectPanel(base.AssociatedForm);
            LinkGraphToDrivers();
        }
        private void StartRacePanel(int[] gridOrder)
        {
            Program.RacePanel = new RacePanel(base.AssociatedForm, gridOrder);
        }
        private void StartGridPanel(int[] gridOrder)
        {
            Program.GridPanel = new GridPanel(base.AssociatedForm, gridOrder);
        }
        void ShowInfoPanel()
        {
            AddPanel(Program.InfoPanel);
        }
        void ShowSettingsPanel()
        {
            AddPanel(Program.SettingsPanel);
        }

        void ShowTimingDataPanel()
        {
            if (Program.TimeAnalysis != null)
            {
                RemoveContentPanel(Program.TimeAnalysis);
                AddContentPanel(Program.TimeAnalysis);
                Program.TimeAnalysis.AddToolStrip();
            }
        }
        void ShowStrategyViewer()
        {
            if (Program.StrategyViewer != null)
            {
                RemoveContentPanel(Program.StrategyViewer);
                AddContentPanel(Program.StrategyViewer);
                Program.StrategyViewer.AddToolStrip();
                RemoveGraphPanels();
                StartGraph(Program.StrategyViewer.GetGraph());
                ShowGraph();
            }
        }
        void ShowDriverList(bool showTimeGaps)
        {
            if (Program.DriverSelectPanel != null)
            {
                RemovePanel(Program.DriverSelectPanel);
                AddPanel(Program.DriverSelectPanel);
                Program.DriverSelectPanel.ShowTimeGaps = showTimeGaps;
            }
        }
        void ShowDriverList()
        {
            if (Program.DriverSelectPanel != null)
            {
                RemovePanel(Program.DriverSelectPanel);
                AddPanel(Program.DriverSelectPanel);
            }
        }
        void ShowGraph()
        {
            if (Program.Graph != null)
            {
                AddPanel(Program.Graph);
            }
        }
        void ShowAxes()
        {
            if (Program.AxesWindow != null)
            {
                AddPanel(Program.AxesWindow);
            }
        }


        void ShowRacePanel()
        {
            if (Program.RacePanel != null)
            {
                AddPanel(Program.RacePanel);
            }
        }

        void ShowGridPanel()
        {
            if (Program.GridPanel != null)
            {
                AddPanel(Program.GridPanel);
            }
        }

        private void StartHistoryPanel()
        {
            Program.RaceHistoryPanel = new RaceHistoryPanel(base.AssociatedForm);
            AddContentPanel(Program.RaceHistoryPanel);
            Program.RaceHistoryPanel.AddToolStrip();
        }

        void StartRaceSimulation()
        {
            //must remove in reverse order of panel index
            if (Program.StrategyViewer.PanelIndex > Program.TimeAnalysis.PanelIndex)
            {
                if (Program.StrategyViewer != null)
                    RemoveContentPanel(Program.StrategyViewer);
                if (Program.TimeAnalysis != null)
                    RemoveContentPanel(Program.TimeAnalysis);
            }
            else
            {
                if (Program.TimeAnalysis != null)
                    RemoveContentPanel(Program.TimeAnalysis);
                if (Program.StrategyViewer != null)
                    RemoveContentPanel(Program.StrategyViewer);
            }

            RemoveGraphPanels();

            //Setup the strategies for the race
            RaceStrategy[] Strategies = new RaceStrategy[Data.NumberOfDrivers];
            int driverIndex = 0;
            foreach (Driver d in Data.Drivers)
            {
                Strategies[driverIndex++] = new RaceStrategy(d.SelectedStrategy, d.PracticeTimes.GridPosition);
            }

            StartDriverList();
            Program.DriverSelectPanel.ShowTimeGaps = true;

            //Start the race
            if (Data.Race == null)
            {
                Data.Race = new Race(Data.RaceIndex, Strategies, base.AssociatedForm);
                Data.Race.SetupGrid();
                Data.Race.SimulateRace();
            }
            else
            {
                NewGraph newGraph;
                Data.Race.RestartSimulation(out newGraph, Strategies);
                StartGraph(newGraph);
            }
        }
        void RemoveGraphPanels()
        {
            if (Program.Graph != null)
                RemovePanel(Program.Graph);
            if (Program.DriverSelectPanel != null)
                RemovePanel(Program.DriverSelectPanel);
            if (Program.AxesWindow != null)
                RemovePanel(Program.AxesWindow);
        }
        void LinkGraphToDrivers()
        {
            if (Program.Graph != null && Program.DriverSelectPanel != null)
            {
                Program.DriverSelectPanel.SetGraph(Program.Graph);
                Program.Graph.SetDriverPanel(Program.DriverSelectPanel);
            }
            if (Program.StrategyViewer != null && Program.DriverSelectPanel != null)
            {
                Program.StrategyViewer.SetDriverPanel(Program.DriverSelectPanel);
            }
            if (Program.StrategyViewer != null && Program.Graph != null)
            {
                Program.StrategyViewer.SetGraph(Program.Graph);
            }
        }

        public StratSimMyToolbar StratSimToolbar
        { get { return (StratSimMyToolbar)base.Toolbar; } }

        public StratSimWindowFlowPanel StratSimMainPanel
        { get { return (StratSimWindowFlowPanel)base.MainPanel; } }

        public StratSimPanelControlEvents StratSimEvents
        { get { return this.events; } }
    }
}
