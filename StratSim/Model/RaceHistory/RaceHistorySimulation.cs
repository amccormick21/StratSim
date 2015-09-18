using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataSources;
using Graphing;
using MyFlowLayout;
using StratSim.View.Panels;
using StratSim.View.MyFlowLayout;
using System.Windows.Forms;
using System.IO;
using StratSim.Model.Files;
using StratSim.View.UserControls;

namespace StratSim.Model.RaceHistory
{
    /// <summary>
    /// Models a race based on actual data recovered from the race history
    /// </summary>
    public class RaceHistorySimulation
    {
        /// <summary>
        /// The race lap collection is the only 'constant'. Everything else is calculated by routines,
        /// and modifying anything else will not change the simulation
        /// </summary>
        public List<RaceHistoryLap>[] raceLapCollection;
        private int raceIndex;
        private int LapsInRace { get { return Data.Tracks[raceIndex].laps; } }
        private NewGraph raceHistoryGraph;
        private DriverSelectPanel driverPanel;
        public RacePosition[,] positions;

        /// <summary>
        /// Tyre usage can be modified to affect the tyre types associated with different stints.
        /// </summary>
        private List<TyreType>[] raceTyreUsage;

        internal event EventHandler SimulationInitialised;

        public RaceHistorySimulation(List<RaceHistoryLap>[] raceLapCollection, int raceIndex, MainForm associatedForm)
        {
            //Set up relevant panels
            if (associatedForm.IOController != null)
            {
                SetupInterface(associatedForm);
            }

            //Initialise the collection
            this.raceLapCollection = raceLapCollection;
            this.raceIndex = raceIndex;
            positions = GetPositionsData();
            FindEvents();
            SetupTyreUsage();
            if (SimulationInitialised != null)
                SimulationInitialised(this, new EventArgs());
        }

        /// <summary>
        /// Loads a race history simulation with no race specified so that it can be loaded from file later.
        /// </summary>
        public RaceHistorySimulation(int raceIndex, MainForm associatedForm)
        {
            //Set up relevant panels
            if (associatedForm.IOController != null)
            {
                SetupInterface(associatedForm);
            }

            //Set up parameters
            this.raceIndex = raceIndex;
            //Load the data
            LoadData();
        }

        private void SetupTyreUsage()
        {
            raceTyreUsage = new List<TyreType>[Data.NumberOfDrivers];
            int numberOfStints;
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                raceTyreUsage[driverIndex] = new List<TyreType>();
                numberOfStints = raceLapCollection[driverIndex].Count(l => l.LapStatus == 0) + 1;
                for (int stintIndex = 0; stintIndex < numberOfStints; stintIndex++)
                {
                    raceTyreUsage[driverIndex].Add(stintIndex == 0 ? TyreType.Option : TyreType.Prime);
                }
            }
        }

        private void SetupInterface(MainForm associatedForm)
        {
            raceHistoryGraph = new NewGraph("Race Simulation", associatedForm, Properties.Resources.Graph);
            ((StratSimPanelControlEvents)associatedForm.IOController.PanelControlEvents).OnStartGraph(raceHistoryGraph);
            driverPanel = new DriverSelectPanel(associatedForm);
            driverPanel.ShowTimeGaps = true;
            Program.DriverSelectPanel = driverPanel;
            raceHistoryGraph.SetDriverPanel(driverPanel);
            driverPanel.SetGraph(raceHistoryGraph);
        }

        private void DriverPanel_LapNumberChanged1(object sender, int e)
        {
            throw new NotImplementedException();
        }

        public void SaveData()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Data.Settings.TimingDataBaseFolder;
            saveFileDialog.Title = "Save File";
            saveFileDialog.DefaultExt = ".txt";
            saveFileDialog.FileOk += SaveFileDialog_FileOk;
            saveFileDialog.ShowDialog();
        }
        void SaveFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string file = (sender as SaveFileDialog).FileName;
            WriteToFile(file, raceLapCollection);
        }

        public static void WriteToFile(string fileName, List<RaceHistoryLap>[] raceLapCollection)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
                {
                    sw.WriteLine(Data.Drivers[driverIndex].DriverName);
                    foreach (var lap in raceLapCollection[driverIndex])
                    {
                        sw.WriteLine(lap.ToString());
                    }
                }
            }
        }

        public void LoadData()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Data.Settings.TimingDataBaseFolder;
            openFileDialog.Title = "Open File";
            openFileDialog.DefaultExt = ".txt";
            openFileDialog.FileOk += OpenFileDialog_FileOk;
            openFileDialog.ShowDialog();
        }

        void OpenFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string file = (sender as OpenFileDialog).FileName;
            raceLapCollection = ReadFromFile(file);
            //No need to find events as this will be a complete set
            positions = GetPositionsData();
            if (SimulationInitialised != null)
                SimulationInitialised(this, new EventArgs());
        }

        public static List<RaceHistoryLap>[] ReadFromFile(string fileName)
        {
            List<RaceHistoryLap>[] raceLapCollection = new List<RaceHistoryLap>[Data.NumberOfDrivers];
            string line;
            RaceHistoryLap lap;
            int returnedDriverIndex;
            int driverIndex = 0;
            using (StreamReader sr = new StreamReader(fileName))
            {
                do
                {
                    line = sr.ReadLine();
                    if (TimingData.IsDriverName(line, out returnedDriverIndex))
                    {
                        driverIndex = returnedDriverIndex;
                        raceLapCollection[driverIndex] = new List<RaceHistoryLap>();
                    }
                    else
                    {
                        //It is a lap line.
                        lap = new RaceHistoryLap(line);
                        raceLapCollection[driverIndex].Add(lap);
                    }
                } while (!sr.EndOfStream);
            }

            return raceLapCollection;
        }


        public List<RaceHistoryStint> GetRaceStints(int driverIndex)
        {
            List<RaceHistoryStint> driverStints = new List<RaceHistoryStint>();
            RaceHistoryStint tempStint = null;
            bool stintAdded = true;
            int stintIndex = 0;
            for (int lapIndex = 0; lapIndex < LapsInRace; lapIndex++)
            {
                //Check that the lap exists properly
                if (lapIndex < raceLapCollection[driverIndex].Count)
                {
                    if (lapIndex == 0 || raceLapCollection[driverIndex][lapIndex].LapStatus == 0)
                    {
                        tempStint = new RaceHistoryStint(raceTyreUsage[driverIndex][stintIndex], lapIndex);
                        stintAdded = false;
                    }
                    if (raceLapCollection[driverIndex][lapIndex].LapDeficit == 0)
                    {
                        //All laps count
                        tempStint.AddLap(raceLapCollection[driverIndex][lapIndex].LapTime);
                    }
                    if (raceLapCollection[driverIndex][lapIndex].LapStatus == 2)
                    {
                        driverStints.Add(tempStint);
                        stintAdded = true;
                        stintIndex++;
                    }
                }
            }
            //Add the stint at the end if it has laps remaining
            if (!stintAdded)
            {
                driverStints.Add(tempStint);
            }
            return driverStints;
        }

        public List<RaceHistoryStint>[] GetRaceStints()
        {
            List<RaceHistoryStint>[] raceStints = new List<RaceHistoryStint>[Data.NumberOfDrivers];
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                raceStints[driverIndex] = GetRaceStints(driverIndex);
            }
            return raceStints;
        }

        private RacePosition[,] GetPositionsData()
        {
            var positions = new RacePosition[Data.NumberOfDrivers, LapsInRace];
            var cumulativeTimes = GetCumulativeTimes();
            int[] indicesByPosition;
            float cumulativeTime;
            int pitStops;
            for (int position = 0; position < Data.NumberOfDrivers; position++)
            {
                cumulativeTime = 0;
                pitStops = 0;
                for (int lapIndex = 0; lapIndex < LapsInRace; lapIndex++)
                {
                    indicesByPosition = GetIndexesByPosition(lapIndex);

                    //update the positions:
                    positions[position, lapIndex].driver = indicesByPosition[position];
                    positions[position, lapIndex].position = position;

                    if (lapIndex < cumulativeTimes[indicesByPosition[position]].Count)
                    {
                        cumulativeTime = cumulativeTimes[indicesByPosition[position]][lapIndex].LapTime;
                        if (cumulativeTimes[indicesByPosition[position]][lapIndex].LapStatus == 0)
                            pitStops++;
                    }
                    positions[position, lapIndex].pitStopsBeforeLap = pitStops;
                    positions[position, lapIndex].cumulativeTime = cumulativeTime;

                    if (position == 0)
                    {
                        positions[position, lapIndex].gap = 0;
                        positions[position, lapIndex].interval = lapIndex + 1;
                    }
                    else
                    {
                        positions[position, lapIndex].gap = GetGapBetweenCars(cumulativeTimes, indicesByPosition[0], indicesByPosition[position], lapIndex);
                        positions[position, lapIndex].interval = GetGapBetweenCars(cumulativeTimes, indicesByPosition[position - 1], indicesByPosition[position], lapIndex);
                    }
                }
            }
            return positions;
        }

        /// <summary>
        /// Gets the (positive) time gap between two cars on track
        /// Will return 0 if the gap cannot be calculated, and -x if there is a lap deficit of x laps
        /// </summary>
        private float GetGapBetweenCars(List<RaceHistoryLap>[] cumulativeTimes, int driverAhead, int driverBehind, int lapIndex)
        {
            float gap;
            if (lapIndex < cumulativeTimes[driverBehind].Count && lapIndex < cumulativeTimes[driverAhead].Count)
            {
                if (cumulativeTimes[driverBehind][lapIndex].LapDeficit != cumulativeTimes[driverAhead][lapIndex].LapDeficit)
                {
                    //One of the drivers is counting laps to the end of the race
                    gap = cumulativeTimes[driverAhead][lapIndex].LapDeficit - cumulativeTimes[driverBehind][lapIndex].LapDeficit;
                }
                else
                {
                    //The gap is the difference between lap times
                    gap = cumulativeTimes[driverBehind][lapIndex].LapTime - cumulativeTimes[driverAhead][lapIndex].LapTime;
                }
            }
            else
            {
                //One of the drivers has retired
                gap = -0.5F;
            }
            return gap;
        }

        private int[] GetIndexesByPosition(int lap)
        {
            var cumulativeTimes = GetCumulativeTimes();
            return GetIndexesByPosition(lap, cumulativeTimes);
        }

        /// <summary>
        /// Returns an array representing the driver indices in order of position through the race.
        /// This array includes any drivers who have stopped.
        /// </summary>
        /// <returns></returns>
        private int[] GetIndexesByPosition(int lap, List<RaceHistoryLap>[] cumulativeTimes)
        {
            int[] indicesByPosition = new int[Data.NumberOfDrivers];
            int driversRacingAtThisLap;
            List<int> retirements = new List<int>();
            if (lap != 0)
                retirements = GetRetirementsInOrder(lap - 1);
            List<RaceHistoryLap> timesAtThisLap = GetTimesAtThisLap(cumulativeTimes, lap, out driversRacingAtThisLap);

            //Sort the times at this lap:
            timesAtThisLap = timesAtThisLap.Sort((a, b) => RaceHistoryLap.Compare(a, b));

            //Assign the driver indices
            for (int position = 0; position < driversRacingAtThisLap; position++)
            {
                indicesByPosition[position] = timesAtThisLap[position].DriverIndex;
            }
            for (int position = driversRacingAtThisLap; position < Data.NumberOfDrivers; position++)
            {
                indicesByPosition[position] = retirements[position - driversRacingAtThisLap];
            }
            return indicesByPosition;
        }

        /// <summary>
        /// Gets the retirements up to and including the lap index specified in 'finalLapIndex'
        /// </summary>
        /// <param name="raceLaps"></param>
        /// <param name="finalLapIndex"></param>
        private List<int> GetRetirementsInOrder(int finalLapIndex)
        {
            List<int> retirements = new List<int>();
            //Always check the first lap, because it contains drivers who did not start.
            for (int lapIndex = 0; lapIndex <= finalLapIndex; lapIndex++)
            {
                for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
                {
                    if (lapIndex < raceLapCollection[driverIndex].Count)
                    {
                        if (raceLapCollection[driverIndex][lapIndex].LapAction == RaceLapAction.Retire)
                            retirements.Add(driverIndex);
                    }
                }
            }
            retirements.Reverse();
            return retirements;
        }

        private void FindEvents()
        {
            int[] positionsThisLap = null;
            int[] positionsPreviousLap = null;
            List<int> stuckEncountersThisLap = null;
            List<int> stuckEncountersPreviousLap = null;

            List<RaceHistoryLap>[] cumulativeTimes = GetCumulativeTimes();

            //Work through the laps individually
            for (int lap = 0; lap < LapsInRace; lap++)
            {
                if (lap != 0)
                {
                    positionsPreviousLap = new int[positionsThisLap.Length];
                    positionsThisLap.CopyTo(positionsPreviousLap, 0);
                    stuckEncountersPreviousLap = new List<int>(stuckEncountersThisLap);
                }

                positionsThisLap = GetIndexesByPosition(lap);
                stuckEncountersThisLap = GetStuckEncounters(cumulativeTimes, positionsThisLap, lap);

                if (lap != 0)
                {
                    //Compare the previous lap with this lap
                    SetStuckEncountersThisLap(stuckEncountersThisLap, stuckEncountersPreviousLap, lap);

                    SetOvertakesThisLap(positionsThisLap, positionsPreviousLap, lap);
                }
            }
        }

        private void SetOvertakesThisLap(int[] positionsThisLap, int[] positionsPreviousLap, int lap)
        {
            int indexAtThisPositionLastLap = 0;
            int positionThisLap = 0;
            for (int positionLastLap = 0; positionLastLap < Data.NumberOfDrivers; positionLastLap++)
            {
                indexAtThisPositionLastLap = positionsPreviousLap[positionLastLap];
                positionThisLap = -1;
                for (int positionIndex = positionLastLap - 1; positionIndex >= 0; positionIndex--)
                {
                    if (positionsThisLap[positionIndex] == indexAtThisPositionLastLap)
                    {
                        positionThisLap = positionIndex;
                    }
                }
                if (positionThisLap != -1 && positionThisLap < positionLastLap)
                {
                    //An overtake may have occurred.
                    //if any car which was previously ahead and is now behind did not pit or retire, the overtake is legitimate.
                    if (OvertakeOccurred(positionLastLap, positionThisLap, indexAtThisPositionLastLap, positionsPreviousLap, positionsThisLap, lap))
                        raceLapCollection[indexAtThisPositionLastLap][lap - 1].SetLapAction(RaceLapAction.Overtake);
                }
            }
        }

        private void SetStuckEncountersThisLap(List<int> stuckEncountersThisLap, List<int> stuckEncountersPreviousLap, int lap)
        {
            foreach (var stuckIndex in stuckEncountersThisLap)
            {
                if (stuckEncountersPreviousLap.Contains(stuckIndex))
                {
                    //If stuck for two consecutive laps
                    raceLapCollection[stuckIndex][lap - 1].SetLapAction(RaceLapAction.Stuck);
                    if (raceLapCollection[stuckIndex][lap].LapAction != RaceLapAction.Retire)
                        raceLapCollection[stuckIndex][lap].SetLapAction(RaceLapAction.Stuck);
                }
            }
        }

        private bool OvertakeOccurred(int positionLastLap, int positionThisLap, int indexAtThisPositionLastLap, int[] positionsPreviousLap, int[] positionsThisLap, int lap)
        {
            bool overtakeOccurred = false;
            for (int carAheadPreviousLap = positionLastLap - 1; carAheadPreviousLap >= 0 && !overtakeOccurred; carAheadPreviousLap--) //Cycling through cars which were ahead
            {
                //Check the car is now behind
                for (int carBehindThisLap = positionThisLap + 1; carBehindThisLap < Data.NumberOfDrivers; carBehindThisLap++)
                {
                    if (positionsPreviousLap[carAheadPreviousLap] == positionsThisLap[carBehindThisLap] //If the indices match, the car that was previously ahead is now behind.
                     && raceLapCollection[positionsPreviousLap[carAheadPreviousLap]].Count > lap - 1
                     && raceLapCollection[positionsPreviousLap[carAheadPreviousLap]][lap - 1].LapAction != RaceLapAction.Retire //Check the car ahead did not retire
                     && raceLapCollection[positionsPreviousLap[carAheadPreviousLap]].Count > lap
                     && raceLapCollection[positionsPreviousLap[carAheadPreviousLap]][lap].LapStatus != 2 //Check the car ahead did not pit
                     && raceLapCollection[indexAtThisPositionLastLap].Count > lap - 1)
                        overtakeOccurred = true;
                }
            }
            return overtakeOccurred;
        }


        private List<int> GetStuckEncounters(List<RaceHistoryLap>[] cumulativeTimes, int[] positionsThisLap, int lap)
        {
            float trafficTimeBoundary = 1.0F;
            List<int> stuckEncountersThisLap = new List<int>();
            for (int position = 0; position < Data.NumberOfDrivers; position++)
            {
                if (position > 0)
                {
                    if (cumulativeTimes[positionsThisLap[position]].Count > lap && cumulativeTimes[positionsThisLap[position - 1]].Count > lap)
                    {
                        if (cumulativeTimes[positionsThisLap[position]][lap].LapTime - cumulativeTimes[positionsThisLap[position - 1]][lap].LapTime < trafficTimeBoundary)
                        {
                            stuckEncountersThisLap.Add(positionsThisLap[position]);
                        }
                    }
                }
            }

            return stuckEncountersThisLap;
        }

        private List<RaceHistoryLap> GetTimesAtThisLap(List<RaceHistoryLap>[] cumulativeTimes, int lap, out int driversCompeting)
        {
            List<RaceHistoryLap> timesAtThisLap = new List<RaceHistoryLap>();
            driversCompeting = 0;
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                if (lap < cumulativeTimes[driverIndex].Count)
                {
                    timesAtThisLap.Add(new RaceHistoryLap(driverIndex, lap, cumulativeTimes[driverIndex][lap].LapTime, cumulativeTimes[driverIndex][lap].LapStatus, raceLapCollection[driverIndex][lap].LapAction, cumulativeTimes[driverIndex][lap].LapDeficit));
                    driversCompeting++;
                }
            }
            return timesAtThisLap;
        }

        public List<RaceHistoryLap>[] GetCumulativeTimes()
        {
            List<RaceHistoryLap>[] cumulativeTimes = new List<RaceHistoryLap>[Data.NumberOfDrivers];
            RaceHistoryLap lapData;
            float cumulativeTime;
            int lapIndex;
            bool retired;
            float lapTime;
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                cumulativeTime = 0;
                lapIndex = 0;
                cumulativeTimes[driverIndex] = new List<RaceHistoryLap>();
                retired = false;
                do
                {
                    lapTime = raceLapCollection[driverIndex][lapIndex].LapTime;
                    retired = raceLapCollection[driverIndex][lapIndex].LapAction == RaceLapAction.Retire;
                    cumulativeTime += lapTime;
                    lapData = new RaceHistoryLap(driverIndex, lapIndex, cumulativeTime, raceLapCollection[driverIndex][lapIndex].LapStatus, raceLapCollection[driverIndex][lapIndex].LapAction, raceLapCollection[driverIndex][lapIndex].LapDeficit);
                    cumulativeTimes[driverIndex].Add(lapData);
                } while (!retired && ++lapIndex < LapsInRace);
            }
            return cumulativeTimes;
        }

        internal void SetLapTime(int driverIndex, int lapIndex, float lapTime)
        {
            //Set a given lap time in the race lap collection
            raceLapCollection[driverIndex][lapIndex].SetLapTime(lapTime);

            //Re-evaluate subsequent actions based on this change.
            UpdateSimulation(lapIndex);
        }

        internal void ChangeAction(int driverIndex, int lapIndex, RaceLapAction newAction)
        {
            //Set a given action in the race lap collection
            int lapAffected = LapsInRace;
            RaceLapAction oldAction = raceLapCollection[driverIndex][lapIndex].LapAction;
            if (newAction != oldAction)
            {
                raceLapCollection[driverIndex][lapIndex].SetLapAction(newAction);

                //Next step depends on what change is made
                if (oldAction == RaceLapAction.Retire)
                {
                    //Extrapolate the stint to the end of the race; assume clear and adjust later if necessary
                    ExtrapolateStrategyFromLap(driverIndex, lapIndex, out lapAffected);
                }
                else if (newAction == RaceLapAction.Retire)
                {
                    //Truncate the stint and remove any future stints
                    TruncateStrategyAtLap(driverIndex, lapIndex, out lapAffected);
                }

                //If the new action is clear or retire there are no changes to be made.
                if (newAction == RaceLapAction.Stuck)
                {
                    //Set the car to stuck until it falls away
                    SetCarStuck(driverIndex, lapIndex, out lapAffected);
                }
                else if (newAction == RaceLapAction.Overtake)
                {
                    //Cause an overtake.
                    SetOvertake(driverIndex, lapIndex, out lapAffected);
                }
            }
            //Re-evaluate subsequent lap times based on this change.
            UpdateSimulation(lapAffected);
        }

        private void SetOvertake(int driverIndex, int lapIndex, out int lapAffected)
        {
            int[] indicesByPosition;
            int thisDriverPosition;
            int driverIndexAhead;
            List<RaceHistoryLap>[] cumulativeLapTimes;
            float timeOfDriverAhead, timeOfDriverBehind;
            lapAffected = lapIndex;
            float lapChange;

            //Sets the current driver to be 0.5s ahead of the driver ahead of him until his lap times fall away.
            if (lapIndex < raceLapCollection[driverIndex].Count)
            {
                cumulativeLapTimes = GetCumulativeTimes();
                indicesByPosition = GetIndexesByPosition(lapIndex);
                thisDriverPosition = GetDriverPosition(driverIndex, lapIndex, indicesByPosition);
                driverIndexAhead = GetDriverAhead(driverIndex, lapIndex, indicesByPosition);
                if (driverIndexAhead != driverIndex)
                {
                    //Lap time of driver ahead
                    timeOfDriverAhead = cumulativeLapTimes[driverIndexAhead][lapIndex].LapTime;
                    timeOfDriverBehind = cumulativeLapTimes[driverIndex][lapIndex].LapTime;
                    //Change the lap time of the driver ahead so that he becomes 0.5s behind.
                    lapChange = timeOfDriverBehind + 0.5F - timeOfDriverAhead;
                    raceLapCollection[driverIndexAhead][lapIndex].SetLapTime(raceLapCollection[driverIndexAhead][lapIndex].LapTime + lapChange);
                }
            }
        }

        private int GetDriverAhead(int driverIndex, int lapIndex)
        {
            var indicesByPosition = GetIndexesByPosition(lapIndex);
            return GetDriverAhead(driverIndex, lapIndex, indicesByPosition);
        }

        private int GetDriverAhead(int driverIndex, int lapIndex, int[] indicesByPosition)
        {
            int driverPosition = GetDriverPosition(driverIndex, lapIndex, indicesByPosition);
            int driverIndexAhead = 0;
            if (driverPosition != 0)
                driverIndexAhead = indicesByPosition[driverPosition - 1];

            return driverIndexAhead;
        }

        private int GetDriverPosition(int driverIndex, int lapIndex, int[] indicesByPosition)
        {
            //Linear search
            bool driverFound = false;
            int thisDriverPosition = 0;
            for (int position = 0; !driverFound && position < indicesByPosition.Length; position++)
            {
                if (indicesByPosition[position] == driverIndex)
                {
                    thisDriverPosition = position;
                    driverFound = true;
                }
            }

            return thisDriverPosition;
        }

        private int GetDriverPosition(int driverIndex, int lapIndex)
        {
            //Get the driver ahead
            var indicesByPosition = GetIndexesByPosition(lapIndex);
            return GetDriverPosition(driverIndex, lapIndex, indicesByPosition);
        }

        private void SetCarStuck(int driverIndex, int lapIndex, out int lapAffected)
        {
            int[] indicesByPosition;
            int thisDriverPosition;
            int driverIndexAhead;
            List<RaceHistoryLap>[] cumulativeLapTimes;
            float timeOfDriverAhead, timeOfDriverBehind;
            lapAffected = lapIndex;
            float lapChange, speedDelta;
            if (lapIndex < raceLapCollection[driverIndex].Count)
            {
                cumulativeLapTimes = GetCumulativeTimes();
                indicesByPosition = GetIndexesByPosition(lapIndex);
                thisDriverPosition = GetDriverPosition(driverIndex, lapIndex, indicesByPosition);
                driverIndexAhead = GetDriverAhead(driverIndex, lapIndex, indicesByPosition);

                //Lap time of driver ahead
                if (driverIndexAhead != driverIndex)
                {
                    timeOfDriverAhead = cumulativeLapTimes[driverIndexAhead][lapIndex].LapTime;
                    timeOfDriverBehind = cumulativeLapTimes[driverIndex][lapIndex].LapTime;
                    speedDelta = raceLapCollection[driverIndex][lapIndex].LapTime - raceLapCollection[driverIndexAhead][lapIndex].LapTime;
                    //Only change the lap time if the driver behind is faster and the natural gap is < 0.5s
                    if (timeOfDriverBehind - timeOfDriverAhead + speedDelta < 0.5F && speedDelta < 0)
                    {
                        lapChange = timeOfDriverAhead + 0.5F - timeOfDriverBehind;
                        raceLapCollection[driverIndex][lapIndex].SetLapTime(raceLapCollection[driverIndex][lapIndex].LapTime + lapChange);
                    }
                }
            }
        }

        private void TruncateStrategyAtLap(int driverIndex, int lapIndex, out int lapAffected)
        {
            //Cuts short the stint we are in, and removes all subsequent lap times
            var stints = GetRaceStints(driverIndex);
            var currentStint = stints.Last(s => s.StartLapIndex < lapIndex);
            var currentStintIndex = stints.FindIndex(s => s.Equals(currentStint));
            lapAffected = lapIndex;
            while (currentStintIndex + 1 < stints.Count)
            {
                raceTyreUsage[driverIndex].RemoveAt(currentStintIndex + 1);
            }
            //Now remove the lap times
            while (lapIndex + 1 < raceLapCollection[driverIndex].Count)
            {
                raceLapCollection[driverIndex].RemoveAt(lapIndex + 1);
            }

        }

        private void ExtrapolateStrategyFromLap(int driverIndex, int lapIndex, out int lapAffected)
        {
            var stints = GetRaceStints(driverIndex);
            var pitLoss = GetPitLosses();
            lapAffected = lapIndex;
            List<float> newLapTimes;
            int stintStartLap;
            //Special case where retirement comes directly after pitting:
            if (raceLapCollection[driverIndex][lapIndex].LapStatus == 2)
            {
                //Add a brand new stint afterwards, using final stint as guide
                var finalStint = stints.Last(s => s.StintLength >= 3);
                raceTyreUsage[driverIndex].Add(finalStint.TyreType);
                stintStartLap = lapIndex + 1;
                newLapTimes = RaceHistoryStint.GetLapsBasedOnOtherStint(finalStint, stints.Count - 1, stints.Count, LapsInRace - lapIndex - 1, pitLoss, stints);
            }
            else
            {
                //Extrapolate directly from the current stint.
                var currentStint = stints.Last();
                var currentStintIndex = stints.Count - 1;
                stintStartLap = currentStint.StartLapIndex;
                //The pit loss is set to zero because there is no out-lap
                newLapTimes = currentStint.Extrapolate(LapsInRace - lapIndex - 1, currentStintIndex, stints.Count, 0, 0, stints);
            }
            SetNewLapTimesInStint(stintStartLap, driverIndex, newLapTimes, new bool[] { true, false });
        }

        internal void StintLengthChanged(object sender, StintLengthChangedEventArgs e)
        {
            int lapAffected;
            ChangeStintLength(e.DriverIndex, e.StintIndex, e.NewStintLength, out lapAffected);
            UpdateSimulation(lapAffected);
        }

        internal void TyreTypeChanged(object sender, TyreTypeChangedEventArgs e)
        {
            raceTyreUsage[e.DriverIndex][e.StintIndex] = e.NewTyreType;
        }

        internal void StintOrderChanged(object sender, StintOperationEventArgs e)
        {
            //If a stint is added or removed, it must affect the lap times and raceTyreUsage
            //If a stint is swapped with the stint before or after, it is equivalent
            //to swapping tyre types and then changing stint lengths.
            int lapAffected = LapsInRace;
            switch (e.StintOperationIndex)
            {
                case 0: //Swap with previous
                    SwapStints(e.DriverIndex, e.StintIndex - 1, out lapAffected);
                    break;
                case 1: //Swap with next
                    SwapStints(e.DriverIndex, e.StintIndex, out lapAffected);
                    break;
                case 2:
                    InsertStintBefore(e.DriverIndex, e.StintIndex, out lapAffected);
                    break;
                case 3:
                    RemoveStint(e.DriverIndex, e.StintIndex, out lapAffected);
                    break;
            }
            UpdateSimulation(lapAffected);
        }

        /// <summary>
        /// Removes a stint and combines it with the next, or the previous if it is the last stint.
        /// </summary>
        /// <param name="driverIndex"></param>
        /// <param name="stintIndex"></param>
        private void RemoveStint(int driverIndex, int stintIndex, out int lapAffected)
        {
            var stints = GetRaceStints(driverIndex);
            RemoveStint(driverIndex, stintIndex, stints, out lapAffected);
        }

        /// <summary>
        /// Removes a stint and combines it with the next, or the previous if it is the last stint.
        /// </summary>
        private void RemoveStint(int driverIndex, int stintIndex, List<RaceHistoryStint> stints, out int lapAffected)
        {
            //Check if it is the last stint
            if (stintIndex == stints.Count - 1)
            {
                RemoveStint(driverIndex, stintIndex - 1, stints, out lapAffected);
            }
            else
            {
                //Combine this stint with the next stint in line.
                //This is equivalent to setting the length of a stint to zero
                var pitLosses = GetPitLosses();
                ChangeStintLength(driverIndex, stintIndex, 0, pitLosses, stints, out lapAffected);
            }
        }

        /// <summary>
        /// Inserts a stint before the given stint, with the same tyre type and degradation,
        /// and half the length.
        /// </summary>
        private void InsertStintBefore(int driverIndex, int stintIndex, out int lapAffected)
        {
            var stints = GetRaceStints(driverIndex);
            int startLap = stints[stintIndex].StartLapIndex;
            lapAffected = startLap;
            int stintLength = stints[stintIndex].StintLength;
            TyreType stintTyreType = stints[stintIndex].TyreType;

            //Add a tyre type to the race history
            raceTyreUsage[driverIndex].Insert(stintIndex, stintTyreType);

            //Add a new stint to the list of stints
            float[] pitLosses = GetPitLosses();
            var newStintLaps = RaceHistoryStint.GetLapsBasedOnOtherStint(stints[stintIndex], stintIndex, stints.Count + 1, stintLength / 2, pitLosses, stints);
            SetNewLapTimesInStint(startLap, driverIndex, newStintLaps, new bool[] { stintIndex != 0, true });
            //Change the second stint to be shorter
            ModifyStintLength(stints[stintIndex], stintIndex + 1, stints.Count + 1, driverIndex, -(stintLength / 2), true, pitLosses, stints);
        }

        /// <summary>
        /// Swaps a stint with the next stint in the list, if it exists.
        /// Swaps the tyre types and then updates the lengths
        /// </summary>
        private void SwapStints(int driverIndex, int stintIndex, out int lapAffected)
        {
            var stints = GetRaceStints(driverIndex);
            float[] pitLosses = GetPitLosses();
            lapAffected = LapsInRace;

            if (stintIndex >= 0 && stintIndex < stints.Count - 1)
            {
                //Swap the tyre types
                TyreType temp = raceTyreUsage[driverIndex][stintIndex];
                raceTyreUsage[driverIndex][stintIndex + 1] = raceTyreUsage[driverIndex][stintIndex];
                raceTyreUsage[driverIndex][stintIndex] = temp;

                //Swap the lengths
                //Change the length of the first stint to equal the second, this automatically edits the second
                int newStintLength = stints[stintIndex + 1].StintLength;
                ChangeStintLength(driverIndex, stintIndex, newStintLength, pitLosses, stints, out lapAffected);
            }
        }

        /// <summary>
        /// Swaps a stint with the next stint in the list, if it exists.
        /// Swaps the tyre types and then updates the lengths
        /// </summary>
        private void ChangeStintLength(int driverIndex, int stintIndex, int newStintLength, float[] pitLosses, List<RaceHistoryStint> stints, out int lapAffected)
        {
            //Cannot change stint length if only one stint
            lapAffected = LapsInRace;
            if (stints.Count > 1)
            {
                //Length change is negative if the first stint gets longer
                int lengthChange = stints[stintIndex].StintLength - newStintLength;
                //If the stint to be changed is the final stint, change the previous one instead.
                if (stintIndex == stints.Count - 1)
                {
                    ChangeStintLength(driverIndex, stintIndex - 1, stints[stintIndex - 1].StintLength + lengthChange, pitLosses, stints, out lapAffected);
                }
                else
                {
                    ModifyStintLength(stints[stintIndex], stintIndex, stints.Count, driverIndex, -lengthChange, false, pitLosses, stints);
                    lapAffected = stints[stintIndex + 1].StartLapIndex - lengthChange;
                    ModifyStintLength(stints[stintIndex + 1], stintIndex + 1, stints.Count, driverIndex, lengthChange, true, pitLosses, stints);
                }
            }
        }

        private void ChangeStintLength(int driverIndex, int stintIndex, int newStintLength, out int lapAffected)
        {
            var stints = GetRaceStints(driverIndex);
            var pitLosses = GetPitLosses();
            ChangeStintLength(driverIndex, stintIndex, newStintLength, pitLosses, stints, out lapAffected);
        }

        /// <summary>
        /// Changes the laps in raceLapCollection to match the required change in stint length
        /// </summary>
        /// <param name="stint">The stint that is being affected, used to provide tyre type, length, and lap details</param>
        /// <param name="lengthChange">The amount by which the stint is made longer (negative if stint is shorter)</param>
        /// <param name="changeStartLap">True if the starting lap must be moved</param>
        private void ModifyStintLength(RaceHistoryStint stint, int stintIndex, int numberOfStints, int driverIndex, int lengthChange, bool changeStartLap, float[] pitLosses, List<RaceHistoryStint> stints)
        {
            float timeOffset = 0;
            if (changeStartLap)
            {
                //If the starting lap is changed, there is an offset on every lap of the stint due to fuel.
                timeOffset = (Data.Settings.DefaultFuelEffect * Data.Tracks[raceIndex].fuelPerLap) * lengthChange;

                //Change the details of the starting lap.
                stint.StartLapIndex -= lengthChange;
            }

            List<float> newStintLaps;
            if (lengthChange > 0) //Extrapolate the stint
                newStintLaps = stint.Extrapolate(lengthChange, stintIndex, numberOfStints, timeOffset, pitLosses[1], stints);
            else //Truncate the stint
                newStintLaps = stint.Truncate(lengthChange, stintIndex, numberOfStints, timeOffset, pitLosses[1]);

            if (newStintLaps.Count == 0) //The new stint is reduced to zero length so is effectively removed
            {
                raceTyreUsage[driverIndex].RemoveAt(stintIndex);
            }
            else
            {
                SetNewLapTimesInStint(stint.StartLapIndex, driverIndex, newStintLaps, new bool[] { stintIndex != 0, stintIndex != numberOfStints - 1 });
            }
        }

        private void SetNewLapTimesInStint(int startLapIndex, int driverIndex, List<float> newStintLaps, bool[] includePitLaps)
        {
            int startLapInRace = startLapIndex;
            int endLapInRace = startLapIndex + newStintLaps.Count;
            int lapIndexInStint = 0;
            int lapIndexInRace = startLapInRace;
            int lapStatus = (includePitLaps[0] ? 0 : 1);
            while (lapIndexInRace < endLapInRace)
            {
                if (lapIndexInRace < raceLapCollection[driverIndex].Count)
                    raceLapCollection[driverIndex][lapIndexInRace] = new RaceHistoryLap(driverIndex, lapIndexInRace, newStintLaps[lapIndexInStint], lapStatus, raceLapCollection[driverIndex][lapIndexInRace].LapAction, raceLapCollection[driverIndex][lapIndexInRace].LapDeficit);
                else
                    raceLapCollection[driverIndex].Add(new RaceHistoryLap(driverIndex, lapIndexInRace, newStintLaps[lapIndexInStint], lapStatus, RaceLapAction.Clear, raceLapCollection[driverIndex].Last().LapDeficit));

                lapIndexInRace++;
                lapIndexInStint++;

                if (includePitLaps[1] && lapIndexInRace == endLapInRace - 1)
                    lapStatus = 2;
                else
                    lapStatus = 1;
            }
        }

        private float[] GetPitLosses()
        {
            var stints = GetRaceStints();
            float[] stintLosses;
            float outLapLossTotal = 0;
            float inLapLossTotal = 0;
            int outLapLossCount = 0;
            int inLapLossCount = 0;
            float[] trendline;
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                if (RaceHistoryStint.GetStintTrendline(stints[driverIndex], out trendline))
                {
                    for (int stintIndex = 0; stintIndex < stints[driverIndex].Count; stintIndex++)
                    {
                        stintLosses = stints[driverIndex][stintIndex].GetPitLosses(trendline);
                        if (stintIndex != 0)
                        {
                            outLapLossTotal += stintLosses[0];
                            outLapLossCount++;
                        }
                        if (stintIndex != stints[driverIndex].Count - 1)
                        {
                            inLapLossTotal += stintLosses[1];
                            inLapLossCount++;
                        }
                    }
                }
            }
            return new float[] { outLapLossTotal / outLapLossCount, inLapLossTotal / inLapLossCount };
        }

        private void UpdateSimulation(int lapIndexOfChange)
        {
            //Re-evaluate all actions after the given lap index
            EvaluateActions(lapIndexOfChange);
            driverPanel.SetPositionData(GetPositionsData());
            DrawGraph();
        }

        private void EvaluateActions(int lapIndexOfChange)
        {
            List<RaceHistoryLap>[] cumulativeTimes = null;
            float intervalToCarAhead, deltaAtCurrentLap;
            float lapTime, driverAheadLapTime;
            float interpolatedLapTime;
            int driverPosition, driverIndexAhead;
            int leadDriverIndex = 0;
            int[] currentPositions = null;
            RaceLapAction currentLapAction;
            List<RaceHistoryStint>[] stints = GetRaceStints();
            RaceHistoryStint currentStint;
            for (int lapIndex = lapIndexOfChange; lapIndex < LapsInRace; lapIndex++)
            {
                if (lapIndex > 0)
                {
                    cumulativeTimes = GetCumulativeTimes();
                    currentPositions = GetIndexesByPosition(lapIndex - 1, cumulativeTimes);
                    leadDriverIndex = currentPositions[0];
                    for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
                    {
                        driverPosition = GetDriverPosition(driverIndex, lapIndex - 1, currentPositions);
                        if (driverPosition > 0)
                        {
                            //Get gap to car ahead on the previous lap
                            driverIndexAhead = GetDriverAhead(driverIndex, lapIndex - 1, currentPositions);
                            intervalToCarAhead = GetGapBetweenCars(cumulativeTimes, driverIndexAhead, driverIndex, lapIndex - 1);

                            //See what happens at this lap
                            if (lapIndex < raceLapCollection[driverIndex].Count && lapIndex < raceLapCollection[driverIndexAhead].Count)
                            {
                                //Find the current status
                                currentLapAction = raceLapCollection[driverIndex][lapIndex].LapAction;
                                lapTime = raceLapCollection[driverIndex][lapIndex].LapTime;
                                if (lapTime != 0 && currentLapAction != RaceLapAction.Retire)
                                {
                                    driverAheadLapTime = raceLapCollection[driverIndexAhead][lapIndex].LapTime;
                                    deltaAtCurrentLap = lapTime - driverAheadLapTime;
                                    //Can an overtake be made?
                                    if (intervalToCarAhead < -deltaAtCurrentLap)
                                    {
                                        if (currentLapAction == RaceLapAction.Clear && raceLapCollection[driverIndexAhead][lapIndex].LapStatus == 1)
                                        {
                                            raceLapCollection[driverIndex][lapIndex].SetLapTime(driverAheadLapTime - intervalToCarAhead + 0.5F);
                                            raceLapCollection[driverIndex][lapIndex].SetLapAction(RaceLapAction.Stuck);
                                        }
                                    }
                                    else if (intervalToCarAhead + deltaAtCurrentLap < 1F)
                                    {
                                        //Within 'stuck' range.
                                        if (intervalToCarAhead < 1F)
                                        {
                                            if (raceLapCollection[driverIndex][lapIndex - 1].LapAction != RaceLapAction.Overtake)
                                                raceLapCollection[driverIndex][lapIndex - 1].SetLapAction(RaceLapAction.Stuck);
                                            raceLapCollection[driverIndex][lapIndex].SetLapAction(RaceLapAction.Stuck);
                                        }
                                    }
                                    else
                                    {
                                        //Not in stuck range. Set to clear
                                        if (raceLapCollection[driverIndex][lapIndex].LapAction != RaceLapAction.Overtake)
                                            raceLapCollection[driverIndex][lapIndex].SetLapAction(RaceLapAction.Clear);
                                        if (currentLapAction == RaceLapAction.Stuck)
                                        {
                                            //The driver has been 'freed' so the lap is based on interpolation in the stint
                                            currentStint = stints[driverIndex].Last(s => s.StartLapIndex < lapIndex);
                                            interpolatedLapTime = currentStint.Interpolate(lapIndex, stints[driverIndex]);
                                            if (interpolatedLapTime < raceLapCollection[driverIndex][lapIndex].LapTime)
                                                raceLapCollection[driverIndex][lapIndex].SetLapTime(interpolatedLapTime);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            SetLapDeficits(GetCumulativeTimes());
        }

        private void SetLapDeficits(List<RaceHistoryLap>[] cumulativeTimes)
        {
            bool unlapped;
            int lapDeficit, lapsBehindLeader;
            int[] positions = GetIndexesByPosition(LapsInRace - 1, cumulativeTimes);
            float leaderTime = cumulativeTimes[positions[0]][LapsInRace - 1].LapTime;
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                unlapped = false;
                lapsBehindLeader = 0;
                for (int lapIndex = LapsInRace - 2; lapIndex > 0 && !unlapped; lapIndex--)
                {
                    if (lapIndex < cumulativeTimes[driverIndex].Count)
                    {
                        unlapped = cumulativeTimes[driverIndex][lapIndex].LapTime < leaderTime;
                        if (!unlapped)
                        {
                            lapsBehindLeader++;
                        }
                    }
                }
                lapDeficit = 1;
                for (int lapIndex = LapsInRace - lapsBehindLeader; lapIndex < LapsInRace; lapIndex++)
                {
                    if (lapIndex < raceLapCollection[driverIndex].Count)
                    {
                        raceLapCollection[driverIndex][lapIndex].SetLapTime(0);
                        raceLapCollection[driverIndex][lapIndex].SetLapDeficit(lapDeficit++);
                    }
                }
            }
        }

        /// <summary>
        /// Displays the history of the race
        /// </summary>
        internal NewGraph StartGraph()
        {
            //Show time gaps:
            driverPanel.SetPositionData(GetPositionsData());
            //Get traces and show the graph
            DrawGraph();
            return raceHistoryGraph;
        }

        private void DrawGraph()
        {
            var traces = GetGraphTraces();
            raceHistoryGraph.DrawGraph(traces, true, false);
        }

        private List<GraphLine> GetGraphTraces()
        {
            List<RaceHistoryLap>[] cumulativeTimes = GetCumulativeTimes();
            List<GraphLine> traces = new List<GraphLine>();
            for (int driver = 0; driver < Data.NumberOfDrivers; driver++)
            {
                traces.Add(new GraphLine(driver, true, Data.Drivers[driver].LineColour));
            }

            DataPoint tempPoint = new DataPoint();
            for (int driverIndex = 0; driverIndex < Data.NumberOfDrivers; driverIndex++)
            {
                traces[driverIndex].DataPoints.Add(new DataPoint(0, 0, driverIndex, 0));
                for (int lapIndex = 0; lapIndex < LapsInRace; lapIndex++)
                {
                    if (lapIndex < cumulativeTimes[driverIndex].Count && cumulativeTimes[driverIndex][lapIndex].LapDeficit == 0)
                    {
                        tempPoint.Y = cumulativeTimes[driverIndex][lapIndex].LapTime;
                        tempPoint.X = lapIndex + 1;
                        tempPoint.index = driverIndex;
                        tempPoint.cycles = 0;

                        traces[driverIndex].DataPoints.Add(tempPoint);
                    }
                }
            }
            return traces;
        }
    }
}
