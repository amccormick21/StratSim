using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataSources.DataConnections;
using StratSim.Model.RaceHistory;
using System.IO;

namespace StratSim.Model.Files
{
    public class HistoryData : TimingData, ISessionData
    {
        public List<RaceHistoryLap>[] RaceLapCollection;

        public HistoryData(string passedData)
            : base(passedData)
        {
            RaceLapCollection = InitialiseRaceLapCollection();
        }

        public HistoryData()
            : base()
        {
            RaceLapCollection = InitialiseRaceLapCollection();
        }

        private List<RaceHistoryLap>[] InitialiseRaceLapCollection()
        {
            var lapCollection = new List<RaceHistoryLap>[Data.NumberOfDrivers];
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; ++driverIndex)
            {
                lapCollection[driverIndex] = new List<RaceHistoryLap>();
            }
            return lapCollection;
        }

        public void AnalyseData(Session session)
        {
            List<int> retirees;
            CollectHistoryDataFromFile(out retirees);
            AnalyseTimings(retirees);
        }

        private void AnalyseTimings(List<int> retirees)
        {
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                //If a driver recorded no laps, add a first lap where they retire
                if (RaceLapCollection[driverIndex].Count == 0)
                {
                    var newLap = new RaceHistoryLap(driverIndex, 0, 0, 2, RaceLapAction.Retire,0);
                    RaceLapCollection[driverIndex].Add(newLap);
                }
                else
                {
                    //Sweep to update the lap status (in-laps ad out-laps)
                    //Note that the first lap is always a normal lap
                    for (int lapIndex = 0; lapIndex < RaceLapCollection[driverIndex].Count - 1; lapIndex++)
                    {
                        if (RaceLapCollection[driverIndex][lapIndex].LapStatus == 2)
                        {
                            RaceLapCollection[driverIndex][lapIndex + 1].SetLapStatus(0);
                        }
                    }
                }
            }

            //Run through the retirees and note a retirement, or fill in the gaps to the end
            int lapDeficit;
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                if (retirees.Count > 0 && retirees[0] == driverIndex)
                {
                    retirees.Remove(driverIndex);
                    RaceLapCollection[driverIndex].Last().SetLapAction(RaceLapAction.Retire);
                }
                else
                {
                    lapDeficit = 0;
                    for (int lapIndex = RaceLapCollection[driverIndex].Count; lapIndex < Data.GetRaceLaps(); lapIndex++)
                    {
                        RaceLapCollection[driverIndex].Add(new RaceHistoryLap(0, lapIndex, 0, 1, RaceLapAction.Clear, ++lapDeficit));
                    }
                }
            }
        }

        private void CollectHistoryDataFromFile(out List<int> retirees)
        {
            string lineInput;
            int driverNumber = 0;
            int driverIndex = 0;
            int lineNumber = 0;
            int currentLapNumber = 1;
            string[] splitLine;
            int lapType;
            string lapTimeString;
            string infoString;
            float lapTime;
            int startOfInfoIndex;
            RaceHistoryLap lap;
            //Set up a list of driver indices that retired.
            retirees = new List<int>();
            for (int i = 0; i < Data.NumberOfDrivers; i++)
            {
                retirees.Add(i);
            }

            do
            {
                //Get the next line
                lineInput = GetNextLineToInput(lineNumber++, FileData);
                if (lineInput != null)
                {
                    //We can analyse the line to see if it matches our requirements
                    if (lineInput == ("LAP " + currentLapNumber.ToString()))
                    {
                        //This marks the start of a lap
                        lineNumber++; //Skip a line (column headings)
                        //Set the current lap number
                        currentLapNumber++;
                        lapType = 1;
                    }
                    else
                    {
                        splitLine = lineInput.Split(' ');
                        if (splitLine.Length > 1 && int.TryParse(splitLine[0], out driverNumber))
                        {
                            //Try and get a driver index
                            driverIndex = Driver.ConvertToDriverIndex(driverNumber);
                            if (driverIndex != -1)
                            {
                                //We have found a valid driver index
                                //Check for 'PIT'
                                lapType = 1;
                                startOfInfoIndex = splitLine[1].IndexOf('.') + 4;
                                lapTimeString = splitLine[1].Substring(0, startOfInfoIndex);
                                lapTime = GetLapTime(lapTimeString);
                                infoString = splitLine[1].Substring(startOfInfoIndex, splitLine[1].Length - startOfInfoIndex);
                                if (infoString.Length >= 3 && infoString.Substring(0, 3) == "PIT")
                                {
                                    lapType = 2;
                                }
                                lap = new RaceHistoryLap(driverIndex, currentLapNumber - 2, lapTime, lapType, RaceLapAction.Clear, 0);
                                RaceLapCollection[driverIndex].Add(lap);
                            }
                        }
                    }
                    if (currentLapNumber == Data.GetRaceLaps() + 1)
                    {
                        //Make sure everyone came across the line
                        retirees.Remove(driverIndex);
                    }
                }
            } while (lineInput != null);
        }

        public void ReadFromFile(string fileName)
        {
            RaceLapCollection = RaceHistorySimulation.ReadFromFile(fileName);
        }

        public void RetrieveArchiveData(Session session, int raceIndex)
        {
            string filePath = GetTimingDataDirectory(raceIndex, Properties.Settings.Default.CurrentYear) + GetFileName(session, raceIndex);
            ReadFromFile(filePath);
        }

        public override void UpdateDatabaseWithData(Session session, int raceIndex)
        {
            //Does nothing: no facility yet to store histories in database.
        }

        public void WriteArchiveData(Session session, int raceIndex)
        {
            string filePath = GetTimingDataDirectory(raceIndex, Properties.Settings.Default.CurrentYear) + GetFileName(session, raceIndex);
            WriteToFile(filePath);
        }

        public void WriteToFile(string fileName)
        {
            if (RaceLapCollection != null)
            {
                RaceHistorySimulation.WriteToFile(fileName, RaceLapCollection);
            }
        }
    }
}
