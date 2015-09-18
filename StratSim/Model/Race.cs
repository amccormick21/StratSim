using StratSim.View.MyFlowLayout;
using StratSim.View.Panels;
using StratSim.View.UserControls;
using StratSim.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using DataSources;
using MyFlowLayout;
using Graphing;

namespace StratSim.Model
{
    public struct RacePosition
    {
        public int position;
        public int driver;
        public float interval;
        public float gap;
        public float cumulativeTime;
        public int pitStopsBeforeLap;

        public override string ToString()
        {
            return position.ToString() + ": " + driver.ToString() + " - " + gap.ToString();
        }
    };

    public class Race : IDisposable
    {
        RaceStrategy[] raceStrategies;
        RaceStrategy[] originalStrategies;
        RacePosition[,] positions;
        bool[,] forcedOvertake;

        int trackIndex;
        int laps;

        NewGraph raceGraph;

        /// <summary>
        /// Sets up a race and updates the graph displayed on screen
        /// </summary>
        /// <param name="TrackIndex">The index of the track on which the race is taking place</param>
        /// <param name="Strategies">The list of strategies representing drivers who are to take part in the race</param>
        /// <param name="AssociatedForm">The form on which the graph is to be displayed</param>
        public Race(int TrackIndex, RaceStrategy[] Strategies, MainForm AssociatedForm)
        {
            trackIndex = TrackIndex;
            laps = Data.Tracks[trackIndex].laps;

            originalStrategies = (RaceStrategy[])Strategies.Clone();
            raceStrategies = (RaceStrategy[])Strategies.Clone();

            positions = new RacePosition[Data.NumberOfDrivers, laps];
            forcedOvertake = Functions.InitialiseArrayAtValue(Data.NumberOfDrivers, laps, false);

            raceGraph = new NewGraph("Race Graph", AssociatedForm, Properties.Resources.Graph);
            raceGraph.GraphPanel.GraphClicked += GraphPanel_GraphClicked;
            ((StratSimPanelControlEvents)AssociatedForm.IOController.PanelControlEvents).OnStartGraph(raceGraph);

            MyEvents.SettingsModified += MyEvents_SettingsModified;
        }

        void GraphPanel_GraphClicked(object sender, DataPoint e)
        {
            ForceOvertake(e.index, (int)e.X);
        }

        private void ForceOvertake(int driverIndex, int lapNumber)
        {
            forcedOvertake[driverIndex, lapNumber - 1] = true;
            ResetParameters(originalStrategies);
            SetupGrid();
            SimulateRace();
        }

        public Race() { }

        void MyEvents_SettingsModified()
        {
            CheckUpdate();
        }

        /// <summary>
        /// Sorts the strategies by pace to simulate a qualifying setup.
        /// </summary>
        public void SetupGrid()
        {
            Functions.QuickSort(ref raceStrategies, 0, raceStrategies.Length - 1, (a, b) => a.GridPosition > b.GridPosition);
        }

        /// <summary>
        /// Runs a race simulation and outputs the results to a graph.
        /// </summary>
        public void SimulateRace()
        {
            //overtake variables:
            Driver driverAhead, driverBehind;
            RaceStrategy temp;
            int[] pitStops = new int[Data.NumberOfDrivers];

            for (int lapNumber = 0; lapNumber < laps; lapNumber++) //for each lap
            {
                for (int position = 0; position < Data.NumberOfDrivers; position++)
                {
                    //check for lapped car encounters:
                    bool stayInLoop = true;
                    float previousLapDeltaTime = 0;
                    float thisLapDeltaTime = 0;
                    for (int lappedPosition = Data.NumberOfDrivers - 1; stayInLoop; lappedPosition--)
                    {
                        previousLapDeltaTime = (raceStrategies[lappedPosition].CumulativeTime - raceStrategies[position].CumulativeTime);
                        thisLapDeltaTime = (raceStrategies[lappedPosition].RaceLapTimes[lapNumber] - raceStrategies[position].RaceLapTimes[lapNumber]);
                        if ((previousLapDeltaTime % raceStrategies[position].RaceLapTimes[lapNumber]) < thisLapDeltaTime)
                        {
                            raceStrategies[lappedPosition].RaceLapTimes[lapNumber] += Data.Settings.BackmarkerLoss;
                        }

                        stayInLoop = (previousLapDeltaTime > raceStrategies[position].RaceLapTimes[lapNumber]);
                    }
                }

                for (int position = 1; position < raceStrategies.Length; position++)
                {
                    //check for overtakes:
                    driverAhead = raceStrategies[position - 1].Driver;
                    driverBehind = raceStrategies[position].Driver;
                    if (CalculateOvertake(driverAhead, driverBehind, position, lapNumber) || forcedOvertake[driverBehind.DriverIndex,lapNumber])
                    {
                        raceStrategies[position - 1].RaceLapTimes[lapNumber] += Data.Settings.TimeLoss;
                        //swap strategies
                        temp = raceStrategies[position - 1];
                        raceStrategies[position - 1] = raceStrategies[position];
                        raceStrategies[position] = temp;
                    }
                    else
                    {
                        //check for following in traffic
                        if (FollowingInTraffic(position - 1, position, lapNumber))
                        {
                            float minimumTime = raceStrategies[position - 1].CumulativeTime + raceStrategies[position - 1].RaceLapTimes[lapNumber] + Data.Settings.TimeGap;
                            float requiredLap = minimumTime - raceStrategies[position].CumulativeTime;
                            raceStrategies[position].RaceLapTimes[lapNumber] = requiredLap;
                        }
                    }
                }

                List<PitStop> pitStopList = new List<PitStop>();
                for (int position = 0; position < raceStrategies.Length; position++)
                {
                    //update pit stops:
                    if (raceStrategies[position].PitStops.Exists(lapNumber + 1))
                    {
                        pitStopList.Add(new PitStop(position, lapNumber, raceStrategies[position].PaceParameters.PitStopLoss));
                        pitStops[raceStrategies[position].DriverIndex]++;
                    }
                }

                foreach (PitStop p in pitStopList)
                {
                    p.SimulateStop(ref raceStrategies);
                }

                PitStop.UpdateRacePositionsAfterPitStop(ref raceStrategies, pitStopList);

                for (int position = 0; position < raceStrategies.Length; position++)
                {
                    //update the times:
                    raceStrategies[position].CumulativeTime += raceStrategies[position].RaceLapTimes[lapNumber];

                    //update the positions:
                    positions[position, lapNumber].driver = raceStrategies[position].Driver.DriverIndex;
                    positions[position, lapNumber].position = position;
                    positions[position, lapNumber].cumulativeTime = raceStrategies[position].CumulativeTime;
                    positions[position, lapNumber].pitStopsBeforeLap = pitStops[raceStrategies[position].DriverIndex];
                    if (position == 0)
                    {
                        positions[position, lapNumber].gap = 0;
                        positions[position, lapNumber].interval = lapNumber;
                    }
                    else
                    {
                        positions[position, lapNumber].gap = GetGapBetweenCars(0, position, lapNumber);
                        positions[position, lapNumber].interval = GetGapBetweenCars(position - 1, position, lapNumber);
                    }
                }
            } //end for each lap
            StartGraph();
        }

        float GetGapBetweenCars(int aheadPosition, int behindPosition, int lap)
        {
            float rawValueGap;

            if (aheadPosition < 0) { rawValueGap = 0F; }
            else { rawValueGap = raceStrategies[behindPosition].CumulativeTime - raceStrategies[aheadPosition].CumulativeTime; }

            return rawValueGap;
        }

        bool CalculateOvertake(Driver driverAhead, Driver driverBehind, int positionBehind, int lap)
        {
            Random rand = new Random(0);
            bool overtake;
            float probability;

            float cumulativeTimeDelta = raceStrategies[positionBehind].CumulativeTime - raceStrategies[positionBehind-1].CumulativeTime;
            float paceDelta = raceStrategies[positionBehind].RaceLapTimes[lap] - raceStrategies[positionBehind-1].RaceLapTimes[lap];
            float totalDelta = cumulativeTimeDelta + paceDelta;
            float speedDelta = driverBehind.PaceParameters.PaceParameters[PaceParameterType.TopSpeed] - driverAhead.PaceParameters.PaceParameters[PaceParameterType.TopSpeed];

            probability = GetOvertakeProbability(paceDelta, speedDelta, totalDelta, Data.Settings.RequiredPaceDelta, Data.Settings.RequiredSpeedDelta);

            float random = (float)rand.Next(0, 1001) / 1000F;
            overtake = (probability > random);

            return overtake;
        }

        /// <summary>
        /// Gets the probability that an overtake will occur
        /// </summary>
        /// <param name="paceDelta">The pace of the first car - pace of second car</param>
        /// <param name="speedDelta">Speed of first car - speed of second car</param>
        /// <param name="totalDelta">Total time gap between the two cars</param>
        /// <param name="requiredPaceDelta">Required pace delta to make an overtake possible</param>
        /// <param name="requiredSpeedDelta">Required speed delta to make an overtake possible</param>
        /// <returns>The probability between 0 and 1 that an overtake occurs</returns>
        public float GetOvertakeProbability(float paceDelta, float speedDelta, float totalDelta, float requiredPaceDelta, float requiredSpeedDelta)
        {
            float speedProbability = 0;
            float paceProbability = 0;

            if (requiredPaceDelta > totalDelta && requiredPaceDelta < paceDelta)
            {
                paceProbability = (paceDelta - requiredPaceDelta) / Math.Abs(requiredPaceDelta);
            }
            else
            {
                paceProbability = 0;
            }

            if (requiredSpeedDelta < speedDelta)
            {
                speedProbability = (speedDelta - requiredSpeedDelta) / Math.Abs(requiredSpeedDelta);
            }
            else
            {
                speedProbability = 0;
            }

            float probability = speedProbability * paceProbability;

            return probability;
        }

        bool FollowingInTraffic(int aheadPosition, int behindPosition, int lapNumber)
        {
            float behindCumulativeTime = raceStrategies[behindPosition].CumulativeTime + raceStrategies[behindPosition].RaceLapTimes[lapNumber];
            float aheadCumulativeTime = raceStrategies[aheadPosition].CumulativeTime + raceStrategies[aheadPosition].RaceLapTimes[lapNumber];

            return ((behindCumulativeTime - aheadCumulativeTime) < Data.Settings.TimeGap);
        }

        float FindGap(int lapNumber, int driver)
        {
            if (lapNumber == 0)
            {
                return 0F;
            }
            else
            {
                for (int position = 0; position < Data.NumberOfDrivers; position++)
                {
                    if (positions[position, lapNumber-1].driver == driver)
                    {
                        return positions[position, lapNumber-1].gap;
                    }
                }
            }
            return 0F;
        }

        float FindTime(int lapNumber, int driver)
        {
            if (lapNumber == 0)
            {
                return 0F;
            }
            else
            {
                for (int position = 0; position < Data.NumberOfDrivers; position++)
                {
                    if (positions[position, lapNumber - 1].driver == driver)
                    {
                        return positions[position, lapNumber - 1].cumulativeTime;
                    }
                }
            }

            return 0F;
        }

        /// <summary>
        /// Creates the traces to be displayed and draws the graph of the race
        /// </summary>
        void StartGraph()
        {
            List<GraphLine> traces = new List<GraphLine>(Data.NumberOfDrivers);
            for (int driver = 0; driver < Data.NumberOfDrivers; driver++)
            {
                traces.Add(new GraphLine(driver, true, Data.Drivers[driver].LineColour));
            }

            DataPoint tempPoint = new DataPoint();
            for (int lapNumber = 0; lapNumber <= laps; lapNumber++)
            {
                for (int driver = 0; driver < Data.NumberOfDrivers; driver++)
                {
                    tempPoint.Y = FindTime(lapNumber, driver);
                    tempPoint.X = lapNumber;
                    tempPoint.index = driver;
                    tempPoint.cycles = 0;

                    traces[driver].DataPoints.Add(tempPoint);
                }
            }
            Program.DriverSelectPanel.SetPositionData(Positions);
            raceGraph.DrawGraph(traces, true, true);
        }

        void CheckUpdate()
        {
            string message = "Settings have been altered. This may affect the outcome of the race simulation." +
                "Restart simulation now?";
            string caption = "Restart Simulation";
            if (Functions.StartDialog(message, caption))
            {
                RestartSimulation(out raceGraph, originalStrategies);
            }
        }

        /// <summary>
        /// Restarts the race simulation when paramters have been changed
        /// </summary>
        /// <param name="graph">The new graph which is output when the race simulation has been completed</param>
        /// <param name="strategies">The list of strategies to be used in the race simulation</param>
        public void RestartSimulation(out NewGraph graph, RaceStrategy[] strategies)
        {
            ResetParameters(strategies);
            //raceGraph.SetRaceLaps(laps);
            raceGraph.GraphPanel.SetupAxes(laps, 0.05, 100, 0.5);
            graph = this.raceGraph;
            forcedOvertake = Functions.InitialiseArrayAtValue<bool>(Data.NumberOfDrivers, laps, false);
            SetupGrid();
            SimulateRace();
        }

        void ResetParameters(RaceStrategy[] strategies)
        {
            laps = Data.GetRaceLaps();

            originalStrategies = (RaceStrategy[])strategies.Clone();
            raceStrategies = (RaceStrategy[])strategies.Clone();

            foreach (RaceStrategy s in strategies)
            {
                s.CumulativeTime = 0F;
                s.UpdateStrategyParameters();
            }

            positions = new RacePosition[Data.NumberOfDrivers, laps];
        }

        /// <summary>
        /// Writes the race data to a CSV file, writing data about each position on a given lap,
        ///  then writing the next lap separated by a line
        /// </summary>
        void WriteRaceData()
        {
            StreamWriter w = new StreamWriter("Race.txt");

            string positionString = "";

            for (int lapNumber = 0; lapNumber < laps; lapNumber++)
            {
                for (int position = 0; position < raceStrategies.Length; position++)
                {
                    positionString = Convert.ToString(positions[position, lapNumber].position);
                    positionString += ',';
                    positionString += Convert.ToString(positions[position, lapNumber].driver);
                    positionString += ',';
                    positionString += Convert.ToString(Math.Round(positions[position, lapNumber].gap,3));
                    positionString += ',';
                    positionString += Convert.ToString(Math.Round(positions[position, lapNumber].interval, 3));

                    w.WriteLine(positionString);
                }
                w.WriteLine();
            }
            w.Dispose();
        }

        /// <summary>
        /// Writes interval data from the race to separate files
        /// </summary>
        void writeIntervalData()
        {
            StreamWriter intervals = new StreamWriter("intervals.txt");
            StreamWriter gaps = new StreamWriter("gaps.txt");
            StreamWriter drivers = new StreamWriter("driverOrder.txt");

            string intervalString = "";
            string gapString = "";
            string driverString = "";

            for (int lapNumber = 0; lapNumber < laps; lapNumber++)
            {
                for (int position = 0; position < raceStrategies.Length; position++)
                {
                    intervalString = Convert.ToString(Math.Round(positions[position, lapNumber].interval, 3));
                    if (position != Strategies.Length - 1)
                        intervalString += ',';

                    gapString = Convert.ToString(Math.Round(positions[position, lapNumber].gap, 3));
                    if (position != Strategies.Length - 1)
                        gapString += ',';

                    driverString = Convert.ToString(positions[position, lapNumber].driver);
                    if (position != Strategies.Length - 1)
                        driverString += ',';

                    intervals.Write(intervalString);
                    gaps.Write(gapString);
                    drivers.Write(driverString);
                }
                intervals.WriteLine();
                gaps.WriteLine();
                drivers.WriteLine();
            }

            intervals.Dispose();
            gaps.Dispose();
            drivers.Dispose();
        }

        public void Dispose()
        {
            raceGraph.Dispose();
        }

        /// <summary>
        /// Gets or sets the list of strategies in the race
        /// </summary>
        public RaceStrategy[] Strategies
        {
            get { return raceStrategies; }
            set { raceStrategies = value; }
        }

        /// <summary>
        /// Gets the data from each race position
        /// </summary>
        internal RacePosition[,] Positions
        { get { return positions; } }
    }
}
