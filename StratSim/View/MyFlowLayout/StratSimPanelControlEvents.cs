using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyFlowLayout;
using StratSim.View.Panels;

namespace StratSim.View.MyFlowLayout
{
    public class StratSimPanelControlEvents : PanelControlEvents
    {
        ///<summary>Shows the version information window.</summary>
        public static void OnShowVersionInfo()
        {
            Program.ShowVersionInfo();
        }
        public static void OnShowInstructions()
        {
            Program.ShowInstructions();
        }

        public delegate void LoadFromFileEventHandler();
        public event LoadFromFileEventHandler LoadPaceParametersFromFile;
        public event LoadFromFileEventHandler LoadStrategiesFromFile;
        public event LoadFromFileEventHandler BeforeLoadStrategiesFromFile;
        public event LoadFromFileEventHandler StrategiesLoaded;
        public delegate void LoadFromRaceEventHandler();
        public event LoadFromRaceEventHandler LoadPaceParametersFromRace;
        public delegate void LoadFromDataEventHandler();
        public event LoadFromDataEventHandler LoadStrategiesFromData;
        public event LoadFromDataEventHandler BeforeLoadStrategiesFromData;
        public delegate void StartRaceEventHandler();
        public delegate void StartGridPanelEventHander(int[] positionsByDriverIndex);
        public event StartGridPanelEventHander StartRacePanel;
        public event StartGridPanelEventHander StartGridPanel;
        public event StartRaceEventHandler StartRaceFromStrategies;
        public event StartRaceEventHandler BeforeStartRaceFromStrategies;
        public delegate void StartGraphEventHandler(NewGraph Graph);
        public event StartGraphEventHandler StartGraph;
        public event ViewPanelEventHandler ShowInfoPanel;
        public event ViewPanelEventHandler ShowSettingsPanel;
        public event ViewPanelEventHandler ShowDriverSelectPanel;
        public event ViewPanelEventHandler ShowGraph;
        public event ViewPanelEventHandler ShowAxes;
        public event ViewPanelEventHandler ShowPaceParameters;
        public event ViewPanelEventHandler ShowStrategies;
        public event ViewPanelEventHandler ShowArchives;
        public event ViewPanelEventHandler ShowDataInput;
        public event ViewPanelEventHandler RemoveGraphPanels;
        public event ViewPanelEventHandler ShowRacePanel;
        public event ViewPanelEventHandler ShowGridPanel;
        public event ViewPanelEventHandler ShowRaceHistoryPanel;

        //The following routines fire events to show particular panels.
        public void OnShowInfoPanel(MainForm form)
        {
            if (ShowInfoPanel != null)
                ShowInfoPanel(form);
        }
        public void OnShowSettingsPanel(MainForm form)
        {
            if (ShowSettingsPanel != null)
                ShowSettingsPanel(form);
        }
        public void OnShowDriverSelectPanel(MainForm form)
        {
            if (ShowDriverSelectPanel != null)
                ShowDriverSelectPanel(form);
        }
        public void OnShowGraph(MainForm form)
        {
            if (ShowGraph != null)
                ShowGraph(form);
        }
        public void OnShowAxes(MainForm form)
        {
            if (ShowAxes != null)
                ShowAxes(form);
        }
        public void OnLoadPaceParametersFromFile()
        {
            if (LoadPaceParametersFromFile != null)
                LoadPaceParametersFromFile();
        }
        public void OnLoadStrategiesFromFile()
        {
            //The three stages to the event allow different stages of processing to be completed in the correct order.
            if (BeforeLoadStrategiesFromFile != null)
                BeforeLoadStrategiesFromFile();
            if (LoadStrategiesFromFile != null)
                LoadStrategiesFromFile();
            if (StrategiesLoaded != null)
                StrategiesLoaded();
        }

        //Loads pace parameters and strategies into main memory.
        public void OnLoadPaceParametersFromRace()
        {
            if (LoadPaceParametersFromRace != null)
                LoadPaceParametersFromRace();
        }
        public void OnLoadStrategiesFromData()
        {
            if (BeforeLoadStrategiesFromData != null)
                BeforeLoadStrategiesFromData();
            if (LoadStrategiesFromData != null)
                LoadStrategiesFromData();
        }
        public void OnStartRaceFromStrategies()
        {
            if (BeforeStartRaceFromStrategies != null)
                BeforeStartRaceFromStrategies();
            if (StartRaceFromStrategies != null)
                StartRaceFromStrategies();
        }
        public void OnShowPaceParameters(MainForm form)
        {
            if (ShowPaceParameters != null)
                ShowPaceParameters(form);
        }
        public void OnShowStrategies(MainForm form)
        {
            if (ShowStrategies != null)
                ShowStrategies(form);
        }
        public void OnShowArchives(MainForm form)
        {
            if (ShowArchives != null)
                ShowArchives(form);
        }
        public void OnShowDataInput(MainForm form)
        {
            if (ShowDataInput != null)
                ShowDataInput(form);
        }
        public void OnStartGraph(NewGraph Graph)
        {
            if (StartGraph != null)
                StartGraph(Graph);
        }
        /// <summary>
        /// Fires the RemoveGraphPanels event, clearing the main graph panels from the screen to allow the screen to resize accoridingly.
        /// </summary>
        public void OnRemoveGraphPanels(MainForm form)
        {
            if (RemoveGraphPanels != null)
                RemoveGraphPanels(form);
        }

        public void OnStartRacePanel(MainForm form, int[] gridOrder)
        {
            if (StartRacePanel != null)
                StartRacePanel(gridOrder);
        }

        public void OnStartGridPanel(MainForm form, int[] gridOrder)
        {
            if (StartGridPanel != null)
                StartGridPanel(gridOrder);
        }

        public void OnShowRacePanel(MainForm form)
        {
            if (ShowRacePanel != null)
                ShowRacePanel(form);
        }

        public void OnShowGridPanel(MainForm form)
        {
            if (ShowGridPanel != null)
                ShowGridPanel(form);
        }
        
        public void OnShowRaceHistoryPanel(MainForm form)
        {
            if (ShowRaceHistoryPanel != null)
                ShowRaceHistoryPanel(form);
        }
    }
}
