using StratSim.Model;
using StratSim.Model.Files;
using StratSim.View.Forms;
using StratSim.View.MyFlowLayout;
using StratSim.View.Panels;
using StratSim.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MyFlowLayout;

namespace StratSim
{
    public static class Program
    {
        public static InfoPanel InfoPanel;
        public static SettingsPanel SettingsPanel;
        public static NewStartPanel NewStartPanel;
        public static DataInput DataInput;
        public static NewGraph Graph;
        public static StrategyViewer StrategyViewer;
        public static PaceParameters TimeAnalysis;
        public static DriverSelectPanel DriverSelectPanel;
        public static ContentTabControl ContentTabControl;
        public static AxesWindow AxesWindow;
        public static TimingArchives TimingArchives;
        public static RacePanel RacePanel;
        public static GridPanel GridPanel;
        public static RaceHistoryPanel RaceHistoryPanel;

        static Data myModel;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            SetupStaticClasses();
        }

        /// <summary>
        /// Starts the project and provides an IO controller for forms relating to this project.
        /// </summary>
        /// <returns>An instance of an IO controller used for forms for this project</returns>
        public static StratSimFormIOController StartProject()
        {
            Main();
            StratSimWindowFlowPanel MainPanel = new StratSimWindowFlowPanel();
            StratSimPanelControlEvents Events = new StratSimPanelControlEvents();
            StratSimMyToolbar Toolbar = new StratSimMyToolbar(MainPanel, Events);

            return new StratSimFormIOController(MainPanel, Toolbar, Events, "StratSim");
        }

        internal static void SetupStaticClasses()
        {
            myModel = new Data();
        }

        public static void ShowVersionInfo()
        {
            var VersionInformation = new VersionWindow();
            VersionInformation.Show();
        }
        public static void ShowInstructions()
        {
            var instructions = new Instructions();
            instructions.Show();
        }

        public static void Exit()
        {
            Application.Exit();
        }
    }
}
