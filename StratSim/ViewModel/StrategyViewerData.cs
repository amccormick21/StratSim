using DataSources;
using StratSim.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using StratSim.View.MyFlowLayout;

namespace StratSim.ViewModel
{
    /// <summary>
    /// Contains methods for manipulating, updating, reading and writing
    /// strategy data to and from file.
    /// </summary>
    class StrategyViewerData : IFileController<Strategy[]>, IDisposable
    {
        Strategy[] strategies = new Strategy[Data.NumberOfDrivers];
        Strategy[] fileData = new Strategy[Data.NumberOfDrivers];

        bool loadedFromFile;
        bool modified;

        SaveFileDialog SaveFileDialog;
        OpenFileDialog OpenFileDialog;

        /// <summary>
        /// Creates a new instance of the StrategyViewerData class.
        /// </summary>
        /// <param name="handlerOnLoad">An event handler to link the the DataLoaded event; it will be executed
        /// whenever the strategies are fully loaded</param>
        public StrategyViewerData(DataLoadedEventHandler handlerOnLoad)
        {
            modified = false;

            //Subscribes to the data loaded event so that data can be displayed when it has been loaded
            DataLoaded += handlerOnLoad;
            MyEvents.SettingsModified += MyEvents_SettingsModified;
            MyEvents.StrategyModified += MyEvents_StrategyModified;
        }

        public void LinkToEvents(StratSimPanelControlEvents Events)
        {
            Events.BeforeStartRaceFromStrategies += PanelControlEvents_StartRaceFromStrategies;
        }

        void PanelControlEvents_StartRaceFromStrategies()
        {
            if (modified)
            {
                string message = "Data has been modified and not updated. Save changes now?";
                string caption = "Save Changes";
                if (Functions.StartDialog(message, caption))
                {
                    SetDriverStrategies();
                }
            }
        }

        public void InitiailiseData(bool loadFromFile)
        {
            loadedFromFile = loadFromFile;

            if (loadFromFile)
            {
                LoadData();
            }
            else
            {
                PopulateDataFromDrivers();
            }
        }

        void MyEvents_StrategyModified(int driverIndex, Strategy strategy, bool showAllOnGraph)
        {
            modified = true;
            SetStrategy(driverIndex, strategy);
            MyEvents.OnStrategyModificationsComplete(showAllOnGraph, false, driverIndex);
        }
        void MyEvents_SettingsModified()
        {
            if (!loadedFromFile)
            {
                CheckUpdate();
            }
        }

        public void RefreshData()
        {
            RefreshData(loadedFromFile);
        }
        public void RefreshData(bool refreshFromFile)
        {
            if (refreshFromFile)
            {
                PopulateDataFromFile();
            }
            else
            {
                PopulateDataFromDrivers();
            }
            modified = false;
            MyEvents.OnStrategyModificationsComplete(true, true, 0);
        }

        void PopulateDataFromDrivers()
        {
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                strategies[driverIndex] = new Strategy(Data.Drivers[driverIndex].SelectedStrategy);
            }
            if (DataLoaded != null)
                DataLoaded();
        }
        void PopulateDataFromFile()
        {
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                strategies[driverIndex] = new Strategy(fileData[driverIndex]);
            }
            if (DataLoaded != null)
                DataLoaded();
        }

        /// <summary>
        /// Gets a specific strategy from the list of strategies
        /// </summary>
        /// <param name="driverIndex">The index at which the strategy is held</param>
        public Strategy GetStrategy(int driverIndex)
        {
            return new Strategy(strategies[driverIndex]);
        }
        /// <summary>
        /// Gets a specific stint from a specific strategy
        /// </summary>
        /// <param name="driverIndex">The index at which the strategy is held</param>
        /// <param name="stintIndex">The index of the stint within the strategy</param>
        public Stint GetStint(int driverIndex, int stintIndex)
        {
            return (Stint)strategies[driverIndex].Stints[stintIndex].Clone();
        }
        /// <summary>
        /// Sets a locally held strategy to the specified strategy
        /// </summary>
        /// <param name="driverIndex">The index of the strategy to overwrite</param>
        /// <param name="value">The new strategy to set as the locally held strategy</param>
        void SetStrategy(int driverIndex, Strategy value)
        {
            strategies[driverIndex] = (Strategy)value.Clone();
        }

        /// <summary>
        /// Sets all driver strategies (to be used in a race) to those held locally.
        /// </summary>
        public void SetDriverStrategies()
        {
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                Data.Drivers[driverIndex].SelectedStrategy = (Strategy)strategies[driverIndex].Clone();
            }
            modified = false;
        }

        public void SaveData()
        {
            SaveFileDialog = new SaveFileDialog();
            SaveFileDialog.InitialDirectory = Data.Settings.TimingDataBaseFolder;
            SaveFileDialog.Title = "Save File";
            SaveFileDialog.DefaultExt = ".csv";
            SaveFileDialog.FileOk += SaveFileDialog_FileOk;
            SaveFileDialog.ShowDialog();
        }
        void SaveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string file = SaveFileDialog.FileName;
            WriteToFile(file);
        }
        public void WriteToFile(string fileName)
        {
            //writes in database format.
            StreamWriter w = new StreamWriter(fileName);

            //Write headings:
            w.WriteLine("Driver,Start Lap,Length,Tyre");
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                foreach (Stint s in strategies[driverIndex].Stints)
                {
                    w.Write(Data.Drivers[driverIndex].DriverName);
                    w.Write(',');
                    s.WriteStintData(ref w);
                    w.WriteLine();
                }
            }

            w.Dispose();
        }

        public void LoadData()
        {
            OpenFileDialog = new OpenFileDialog();
            OpenFileDialog.InitialDirectory = Data.Settings.TimingDataBaseFolder;
            OpenFileDialog.Title = "Open File";
            OpenFileDialog.DefaultExt = ".csv";
            OpenFileDialog.FileOk += OpenFileDialog_FileOk;
            OpenFileDialog.ShowDialog();
        }
        void OpenFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            loadedFromFile = true;
            string file = OpenFileDialog.FileName;

            if (File.Exists(file))
            {
                try
                {
                    ReadFromFile(file);
                }
                catch (IOException exception)
                {
                    Console.Out.WriteLine("Error reading from {0}. Message: {1}", file, exception.Message);
                }
                finally
                {
                    PopulateDataFromFile();
                }
            }
        }
        public void ReadFromFile(string fileName)
        {
            int driverIndex = 0;
            int stintIndex = 0;

            int startLap, length;
            TyreType tyreType;
            string driverName;

            List<Stint> listOfStintsToAdd = new List<Stint>();
            Stint tempStint = new Stint();

            StreamReader s = new StreamReader(fileName);
            //read titles to remove.
            s.ReadLine();

            stintIndex = 0;
            listOfStintsToAdd.Clear();

            while (!s.EndOfStream)
            {

                string[] inputData = s.ReadLine().Split(',');
                driverName = inputData[0];
                startLap = int.Parse(inputData[1]);
                length = int.Parse(inputData[2]);
                if (inputData[3] == "Prime") { tyreType = TyreType.Prime; }
                else { tyreType = TyreType.Option; }

                if (driverName != Data.Drivers[driverIndex].DriverName)
                {
                    fileData[driverIndex] = new Strategy(listOfStintsToAdd);
                    driverIndex++;
                    stintIndex = 0;
                    listOfStintsToAdd.Clear();
                }

                tempStint = new Stint(startLap, tyreType, length);
                listOfStintsToAdd.Add(tempStint);

                stintIndex++;
            }
            fileData[driverIndex] = new Strategy(listOfStintsToAdd);
        }

        void CheckUpdate()
        {
            string message = "Settings have been altered. This may affect the way strategies are calculated." +
                "Recalculate strategies now?";
            string caption = "Recalculate Strateiges";
            if (Functions.StartDialog(message, caption))
            {
                Model.CalculationControllers.CalculationController.OptimiseAllStrategies(Data.RaceIndex);
                RefreshData(false);
            }
        }

        public void Dispose()
        {
            SaveFileDialog.Dispose();
            OpenFileDialog.Dispose();
        }

        public delegate void DataLoadedEventHandler();
        public event DataLoadedEventHandler DataLoaded;

        public Strategy[] FileData
        { get { return fileData; } }
    }
}
