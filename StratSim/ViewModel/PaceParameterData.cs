using DataSources;
using StratSim.Model;
using StratSim.View.UserControls;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using StratSim.ViewModel;
using StratSim.View.MyFlowLayout;

namespace StratSim.ViewModel
{
    /// <summary>
    /// Contains methods for manipulating, updating, reading and writing
    /// pace parameter data to and from file.
    /// </summary>
    class PaceParameterData : IFileController<float[,]>, IDisposable
    {
        string[,] labelData = new string[3, Data.NumberOfDrivers];
        float[,] paceData = new float[8, Data.NumberOfDrivers];

        float[,] fileData = new float[8, Data.NumberOfDrivers];

        bool loadedFromFile;
        bool modified;

        SaveFileDialog SaveFileDialog;
        OpenFileDialog OpenFileDialog;

        public PaceParameterData(DataLoadedEventHandler handlerOnLoad)
        {
            modified = false;
            DataLoaded += handlerOnLoad;
            MyEvents.SettingsModified += MyEvents_SettingsModified;
        }

        public void LinkToEvents(StratSimPanelControlEvents Events)
        {
            Events.BeforeLoadStrategiesFromData += NotifyIfModified;
            Events.BeforeLoadStrategiesFromFile += NotifyIfModified;
        }

        /// <summary>
        /// If data has been modified and not updated, displays a warning message warning the user of this
        /// before the data is used in processing.
        /// </summary>
        void NotifyIfModified()
        {
            if (modified)
            {
                string message = "Data has been modified and not updated. Save changes now?";
                string caption = "Save Changes";
                if (Functions.StartDialog(message, caption))
                {
                    SetDriverPaceData();
                }
            }
        }

        public void InitialiseData(bool loadFromFile)
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

        void MyEvents_SettingsModified()
        {
            if (!loadedFromFile)
            {
                CheckUpdate();
            }
        }

        public void RefreshData()
        {
            if (loadedFromFile)
            {
                PopulateDataFromFile();
            }
            else
            {
                PopulateDataFromDrivers();
            }
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
        }

        /// <summary>
        /// Populates the locally stored data from the driver data held within the program.
        /// </summary>
        void PopulateDataFromDrivers()
        {
            int driverIndex = 0;

            foreach (Driver d in Data.Drivers)
            {
                labelData[0, driverIndex] = Convert.ToString(d.DriverNumber);
                labelData[1, driverIndex] = Convert.ToString(d.Team);
                labelData[2, driverIndex] = d.DriverName;
                for (int dataIndex = 0; dataIndex < 8; ++dataIndex)
                {
                    paceData[dataIndex, driverIndex] = d.PaceParameters.PaceParameters[(PaceParameterType)dataIndex];
                }

                driverIndex++;
            }
            if (DataLoaded != null)
                DataLoaded();
        }
        /// <summary>
        /// Populates driver pace data from a csv file selected by the user.
        /// </summary>
        void PopulateDataFromFile()
        {
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                //Set the label data as a string
                labelData[0, driverIndex] = Convert.ToString(Data.Drivers[driverIndex].DriverNumber);
                labelData[1, driverIndex] = Data.Drivers[driverIndex].Team;
                labelData[2, driverIndex] = Data.Drivers[driverIndex].DriverName;

                //Set the pace data array
                for (int j = 0; j < 8; j++)
                {
                    paceData[j, driverIndex] = fileData[j, driverIndex];
                }
            }
            if (DataLoaded != null)
                DataLoaded();
        }

        /// <summary>
        /// Sets the driver pace data to the data currently held in the buffer.
        /// </summary>
        public void SetDriverPaceData()
        {
            int driverIndex = 0;
            foreach (Driver d in Data.Drivers)
            {
                for (int dataIndex = 0; dataIndex < 8; ++dataIndex)
                {
                    d.PaceParameters.PaceParameters[(PaceParameterType)dataIndex] = paceData[dataIndex, driverIndex];
                }

                driverIndex++;
            }
            modified = false;
        }
        /// <summary>
        /// Sets one item of pace data to the specified value.
        /// </summary>
        void SetPaceData(int dataIndex, int driverIndex, float value)
        {
            paceData[dataIndex, driverIndex] = value;
            modified = true;
        }

        public float GetPaceData(int dataIndex, int driverIndex)
        {
            return paceData[dataIndex, driverIndex];
        }
        public string GetLabelData(int parameterIndex, int driverIndex)
        {
            return labelData[parameterIndex, driverIndex];
        }

        public string GetData(int parameterIndex, int driverIndex)
        {
            if (parameterIndex <= 2)
            {
                return GetLabelData(parameterIndex, driverIndex);
            }
            else
            {
                if (parameterIndex > 2 && parameterIndex <= 10)
                {
                    return Convert.ToString(GetPaceData(parameterIndex - 3, driverIndex));
                }
                else
                {
                    return "";
                }
            }
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
            StreamWriter s = new StreamWriter(fileName);
            s.WriteLine("Driver,Speed,Delta,PrimeDeg,OptionDeg,Pace,FuelEffect,FuelConsumption,FuelLoad");
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                s.Write(Data.Drivers[driverIndex].DriverName);
                for (int dataIndex = 0; dataIndex < 8; dataIndex++)
                {
                    s.Write(", ");
                    s.Write(paceData[dataIndex, driverIndex]);
                }
                s.WriteLine();
            }

            s.Dispose();
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
            ReadFromFile(file);
            PopulateDataFromFile();
        }

        public void ReadFromFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                StreamReader s = new StreamReader(fileName);
                //read titles to remove.
                s.ReadLine();
                for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
                {
                    string[] inputData = s.ReadLine().Split(',');
                    for (int dataIndex = 0; dataIndex < 8; dataIndex++)
                    {
                        fileData[dataIndex, driverIndex] = float.Parse(inputData[dataIndex + 1]);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the formatting of the text box when its value is changed
        /// </summary>
        /// <param name="e">The event arguments generated from the value changed event</param>
        void SetColours(TextChangedEventArgs e)
        {
            float parameterValue = float.Parse(e.Value);
            //Set the fill colour if the value is not the default
            if (IsDefault(parameterValue, e.ParameterIndex - 3, e.DriverIndex)) { e.TextBox.BackColor = Color.Yellow; }
            else
            {
                e.TextBox.BackColor = Color.Cyan;
                if (!e.TextBox.textChanged) { e.TextBox.BackColor = Color.White; }
            }

            //Set the fore colour if the value is outside of a given range.
            if (IsAnomaly(parameterValue, e.ParameterIndex - 3)) { e.TextBox.ForeColor = Color.Red; }
            else { e.TextBox.ForeColor = ParameterTextBox.DefaultForeColor; }
        }

        /// <summary>
        /// Updates a parameter value after the text box has been altered
        /// </summary>
        /// <param name="e">The text changed event arguments generated when the text box text has been changed</param>
        public void UpdateParameter(TextChangedEventArgs e)
        {
            SetPaceData(e.ParameterIndex - 3, e.DriverIndex, float.Parse(e.Value));
            SetColours(e);

            if (e.ParameterIndex == 10)
            {
                string message = "Changing this value will cause changes to all other pace parameters. "
                    + "To see these changes, the values must be updated."
                    + "\r\nUpdate values now?";
                string caption = "Update Values?";

                if (Functions.StartDialog(message, caption))
                {
                    EditDriverParameters();
                }

            }
        }

        /// <summary>
        /// Returns true if  the current value is the same as the default value, stored in the settings file.
        /// </summary>
        public bool IsDefault(float currentValue, int dataIndex, int driverIndex)
        {
            bool sameAsDefault = false;

            switch (dataIndex)
            {
                case 0: sameAsDefault = (paceData[dataIndex, driverIndex] == Data.Settings.DefaultTopSpeed); break;
                case 1: sameAsDefault = (paceData[dataIndex, driverIndex] == Data.Settings.DefaultCompoundDelta); break;
                case 2: sameAsDefault = (paceData[dataIndex, driverIndex] == Data.Settings.DefaultPrimeDegradation); break;
                case 3: sameAsDefault = (paceData[dataIndex, driverIndex] == Data.Settings.DefaultOptionDegradation); break;
                case 4: sameAsDefault = (paceData[dataIndex, driverIndex] == Data.Settings.DefaultPace); break;
                case 5: sameAsDefault = (paceData[dataIndex, driverIndex] == Data.Settings.DefaultFuelEffect); break;
                case 6: sameAsDefault = (paceData[dataIndex, driverIndex] == Data.Tracks[Data.RaceIndex].fuelPerLap); break;
                case 7: sameAsDefault = (paceData[dataIndex, driverIndex] == Data.Settings.DefaultP2Fuel); break;
                default: sameAsDefault = false; break;
            }

            return sameAsDefault;
        }

        /// <summary>
        /// Returns true if the current value is outside of 25% of the default value, stored in the settings.
        /// </summary>
        public bool IsAnomaly(float currentValue, int dataIndex)
        {
            bool withinBounds = true;
            float defaultValue;

            switch (dataIndex)
            {
                case 0: defaultValue = Data.Settings.DefaultTopSpeed; break;
                case 1: defaultValue = Data.Settings.DefaultCompoundDelta; break;
                case 2: defaultValue = Data.Settings.DefaultPrimeDegradation; break;
                case 3: defaultValue = Data.Settings.DefaultOptionDegradation; break;
                case 5: defaultValue = Data.Settings.DefaultFuelEffect; break;
                case 6: defaultValue = Data.Tracks[Data.RaceIndex].fuelPerLap; break;
                case 7: defaultValue = Data.Settings.DefaultP2Fuel; break;
                default: defaultValue = 0; break;
            }

            if (dataIndex != 4)
                withinBounds = (Math.Abs(currentValue - defaultValue) <= Math.Abs(defaultValue * 0.4));

            return !withinBounds;
        }

        /// <summary>
        /// Recalculates the driver pace paramters after a significant change.
        /// </summary>
        public void EditDriverParameters()
        {
            //Recalculate all pace parameters
            Model.CalculationControllers.CalculationController.CalculatePaceParameters();
            //Refresh the data held about the pace parameters
            RefreshData(false);
        }

        /// <summary>
        /// Displays an error message to the user if a new strategy optimisation is started without
        /// completing the parameter update.
        /// </summary>
        void CheckUpdate()
        {
            string message = "Settings have been altered. This may affect the calculated pace parameters." +
                "Recalculate parameters now?";
            string caption = "Recalculate Parameters";
            if (Functions.StartDialog(message, caption))
            {
                EditDriverParameters();
            }
        }

        public void Dispose()
        {
            SaveFileDialog.Dispose();
            OpenFileDialog.Dispose();
        }

        public delegate void DataLoadedEventHandler();
        public event DataLoadedEventHandler DataLoaded;

        public float[,] FileData
        { get { return fileData; } }
    }
}
