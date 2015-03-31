using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DataSources.DataConnections;
using DataSources;

namespace StratSim.Model.Files
{
    /// <summary>
    /// Processes data about driver top speeds from a PDF file
    /// </summary>
    public class SpeedData : TimingData, ISessionData
    {
        float[] topSpeeds;

        public SpeedData(string passedData)
            : base (passedData)
        {
            topSpeeds = new float[Data.NumberOfDrivers];
        }

        public SpeedData()
            : base()
        {
            topSpeeds = new float[Data.NumberOfDrivers];
        }

        public override void UpdateDatabaseWithData(Session session, int raceIndex)
        {
            base.UpdateDatabaseWithData(session, raceIndex);

            int[] positionByDriverIndex = GetPositionsFromDriverIndex();

            Result[,] results = new Result[Data.NumberOfDrivers, Data.NumberOfTracks];

            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                results[driverIndex, raceIndex].modified = true;
                //positions are returned as zero based.
                if (driverIndex >= positionByDriverIndex.Length)
                {
                    results[driverIndex, raceIndex].position = driverIndex + 1;
                    results[driverIndex, raceIndex].finishState = FinishingState.DidNotEnter;
                }
                else
                {
                    results[driverIndex, raceIndex].position = positionByDriverIndex[driverIndex] + 1;
                    results[driverIndex, raceIndex].finishState = 0;
                }
            }

            DriverResultsTableUpdater.SetResults(results, session, Data.NumberOfDrivers, Data.NumberOfTracks, Properties.Settings.Default.CurrentYear, Driver.GetDriverIndexDictionary(), Driver.GetDriverNumberArray());
        }

        private int[] GetPositionsFromDriverIndex()
        {
            int[] resultsByDriverIndex = new int[Data.NumberOfDrivers];

            DriverTimingDataElement[] driverTopSpeeds = new DriverTimingDataElement[Data.NumberOfDrivers];

            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                driverTopSpeeds[driverIndex].driverIndex = driverIndex;
                driverTopSpeeds[driverIndex].value = TopSpeeds[driverIndex];
            }

            Functions.QuickSort<DriverTimingDataElement>(ref driverTopSpeeds, 0, driverTopSpeeds.Length - 1, (a, b) => { return a.value < b.value; });

            for (int position = 0; position < Data.NumberOfDrivers; position++)
            {
                resultsByDriverIndex[driverTopSpeeds[position].driverIndex] = position;
            }

            return resultsByDriverIndex;
        }

        public void AnalyseData(Session session)
        {
            //Session index is implicit
            CollectAndProcessData();
        }

        public void WriteArchiveData(Session session, int raceIndex)
        {
            string filePath = GetTimingDataDirectory(raceIndex, Properties.Settings.Default.CurrentYear) + GetFileName(session, raceIndex);
            WriteToFile(filePath);
        }

        public void RetrieveArchiveData(Session session, int raceIndex)
        {
            string filePath = GetTimingDataDirectory(raceIndex, Properties.Settings.Default.CurrentYear) + GetFileName(session, raceIndex);
            ReadFromFile(filePath);
        }

        public void CollectAndProcessData()
        {
            string lineInput = "";
            int lineNumber = 0;
            int rank = 0;
            int processedDrivers = 1;
            int driverNumber = 0;
            int driverIndex;
            bool readData = true;
            float driverTopSpeed;

            lineInput = GetNextLineToInput(lineNumber++, FileData);
            do
            {
                driverTopSpeed = Data.Settings.DefaultTopSpeed;
                readData = true;

                rank = GetNumber(lineInput, 0);

                //check ranking against processed drivers
                if (rank != processedDrivers)
                { readData = false; }
                else
                {
                    //remove the driver's ranking.
                    lineInput = lineInput.Remove(0, (processedDrivers >= 10 ? 3 : 2));

                    processedDrivers++;

                    //check the driver's name.
                    readData = NameCheck(lineInput, out driverNumber);
                }

                driverIndex = Driver.ConvertToDriverIndex(driverNumber);

                if (readData)
                {
                    driverTopSpeed = AssignDriverSpeedData(lineInput, driverIndex);
                    TopSpeeds[driverIndex] = driverTopSpeed;
                }

            } while ((lineInput = GetNextLineToInput(lineNumber++, FileData)) != null);
        }

        float AssignDriverSpeedData(string speed, int driverIndex)
        {
            float driverTopSpeed = 0;
            int decimalPointPosition = 0;
            int characterIndex = 0;

            speed = speed.Remove(0, Data.Drivers[driverIndex].DriverName.Length + (Data.Drivers[driverIndex].DriverNumber >= 10 ? 6 : 5) + 1);
            speed = speed.Substring(0, 5);

            decimalPointPosition = speed.LastIndexOf('.');
            for (int orderOfMagnitude = decimalPointPosition - 1; orderOfMagnitude >= 0; --orderOfMagnitude)
            {
                //Sums the orders of magnitude of the top speed
                driverTopSpeed += CovertCharToInt(speed[characterIndex++]) * (float)Math.Pow(10, orderOfMagnitude);
            }
            //Adds the value of the first decimal point
            driverTopSpeed += (float)(CovertCharToInt(speed[decimalPointPosition + 1]) * 0.1);

            return driverTopSpeed;
        }

        public void WriteToFile(string fileName)
        {
            string lineInput = "";
            int lineNumber = 0;
            bool readData;
            int rank = 0;
            int processedDrivers = 1;
            int driverNumber = 0;
            int driverIndex;
            float topSpeed = Data.Settings.DefaultTopSpeed;

            StreamWriter w = new StreamWriter(fileName);

            while ((lineInput = GetNextLineToInput(lineNumber++, FileData)) != null)
            {
                readData = true;

                rank = GetNumber(lineInput, 0);

                //check ranking against processed drivers
                if (rank != processedDrivers)
                { readData = false; }
                else
                {
                    //remove the driver's ranking.
                    lineInput = lineInput.Remove(0, (processedDrivers >= 10 ? 3 : 2));

                    processedDrivers++;

                    //check the driver's name.
                    readData = NameCheck(lineInput, out driverNumber);
                }

                driverIndex = Driver.ConvertToDriverIndex(driverNumber);

                if (readData)
                {
                    topSpeed = AssignDriverSpeedData(lineInput, driverIndex);

                    w.Write(driverIndex);
                    w.Write(",");
                    w.Write(Data.Drivers[driverIndex].DriverName);
                    w.Write(",");
                    w.WriteLine(topSpeed);
                }
            }

            w.Dispose();
        }

        public void ReadFromFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                string line = "";

                StreamReader r = new StreamReader(fileName);

                do
                {
                    line = r.ReadLine();
                    SetDriverSpeed(line);
                } while (!r.EndOfStream);
            }
        }

        public void SetDriverSpeed(string lineContainingDataFromFile)
        {
            string[] lineParts;

            float driverSpeed = Data.Settings.DefaultTopSpeed;
            int driverIndex = 0;

            lineParts = lineContainingDataFromFile.Split(',');
            driverSpeed = float.Parse(lineParts[2]);
            driverIndex = int.Parse(lineParts[0]);

            TopSpeeds[driverIndex] = driverSpeed;
        }

        public float[] TopSpeeds
        {
            get { return topSpeeds; }
            set { topSpeeds = value; }
        }
    }
}
