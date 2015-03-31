using System.Globalization;
using System.IO;

namespace StratSim.Model
{
    public class Settings
    {
        public Settings()
        {
            LoadData();
        }

        float requiredPaceDelta, requiredSpeedDelta, timeLoss, backmarkerLoss, timeGap;
        float defaultCompoundDelta, optionDegradation, primeDegradation, topSpeed, fuelEffect, pace, P2Fuel;
        string dataFilePath;
        float trackImprovement;
        string PDFReader;
        string timingDataBaseFolder;
        float[] trackEvolution = new float[5];

        public const string SettingsFile = "../../../StratSim/Settings.txt";

        #region Settings
        /// <summary>
        /// Pace delta required for an overtake to be certain.
        /// </summary>
        public float RequiredPaceDelta
        {
            get
            {
                return requiredPaceDelta;
            }
            set
            {
                requiredPaceDelta = value;
            }
        }
        /// <summary>
        ///Speed delta required for an overtake to be certain.
        /// </summary>
        public float RequiredSpeedDelta
        {
            get
            {
                return requiredSpeedDelta;
            }
            set
            {
                requiredSpeedDelta = value;
            }
        }
        /// <summary>
        ///Time loss implemented when overtaken by another car.
        /// </summary>
        public float TimeLoss
        {
            get
            {
                return timeLoss;
            }
            set
            {
                timeLoss = value;
            }
        }
        /// <summary>
        ///Time loss for a backmarker when lapped by another car.
        /// </summary>
        public float BackmarkerLoss
        {
            get
            {
                return backmarkerLoss;
            }
            set
            {
                backmarkerLoss = value;
            }
        }
        /// <summary>
        ///Minimum gap between cars when following each other.
        /// </summary>
        public float TimeGap
        {
            get
            {
                return timeGap;
            }
            set
            {
                timeGap = value;
            }
        }
        /// <summary>
        ///Default difference in pace between the two tyre compounds.
        /// </summary>
        public float DefaultCompoundDelta
        {
            get
            {
                return defaultCompoundDelta;
            }
            set
            {
                defaultCompoundDelta = value;
            }
        }
        /// <summary>
        ///Default degradation per lap on the option tyre.
        /// </summary>
        public float DefaultOptionDegradation
        {
            get
            {
                return optionDegradation;
            }
            set
            {
                optionDegradation = value;
            }
        }
        /// <summary>
        ///Default degradation per lap on the prime tyre.
        /// </summary>
        public float DefaultPrimeDegradation
        {
            get
            {
                return primeDegradation;
            }
            set
            {
                primeDegradation = value;
            }
        }
        /// <summary>
        ///Default top speed.
        /// </summary>
        public float DefaultTopSpeed
        {
            get
            {
                return topSpeed;
            }
            set
            {
                topSpeed = value;
            }
        }
        /// <summary>
        ///Default effect per kilogram of fuel.
        /// </summary>
        public float DefaultFuelEffect
        {
            get
            {
                return fuelEffect;
            }
            set
            {
                fuelEffect = value;
            }
        }
        /// <summary>
        ///Default pace for cars with no data present.
        /// </summary>
        public float DefaultPace
        {
            get
            {
                return pace;
            }
            set
            {
                pace = value;
            }
        }
        /// <summary>
        ///Default fuel used during the P2 session
        /// </summary>
        public float DefaultP2Fuel
        {
            get
            {
                return P2Fuel;
            }
            set
            {
                P2Fuel = value;
            }
        }
        /// <summary>
        ///File path of the base folder where race timing data is to be stored.
        /// </summary>
        public string DataFilePath
        {
            get
            {
                return dataFilePath;
            }
            set
            {
                dataFilePath = value;
            }
        }
        /// <summary>
        ///Amount by which the track gets faster for every lap in the session.
        /// </summary>
        public float TrackImprovement
        {
            get
            {
                return trackImprovement;
            }
            set
            {
                trackImprovement = value;
            }
        }
        /// <summary>
        ///Name of the PDF reader application.
        /// </summary>
        public string PDFReaderName
        {
            get
            {
                return PDFReader;
            }
            set
            {
                PDFReader = value;
            }
        }
        /// <summary>
        ///Base folder for PDF formatted timing data.
        /// </summary>
        public string TimingDataBaseFolder
        {
            get
            {
                return timingDataBaseFolder;
            }
            set
            {
                timingDataBaseFolder = value;
            }
        }
        /// <summary>
        ///Array of five values representing the track performance variation in the five weekend sessions
        /// </summary>
        public float[] TrackEvolution
        {
            get
            {
                return trackEvolution;
            }
            set
            {
                trackEvolution = value;
            }
        }
        #endregion

        /// <summary>
        /// Writes the settings data to a file
        /// </summary>
        public void WriteSettingsData()
        {
            using (StreamWriter s = new StreamWriter(SettingsFile))
            {
                s.WriteLine(requiredPaceDelta);
                s.WriteLine(requiredSpeedDelta);
                s.WriteLine(timeLoss);
                s.WriteLine(backmarkerLoss);
                s.WriteLine(timeGap);
                s.WriteLine(defaultCompoundDelta);
                s.WriteLine(optionDegradation);
                s.WriteLine(primeDegradation);
                s.WriteLine(topSpeed);
                s.WriteLine(fuelEffect);
                s.WriteLine(pace);
                s.WriteLine(P2Fuel);
                s.WriteLine(dataFilePath);

                for (int sessionIndex = 0; sessionIndex <= 3; sessionIndex++)
                {
                    s.Write(trackEvolution[sessionIndex]);
                    s.Write(',');
                }
                s.WriteLine(trackEvolution[4]);
                s.WriteLine(trackImprovement);
                s.WriteLine(PDFReader);
                s.WriteLine(timingDataBaseFolder);
            }
        }
        /// <summary>
        /// Reads the settings data from a file
        /// </summary>
        public void LoadData()
        {
            var dp = CultureInfo.InvariantCulture.NumberFormat;
            string[] readEvolution = new string[4];

            using (StreamReader r = new StreamReader(SettingsFile))
            {
                requiredPaceDelta = float.Parse(r.ReadLine(), dp);
                requiredSpeedDelta = float.Parse(r.ReadLine(), dp);
                timeLoss = float.Parse(r.ReadLine(), dp);
                backmarkerLoss = float.Parse(r.ReadLine(), dp);
                timeGap = float.Parse(r.ReadLine(), dp);
                defaultCompoundDelta = float.Parse(r.ReadLine(), dp);
                optionDegradation = float.Parse(r.ReadLine(), dp);
                primeDegradation = float.Parse(r.ReadLine(), dp);
                topSpeed = float.Parse(r.ReadLine(), dp);
                fuelEffect = float.Parse(r.ReadLine(), dp);
                pace = float.Parse(r.ReadLine(), dp);
                P2Fuel = float.Parse(r.ReadLine(), dp);
                dataFilePath = r.ReadLine();
                readEvolution = r.ReadLine().Split(',');

                for (int sessionIndex = 0; sessionIndex <= 4; sessionIndex++)
                {
                    trackEvolution[sessionIndex] = float.Parse(readEvolution[sessionIndex]);
                }
                trackImprovement = float.Parse(r.ReadLine(), dp);
                PDFReader = r.ReadLine();
                timingDataBaseFolder = r.ReadLine();
            }
        }   
    }
}
