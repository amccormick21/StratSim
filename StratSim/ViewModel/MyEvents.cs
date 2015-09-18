using Graphing;
using StratSim.Model;
using StratSim.View.Panels;
using System.Windows.Forms;

namespace StratSim.ViewModel
{
    /// <summary>
    /// Class contianing generic events for the program
    /// </summary>
    static class MyEvents
    {
        /// <summary>
        /// Fires the SettingsModified event
        /// </summary>
        public static void OnSettingsModified()
        {
            if (SettingsModified != null)
                SettingsModified();
        }

        public delegate void SettingsModifiedEventHandler();
        public static event SettingsModifiedEventHandler SettingsModified;

        /// <summary>
        /// Fires the FinishedLoadingParameters event
        /// </summary>
        public static void OnFinishedLoadingParameters()
        {
            if (FinishedLoadingParameters != null)
                FinishedLoadingParameters();
        }
        /// <summary>
        /// Fires the FinishedLoadingStrategies event
        /// </summary>
        public static void OnFinishedLoadingStrategies()
        {
            if (FinishedLoadingStrategies != null)
                FinishedLoadingStrategies();
        }

        public delegate void FinishedLoadingEventHandler();
        public static FinishedLoadingEventHandler FinishedLoadingParameters;
        public static FinishedLoadingEventHandler FinishedLoadingStrategies;

        /// <summary>
        /// Fires the strategy modified event after a strategy has been modified
        /// </summary>
        public static void OnStrategyModified(Driver driver, Strategy strategy, bool showAllOnGraph)
        {
            if (StrategyModified != null)
                StrategyModified(driver.DriverIndex, strategy, showAllOnGraph);
        }

        /// <summary>
        /// Fires the strategy modifications complete event when all changes to a strategy have been implemented
        /// </summary>
        public static void OnStrategyModificationsComplete(bool showAllOnGraph, bool changeNormalisedDriver, int driverIndex)
        {
            if (StrategyModificationsComplete != null)
                StrategyModificationsComplete(showAllOnGraph, changeNormalisedDriver, driverIndex);
        }

        public delegate void StrategyModificationsCompleteEventHandler(bool showAllOnGraph, bool changeNormalisedDriver, int driverIndex);
        public static event StrategyModificationsCompleteEventHandler StrategyModificationsComplete;

        public delegate void StrategyModifiedEventHandler(int driverIndex, Strategy strategy, bool showAllOnGraph);
        public static event StrategyModifiedEventHandler StrategyModified;

        /// <summary>
        /// Fires the AxesChangedByUser event. This represents the axes panel being modified.
        /// </summary>
        /// <param name="horizontalAxis">The new horizontal axis</param>
        /// <param name="verticalAxis">The new vertical axis</param>
        /// <param name="normalisation">The new graph normalisation type</param>
        public static void OnAxesChangedByUser(AxisParameters horizontalAxis, AxisParameters verticalAxis, NormalisationType normalisation)
        {
            if (AxesChangedByUser != null)
                AxesChangedByUser(horizontalAxis, verticalAxis, normalisation);
        }
        public delegate void AxesModifiedEventHandler(AxisParameters horizontalAxis, AxisParameters verticalAxis, NormalisationType normalisation);
        public static event AxesModifiedEventHandler AxesChangedByUser;

        /// <summary>
        /// Fires the AxesChangedByComputer event. This represents any scaling or setting which does not involve the axes panel.
        /// </summary>
        /// <param name="horizontalAxis">The new horizontal axis</param>
        /// <param name="verticalAxis">The new vertical axis</param>
        /// <param name="normalisation">The new graph normalisation type</param>
        public static void OnAxesComputerGenerated(AxisParameters horizontalAxis, AxisParameters verticalAxis, NormalisationType normalisation)
        {
            if (AxesChangedByComputer != null)
                AxesChangedByComputer(horizontalAxis, verticalAxis, normalisation);
        }
        public static event AxesModifiedEventHandler AxesChangedByComputer;
    }
}
