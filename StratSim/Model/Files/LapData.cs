using System;
using System.IO;
using System.Collections.Generic;
using DataSources.DataConnections;
using DataSources;

namespace StratSim.Model.Files
{
    /// <summary>
    /// Processes timing data about the lap times of drivers from a PDF file.
    /// </summary>
    public class LapData : TimingData, ISessionData
    {
        List<Stint>[] driverWeekendStints; //Indexed by driver.

        public LapData(string passedData)
            : base(passedData)
        {
            InitialiseDriverWeekendStints(ref driverWeekendStints);
        }

        public LapData()
            : base()
        {
            InitialiseDriverWeekendStints(ref driverWeekendStints);
        }

        public override void UpdateDatabaseWithData(Session session, int raceIndex)
        {
            base.UpdateDatabaseWithData(session, raceIndex);

            if (session != Session.Race && session != Session.Qualifying) //Don't update qualifying or the race based on this.
            {
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
        }

        /// <summary>
        /// Calculates and returns an array of positions, indexed by the driver's position in the session.
        /// </summary>
        /// <returns></returns>
        private int[] GetPositionsFromDriverIndex()
        {
            int[] resultsByDriverIndex = new int[Data.NumberOfDrivers];

            DriverTimingDataElement[] fastestLaps = new DriverTimingDataElement[Data.NumberOfDrivers];

            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                fastestLaps[driverIndex].driverIndex = driverIndex;
                fastestLaps[driverIndex].value = GetFastestLapFromSessionData(DriverWeekendStints[driverIndex]);
            }

            Functions.QuickSort<DriverTimingDataElement>(ref fastestLaps, 0, fastestLaps.Length - 1, (a, b) => { return (a.value > b.value); });

            for (int position = 0; position < Data.NumberOfDrivers; position++)
            {
                resultsByDriverIndex[fastestLaps[position].driverIndex] = position;
            }

            return resultsByDriverIndex;
        }

        private float GetFastestLapFromSessionData(List<Stint> sessionData)
        {
            float fastestLap;
            float fastestLapHolder = Data.Settings.DefaultPace;

            for (int stintIndex = 0; stintIndex < sessionData.Count; stintIndex++)
            {
                if (stintIndex == 0)
                {
                    fastestLapHolder = sessionData[stintIndex].FastestLap();
                }
                else
                {
                    fastestLap = sessionData[stintIndex].FastestLap();
                    if (fastestLap < fastestLapHolder)
                    {
                        fastestLapHolder = fastestLap;
                    }
                }
            }

            return fastestLapHolder;
        }

        void InitialiseDriverWeekendStints(ref List<Stint>[] DriverWeekendStints)
        {
            DriverWeekendStints = new List<Stint>[Data.NumberOfDrivers];
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; ++driverIndex)
            {
                DriverWeekendStints[driverIndex] = new List<Stint>();
            }
        }

        public void AnalyseData(Session session)
        {
            CollectAndProcessData(session);
        }

        public void WriteArchiveData(Session session, int raceIndex)
        {
            string filePath = GetTimingDataDirectory(raceIndex, Properties.Settings.Default.CurrentYear) + GetFileName(session, raceIndex);
            WriteToFile(filePath);
        }

        public void RetrieveArchiveData(Session session, int raceIndex)
        {
            string filePath = GetTimingDataDirectory(raceIndex, Properties.Settings.Default.CurrentYear) + GetFileName(session, raceIndex);
            ReadFromFile(session, filePath);
        }

        public void CollectAndProcessData(Session session)
        {
            string lineInput = "";
            int lineNumber = 0;
            int lapNo = 0; //number of laps in driver's stint
            int lineStatus = 0; //0 is no numbers read, 1 is reading, 2 is finished reading
            int driverIndex = -1;
            int driverNumber;
            int readStatus = 0;
            int sessionIndex = session.GetSessionIndex();

            Stint tempStint;
            do //terminates when file ends.
            {
                if (lineStatus == 0)
                    lineInput = GetNextLineToInput(lineNumber++, FileData);

                if (NameCheck(lineInput, out driverNumber) == true)
                {
                    lapNo = 0;
                    lineStatus = 0;
                    driverIndex = Driver.ConvertToDriverIndex(driverNumber);
                    DriverWeekendStints[driverIndex].Clear();
                    tempStint = new Stint(sessionIndex, lapNo);
                    readStatus = 1;

                    do
                    {

                        lineInput = GetNextLineToInput(lineNumber++, FileData);
                        if (GetNumber(lineInput, 0) == lapNo++)
                        {
                            UseTimeData(lineInput, lapNo - 1, driverIndex, sessionIndex, ref readStatus, ref tempStint);
                        }
                        else { lineStatus++; }
                    } while (lineStatus != 2); //exits when times are finished

                    if (tempStint.lapTimes.Count != 0) { DriverWeekendStints[driverIndex] += tempStint; }
                }
                else
                {
                    if (lineStatus == 2)
                        lineInput = GetNextLineToInput(lineNumber++, FileData);
                }
                //end if

            } while (lineInput != null);

        }

        void UseTimeData(string time, int lapNo, int driverIndex, int sessionIndex, ref int readStatus, ref Stint tempStint)
        {
            float lapTime;
            //readStatus - 0: waiting for start, 1: out-lap, 2: add lap to stint.

            time = time.Remove(0, (lapNo >= 10 ? 3 : 2)); //takes away the lap number
            if (time[0] == 'P')
            {
                time = time.Remove(0, 2); //removes the 'P'

                if (readStatus == 2)
                {
                    //pass old stint
                    DriverWeekendStints[driverIndex] += tempStint;

                    //start new tempStint 
                    tempStint = new Stint(sessionIndex, lapNo);
                }

                //get ready to read new stint
                readStatus = 0;
            }

            //load the lap time from a string
            lapTime = GetLapTime(time);

            if (readStatus == 2) //if reading
            {
                //adds lap time to stint
                tempStint += lapTime;
            }
            else
            {
                //increments readStatus if not already reading
                readStatus++;
            }
        }

        public void WriteToFile(string fileName)
        {
            string lineInput = "";
            int lineNumber = 0;
            int lapNumber = 1;
            int lapType = 2;
            int driverIndex = 0;
            int driverNumber = 0;
            float lapTime = 0;

            StreamWriter w = new StreamWriter(fileName);

            while ((lineInput = GetNextLineToInput(lineNumber++, FileData)) != null)
                //While there is data to read
            {
                if (NameCheck(lineInput, out driverNumber)) //If a driver's name is found
                {
                    driverIndex = Driver.ConvertToDriverIndex(driverNumber);
                    lapNumber = 1;
                    lapType = 2;
                    w.WriteLine(Data.Drivers[driverIndex].DriverName);
                }

                else //reading times
                {
                    if (GetNumber(lineInput, 0) == lapNumber)
                    {
                        lineInput = lineInput.Remove(0, (lapNumber >= 10 ? 3 : 2));
                        if (lineInput[0] == 'P')
                        {
                            lineInput = lineInput.Remove(0, 2);
                            lapType = 2;
                        }
                        lapTime = GetLapTime(lineInput);
                        lapNumber++;

                        try
                        {
                            w.Write(lapTime);
                            w.Write(",");
                            w.WriteLine(lapType);
                        }
                        catch
                        {
                            w.WriteLine("0,2");
                        }

                        if (lapType == 2) //if in-lap, cycle the lap status so the next lap is an out-lap
                        { lapType = 0; }
                        else
                        { lapType = 1; }
                    }
                }
            }
            w.Dispose();
        }

        public void ReadFromFile(string fileName)
        {
            Session session = SessionExtensionMethods.GetFromFileName(Path.GetFileNameWithoutExtension(fileName));
            ReadFromFile(session, fileName);
        }

        public void ReadFromFile(Session session, string fileName)
        {
            if (File.Exists(fileName))
            {
                string line = "";
                string[] lineParts;
                float lapTime = 0;
                int lapType = 0;
                int lapNumber = 0;

                int returnedDriverIndex = -1;
                int driverIndex = -1;

                int sessionIndex = session.GetSessionIndex();

                Stint tempStint = null;

                StreamReader r = new StreamReader(fileName);

                do
                {
                    line = r.ReadLine();

                    if (IsDriverName(line, out returnedDriverIndex))
                    {
                        driverIndex = returnedDriverIndex;
                        if (driverIndex - 1 >= 0 && tempStint != null && tempStint.lapTimes.Count > 0) { DriverWeekendStints[driverIndex - 1] += tempStint; }
                        lapNumber = 0;
                        DriverWeekendStints[driverIndex].Clear();
                        tempStint = new Stint(sessionIndex, lapNumber);
                    }
                    else
                    {
                        lineParts = line.Split(',');
                        if (lineParts.Length == 2)
                        {
                            if (lineParts[0] != "")
                                lapTime = float.Parse(lineParts[0]);
                            if (lineParts[1] != "")
                                lapType = int.Parse(lineParts[1]);
                        }


                        lapNumber++;

                        if (lapType == 1 && tempStint != null)
                            tempStint += lapTime;
                        if (lapType == 2 && tempStint != null && tempStint.lapTimes.Count > 0)
                        {
                            DriverWeekendStints[driverIndex] += tempStint;
                            tempStint = new Stint(sessionIndex, lapNumber);
                        }
                    }

                } while (!r.EndOfStream);
                r.Dispose();
            }
        }

        /// <summary>Gets a lap time from a specified string</summary>
        /// <param name="time">A string with the time in mm:ss:000 format</param>
        /// <returns>The number of seconds represented by the time</returns>
        public float GetLapTime(string time)
        {
            float lapTime = 0;
            int decimalPlaces = 3;

            if (time[1] != ':')
            {
                lapTime += CovertCharToInt(time[0]) * 600; //counts tens of minutes if they are present
                time = time.Remove(0, 1); //removes tens of minutes
                decimalPlaces = 2;
            }

            //add minutes, 10 x seconds, seconds
            lapTime += CovertCharToInt(time[0]) * 60;
            lapTime += CovertCharToInt(time[2]) * 10;
            lapTime += CovertCharToInt(time[3]);

            //add thousandths
            for (int characterIndex = 1; characterIndex <= decimalPlaces; characterIndex++)
            {
                try
                { lapTime += CovertCharToInt(time[4 + characterIndex]) * (float)Math.Pow(10, -characterIndex); }
                catch
                { }
            }

            return lapTime;
        }

        public List<Stint>[] DriverWeekendStints
        {
            get { return driverWeekendStints; }
            set { driverWeekendStints = value; }
        }
    }
}
