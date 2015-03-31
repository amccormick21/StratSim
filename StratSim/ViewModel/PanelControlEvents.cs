
using StratSim.View.MyFlowLayout;

namespace StratSim.ViewModel
{
    /// <summary>
    /// Contains events which control hiding and showing of panels on a form
    /// </summary>
    class PanelControlEvents
    {
        ///<summary>Shows the version information window.</summary>
        public static void OnShowVersionInfo()
        {
            Program.ShowVersionInfo();
        }

        public delegate void LoadFromFileEventHandler();
        public static event LoadFromFileEventHandler LoadPaceParametersFromFile;
        public static event LoadFromFileEventHandler LoadStrategiesFromFile;
        public static event LoadFromFileEventHandler BeforeLoadStrategiesFromFile;
        public static event LoadFromFileEventHandler StrategiesLoaded;
        public delegate void LoadFromRaceEventHandler();
        public static event LoadFromRaceEventHandler LoadPaceParametersFromRace;
        public delegate void LoadFromDataEventHandler();
        public static event LoadFromDataEventHandler LoadStrategiesFromData;
        public static event LoadFromDataEventHandler BeforeLoadStrategiesFromData;
        public delegate void StartRaceEventHandler();
        public static event StartRaceEventHandler StartRaceFromStrategies;
        public static event StartRaceEventHandler BeforeStartRaceFromStrategies;
        public delegate void ViewPanelEventHandler(MainForm form);
        public static event ViewPanelEventHandler ShowInfoPanel;
        public static event ViewPanelEventHandler ShowSettingsPanel;
        public static event ViewPanelEventHandler ShowContentTabControl;
        public static event ViewPanelEventHandler ShowDriverSelectPanel;
        public static event ViewPanelEventHandler ShowGraph;
        public static event ViewPanelEventHandler ShowAxes;
        public static event ViewPanelEventHandler ShowPaceParameters;
        public static event ViewPanelEventHandler ShowStrategies;
        public static event ViewPanelEventHandler ShowArchives;
        public static event ViewPanelEventHandler ShowDataInput;
        public static event ViewPanelEventHandler RemoveGraphPanels;

        //The following routines fire events to show particular panels.
        public static void OnShowInfoPanel(MainForm form)
        {
            if (ShowInfoPanel != null)
                ShowInfoPanel(form);
        }
        public static void OnShowSettingsPanel(MainForm form)
        {
            if (ShowSettingsPanel != null)
                ShowSettingsPanel(form);
        }
        public static void OnShowContentTabControl(MainForm form)
        {
            if (ShowContentTabControl != null)
                ShowContentTabControl(form);
        }
        public static void OnShowDriverSelectPanel(MainForm form)
        {
            if (ShowDriverSelectPanel != null)
                ShowDriverSelectPanel(form);
        }
        public static void OnShowGraph(MainForm form)
        {
            if (ShowGraph != null)
                ShowGraph(form);
        }
        public static void OnShowAxes(MainForm form)
        {
            if (ShowAxes != null)
                ShowAxes(form);
        }
        public static void OnLoadPaceParametersFromFile()
        {
            if (LoadPaceParametersFromFile != null)
                LoadPaceParametersFromFile();
        }
        public static void OnLoadStrategiesFromFile()
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
        public static void OnLoadPaceParametersFromRace()
        {
            if (LoadPaceParametersFromRace != null)
                LoadPaceParametersFromRace();
        }
        public static void OnLoadStrategiesFromData()
        {
            if (BeforeLoadStrategiesFromData != null)
                BeforeLoadStrategiesFromData();
            if (LoadStrategiesFromData != null)
                LoadStrategiesFromData();
        }
        public static void OnStartRaceFromStrategies()
        {
            if (BeforeStartRaceFromStrategies != null)
                BeforeStartRaceFromStrategies();
            if (StartRaceFromStrategies != null)
                StartRaceFromStrategies();
        }
        public static void OnShowPaceParameters(MainForm form)
        {
            if (ShowPaceParameters != null)
                ShowPaceParameters(form);
        }
        public static void OnShowStrategies(MainForm form)
        {
            if (ShowStrategies != null)
                ShowStrategies(form);
        }
        public static void OnShowArchives(MainForm form)
        {
            if (ShowArchives != null)
                ShowArchives(form);
        }
        public static void OnShowDataInput(MainForm form)
        {
            if (ShowDataInput != null)
                ShowDataInput(form);
        }
        /// <summary>
        /// Fires the RemoveGraphPanels event, clearing the main graph panels from the screen to allow the screen to resize accoridingly.
        /// </summary>
        public static void OnRemoveGraphPanels(MainForm form)
        {
            if (RemoveGraphPanels != null)
                RemoveGraphPanels(form);
        }
    }
}
