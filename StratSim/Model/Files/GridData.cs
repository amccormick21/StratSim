using DataSources.DataConnections;
using System;
using System.Collections.Generic;
using System.IO;

namespace StratSim.Model.Files
{
    /// <summary>
    /// Processes qualifying classification data from PDF files
    /// </summary>
    public class GridData : TimingData, ISessionData
    {
        int[] gridOrder; //Indexed by position, values are driver indices.

        public GridData(string passedData)
            : base(passedData)
        {
            gridOrder = new int[Data.NumberOfDrivers];
        }

        public GridData()
            : base()
        {
            gridOrder = new int[Data.NumberOfDrivers];
        }

        public override void UpdateDatabaseWithData(Session session, int raceIndex)
        {
            base.UpdateDatabaseWithData(session, raceIndex);

            Result[,] results = new Result[Data.NumberOfDrivers, Data.NumberOfTracks];
            int[] positionsByDriverIndex = GetPositionsByDriverIndex(GridOrder);

            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                results[driverIndex, raceIndex].modified = true;
                //extract data from grid order.
                if (driverIndex >= positionsByDriverIndex.Length)
                {
                    //if out of range
                    results[driverIndex, raceIndex].position = driverIndex+1;
                    results[driverIndex, raceIndex].finishState = FinishingState.DidNotEnter;
                }
                else
                {
                    //update normally
                    results[driverIndex, raceIndex].position = positionsByDriverIndex[driverIndex] + 1;
                    results[driverIndex, raceIndex].finishState = 0;
                }
            }

            DriverResultsTableUpdater.SetResults(results, session, Data.NumberOfDrivers, Data.NumberOfTracks, Properties.Settings.Default.CurrentYear, Driver.GetDriverIndexDictionary(), Driver.GetDriverNumberArray());
        }

        public void AnalyseData(Session session)
        {
            ProcessData();
        }

        private void ProcessData()
        {
            int[] gridOrder = new int[FileData.Length];
            for (int driverIndex = 0; driverIndex < FileData.Length; driverIndex++)
            {
                gridOrder[driverIndex] = Convert.ToInt32(FileData[driverIndex]);
            }

            GridOrder = gridOrder;
        }

        /// <summary>
        /// Processes data from the file data string to ascertain a grid order.
        /// Returns an array of driver indices indexed by position.
        /// Overloaded to avoid the out parameter.
        /// </summary>
        private static int[] GetDataFromFileString(string[] fileData, int raceYear)
        {
            int driversRacing;
            return GetDataFromFileString(fileData, raceYear, out driversRacing);
        }

        /// <summary>
        /// Processes data from the file data string to ascertain a grid order.
        /// Returns an array of driver indices indexed by position.
        /// </summary>
        private static int[] GetDataFromFileString(string[] fileData, int raceYear, out int driversRacing)
        {
            int[] gridOrder;
            //TODO: save the 2015 search?
            gridOrder = Get2014GridDataFromFile(fileData, out driversRacing);
            /*if (raceYear == 2014)
                gridOrder = Get2014GridDataFromFile(fileData, out driversRacing);
            else
                gridOrder = Get2015GridDataFromFile(fileData, out driversRacing);*/
            return gridOrder;
        }

        private static int[] Get2015GridDataFromFile(string[] fileData, out int driversRacing)
        {
            int[] driverIndexArray = new int[Data.NumberOfDrivers];

            //Split the html code into separate words and tags
            string[] tableCells = fileData[0].Split(new string[] { " ", "<", ">" }, StringSplitOptions.RemoveEmptyEntries);

            List<string> driverNames = new List<string>();
            foreach (var driver in Data.Drivers)
            {
                driverNames.Add(driver.DriverName.ToUpper());
            }

            int driverFoundIndex = 0;
            int position = 0;
            //We will search through the words to find the words that are driver surnames.
            foreach (var tableEntry in tableCells)
            {
                driverFoundIndex = driverNames.IndexOf(tableEntry.ToUpper());
                //If we can find the table entry in the list of driver names...
                if (driverFoundIndex != -1)
                {
                    //The position and driver index are recorded
                    driverIndexArray[position++] = driverFoundIndex;
                }
            }
            //The number of driver names found in the list is the number of drivers racing
            driversRacing = position;
            return driverIndexArray;
        }

        private static int[] Get2014GridDataFromFile(string[] fileData, out int driversRacing)
        {
            string lineInput = "";
            int lineNumber = 0;
            int[] gridOrder;

            int driverIndex;
            int driverNumber;

            bool startedReading = false;
            bool finishedReading = false;
            //Positions are one-based but array indices are 0 based.
            //An adjustment is required in the array indexing for this.
            int position = 1;

            gridOrder = new int[Data.NumberOfDrivers];

            do
            {
                lineInput = GetNextLineToInput(lineNumber++, fileData);
                if (NameCheck(lineInput, out driverNumber))
                {
                    startedReading = true;
                    //If the name is at the start of the line, the driver did not finish.
                    //His position is therefore set based on the position iterator.
                    driverIndex = Driver.ConvertToDriverIndex(driverNumber);
                    gridOrder[position - 1] = driverIndex;
                    position++;
                }
                else
                {
                    if (GetNumber(lineInput, 0) == position)
                    {
                        startedReading = true;
                        //Position has been found.
                        lineInput = lineInput.Remove(0, (position >= 10 ? 3 : 2));
                        if (NameCheck(lineInput, out driverNumber))
                        {
                            //Valid driver name is in position
                            driverIndex = Driver.ConvertToDriverIndex(driverNumber);

                            //Update the array
                            gridOrder[position - 1] = driverIndex;

                            //Get ready to read the next position
                            position++;
                        }
                    }
                    else
                    {
                        //This is not a valid driver line
                        if (startedReading)
                            finishedReading = true;
                    }
                }
            } while (lineInput != null && !finishedReading); //New to adjust for fewer cars on the grid

            driversRacing = position - 1;
            return gridOrder;
        }

        public void WriteArchiveData(Session session, int raceIndex)
        {
            string filePath = GetTimingDataDirectory(raceIndex, Properties.Settings.Default.CurrentYear) + GetFileName(session, raceIndex);
            WriteToFile(filePath, GridOrder);
        }

        public void RetrieveArchiveData(Session session, int raceIndex)
        {
            string filePath = GetTimingDataDirectory(raceIndex, Properties.Settings.Default.CurrentYear) + GetFileName(session, raceIndex);
            ReadFromFile(filePath);
        }

        public void ReadFromFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                StreamReader r = new StreamReader(fileName);
                int position = 0;
                
                while (!r.EndOfStream)
                {
                    //Data read in is driver numbers
                    GridOrder[position++] = Driver.ConvertToDriverIndex(Convert.ToInt32(r.ReadLine()));
                }

                r.Dispose();
            }
        }

        public void WriteToFile(string fileName)
        {
            WriteToFile(fileName, GridOrder);
        }

        public static void WriteToFile(string fileName, int[] gridOrder)
        {
            StreamWriter w = new StreamWriter(fileName);

            for (int position = 0; position < gridOrder.Length; position++)
            {
                //Data that is written is driver numbers
                w.WriteLine(Convert.ToString(Data.Drivers[gridOrder[position]].DriverNumber));
            }

            w.Dispose();
        }

        public void SetGridOrder(int[] gridOrder)
        {
            GridOrder = gridOrder;
        }

        public int[] GridOrder
        {
            get { return gridOrder; }
            private set { gridOrder = value; }
        }

        internal int[] GetPositionsByDriverIndex()
        {
            return GetPositionsByDriverIndex(GridOrder);
        }

        internal static int[] GetPositionsByDriverIndex(int[] gridOrder)
        {
            return GetPositionsByDriverIndex(gridOrder, gridOrder.Length);
        }

        internal static int[] GetPositionsByDriverIndex(int[] gridOrder, int driversRacing)
        {
            int[] positionsByDriverIndex = new int[driversRacing];
            List<int> gridList = new List<int>(gridOrder);
            int driverIndexGridPosition = 0;

            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                driverIndexGridPosition = gridList.IndexOf(driverIndex);
                if (driverIndexGridPosition != -1)
                    positionsByDriverIndex[driverIndex] = driverIndexGridPosition;
            }
            return positionsByDriverIndex;
        }

        internal static int[] GetGridOrderFromFileText(string dataFromClipboard, int currentYear)
        {
            string[] fileText = ConvertToArray(dataFromClipboard);

            int driversRacing;
            int[] gridOrder = GetDataFromFileString(fileText, currentYear, out driversRacing);
            //int[] positionsByDriverIndex = GetPositionsByDriverIndex(gridOrder, driversRacing);

            return gridOrder;
        }
    }
}
