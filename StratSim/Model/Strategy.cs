using DataSources;
using StratSim.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StratSim.Model
{
    /// <summary>
    /// <para>Contains data about a race strategy, including list of stints making up the strategy, 
    /// the driver completing the stint, and the full list of lap times used for simulating the race.</para>
    /// <para>Contains methods for optimising the strategy and manipulating the stints within the strategy
    /// once it has been created.</para>
    /// </summary>
    public class Strategy : ICloneable
    {
        int driverIndex;
        PaceParameterCollection paceParameters;
        Track t;
        List<Stint> listOfStints = new List<Stint>();
        float[] lapTimes;
        float totalTime;
        int noOfStints;
        int lapsInRace;
        int optionLaps;
        int primeLaps;
        List<int> pitStops = new List<int>();

        /// <summary>
        /// Starts a strategy from driver data and stint information and optimises the strategy
        /// </summary>
        /// <param name="NoOfStints">The number of stints to complete</param>
        /// <param name="PrimeStints">The number of prime tyre stints to be completed</param>
        /// <param name="_driverIndex">The index of the driver who is completing the stint</param>
        public Strategy(int NoOfStints, int PrimeStints, PaceParameterCollection PaceParameters, int DriverIndex)
        {
            this.DriverIndex = DriverIndex;
            int primeStints = PrimeStints;
            int optionStints = NoOfStints - PrimeStints;

            int trackIndex = Data.RaceIndex;

            t = Data.Tracks[trackIndex];
            this.PaceParameters = PaceParameters;

            lapsInRace = Data.GetRaceLaps();
            lapTimes = new float[lapsInRace];

            SetStintLengths(lapsInRace, primeStints, optionStints);

            SetupStrategyStints(NoOfStints, primeStints);
            PopulateAllStints();
            UpdateStrategyParameters();
        }
        /// <summary>
        /// Starts a strategy linked to a driver with a list of pre-determined stints to load
        /// </summary>
        public Strategy(List<Stint> _stints)
        {
            lapsInRace = Data.GetRaceLaps();

            listOfStints = new List<Stint>(_stints);

            foreach (Stint s in listOfStints)
            {
                if (s.startLap != 0) { pitStops.Add(s.startLap - 1); }
            }

            t = Data.CurrentTrack;

            PopulateAllStints();
            UpdateStrategyParameters();
        }

        public Strategy()
        { }


        /// <summary>
        /// Copy constructor for the strategy class.
        /// </summary>
        public Strategy(Strategy s)
        {
            t = s.t;
            driverIndex = s.DriverIndex;
            lapsInRace = s.lapsInRace;
            optionLaps = s.optionLaps;
            primeLaps = s.primeLaps;
            this.PaceParameters = s.PaceParameters;

            foreach (Stint stint in s.Stints)
            {
                listOfStints.Add((Stint)stint.Clone());
            }

            PopulateAllStints();
            UpdateStrategyParameters();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        /// Sets the optimum prime and option stint lengths for the stint
        /// </summary>
        /// <param name="lapsInRace">The number of laps in the race being simulated</param>
        /// <param name="noOfOptionStints">The number of stints to be completed on the prime tyre</param>
        /// <param name="noOfPrimeStints">The number of stints to be completed on the option tyre</param>
        public void SetStintLengths(int lapsInRace, int noOfPrimeStints, int noOfOptionStints)
        {
            float optimumPrimeLength = OptimisePrime(this.PaceParameters, noOfPrimeStints, noOfOptionStints, lapsInRace);
            float ratio = (float)noOfPrimeStints / (float)(noOfPrimeStints + noOfOptionStints);
            primeLaps = RoundValueToIntegerBasedOnRatio(optimumPrimeLength, ratio);
            if (primeLaps >= lapsInRace)
                primeLaps = lapsInRace - 1;
            else if (primeLaps <= 0)
                primeLaps = 1;
            optionLaps = (lapsInRace - (noOfPrimeStints * primeLaps)) / (noOfOptionStints);
        }


        /// <returns>The race time for the strategy</returns>
        float GetStrategyTime()
        {
            float totalStrategyTime = 0;

            foreach (Stint s in listOfStints)
            {
                totalStrategyTime += s.TotalTime();
            }

            return totalStrategyTime;
        }

        /// <summary>
        /// Sets up the empty stints for the strategy
        /// Sets option stints before prime stints by default
        /// </summary>
        /// <param name="numberOfStints">The number of stints to do in the race</param>
        /// <param name="numberOfPrimeStints">The number of stints to do on the prime tyre0</param>
        void SetupStrategyStints(int numberOfStints, int numberOfPrimeStints)
        {
            Stint tempStint;
            int lapsThroughRace = 0;
            int stintLength = 0;

            //Populates option stints before prime stints
            for (int i = numberOfPrimeStints; i < numberOfStints; i++)
            {
                tempStint = new Stint(lapsThroughRace, TyreType.Option, optionLaps);
                lapsThroughRace += optionLaps;
                listOfStints += tempStint;
            }
            for (int i = 0; i < numberOfPrimeStints; i++)
            {
                if (i == (numberOfPrimeStints - 1)) { stintLength = t.laps - lapsThroughRace; }
                else { stintLength = primeLaps; }
                tempStint = new Stint(lapsThroughRace, TyreType.Prime, stintLength);
                lapsThroughRace += primeLaps;
                listOfStints += tempStint;
            }
        }

        /// <summary>
        /// Populates the stints of the strategy with lap times
        /// Populates the list of pit stops
        /// </summary>
        void PopulateAllStints()
        {
            int lapsThroughRace = 0;
            int stintIndex = 0;
            int totalStints = listOfStints.Count - 1;
            pitStops.Clear();

            for (stintIndex = 0; stintIndex <= totalStints; stintIndex++)
            {
                listOfStints[stintIndex].modified = true;
                if (stintIndex != 0)
                    pitStops.Add(lapsThroughRace);
                listOfStints[stintIndex] = PopulateSingleStint(listOfStints[stintIndex], ref lapsThroughRace);
            }
        }

        /// <summary>
        /// Populates a stint with lap times
        /// </summary>
        /// <param name="lapsThroughRace">The race lap on which the stint will start. It is updated within the method</param>
        /// <returns>The populated stint</returns>
        Stint PopulateSingleStint(Stint stintToPopulate, ref int lapsThroughRace)
        {
            float degradation;
            float lapTime;
            float tyreDelta = 0;

            stintToPopulate.lapTimes.Clear();
            degradation = 0;

            if (stintToPopulate.tyreType == TyreType.Prime)
            {
                if (!stintToPopulate.modified)
                    lapsThroughRace += primeLaps;
                degradation = this.PaceParameters.PaceParameters[PaceParameterType.PrimeDegradation];
            }
            else
            {
                if (!stintToPopulate.modified)
                    lapsThroughRace += optionLaps;
                tyreDelta = this.PaceParameters.PaceParameters[PaceParameterType.TyreDelta];
                degradation = this.PaceParameters.PaceParameters[PaceParameterType.OptionDegradation];
            }

            if (stintToPopulate.modified && (stintToPopulate.stintLength != 0))
            { lapsThroughRace += stintToPopulate.stintLength; }

            float fuelAddedTime;
            float fuelTimePerLap = this.PaceParameters.PaceParameters[PaceParameterType.FuelConsumption] * this.PaceParameters.PaceParameters[PaceParameterType.FuelEffect];

            int lap;
            int stintLength = stintToPopulate.stintLength;
            for (lap = 0; lap < stintLength; lap++)
            {
                fuelAddedTime = (lapsInRace - stintToPopulate.startLap - lap) * (fuelTimePerLap);
                lapTime = this.PaceParameters.PaceParameters[PaceParameterType.Pace];
                lapTime += tyreDelta;
                lapTime += fuelAddedTime;
                lapTime += (lap * degradation);
                AddLapToStint(stintToPopulate, lapTime);
            }

            if (lapsThroughRace != lapsInRace)
            {
                stintToPopulate.lapTimes[lap - 1] += this.PaceParameters.PitStopLoss;
            }

            return stintToPopulate;
        }

        /// <summary>
        /// Sets the total strategy time and the list of lap times used for the strategy,
        /// and calculates the number of stints in the strategy.
        /// Sorts the pit stops using a quicksort.
        /// </summary>
        public void UpdateStrategyParameters()
        {
            totalTime = GetStrategyTime();
            lapTimes = SetLapTimes(true);
            noOfStints = listOfStints.Count();

            //Sort pit stops
            int[] pitStopList = pitStops.ToArray<int>();
            Functions.QuickSort(ref pitStopList, 0, pitStopList.Length - 1, ((a, b) => a > b));
            pitStops = new List<int>(pitStopList);
        }

        /// <summary>
        /// Populates an array of lap times for every lap of the race
        /// Pulls data from the stints to create a one dimensional array
        /// </summary>
        /// <param name="pitStopsIncluded">Represents whether pit stop times are included in the lap times</param>
        /// <returns>An array, the length of the race, containing the lap times for the entire race.</returns>
        internal float[] SetLapTimes(bool pitStopsIncluded)
        {
            float[] times = new float[lapsInRace];
            int lapNumber = 0;

            foreach (Stint s in listOfStints)
            {
                foreach (float lap in s.lapTimes)
                {
                    times[lapNumber++] = lap;
                }
                if (!pitStopsIncluded)
                {
                    if (lapNumber != lapsInRace) { times[lapNumber - 1] -= this.PaceParameters.PitStopLoss; }
                }
            }

            return times;
        }

        float OptimisePrime(PaceParameterCollection PaceParameters, int primeStints, int optionStints, int lapsInRace)
        {
            int l = lapsInRace;
            float optionD = PaceParameters.PaceParameters[PaceParameterType.OptionDegradation];
            float primeD = PaceParameters.PaceParameters[PaceParameterType.PrimeDegradation];
            float delta = PaceParameters.PaceParameters[PaceParameterType.TyreDelta];
            float optimumLength = 0;

            optimumLength += (l * optionD / optionStints) + (delta) + (primeD / 2) - (optionD / 2);
            optimumLength /= ((primeD) + (optionD * primeStints / optionStints));

            return optimumLength;
        }

        void AddLapToStint(Stint stintToAdd, float lapTime)
        {
            stintToAdd += lapTime;
            stintToAdd.stintLength--;
        }

        /// <summary>
        /// Decides if it is optimal to round a stint up or down
        /// </summary>
        /// <param name="value">The value to round</param>
        /// <param name="ratio">The ratio below which the value will be rounded down</param>
        int RoundValueToIntegerBasedOnRatio(float value, float ratio)
        {
            float remainder = (value % 1);
            bool roundUp = (remainder > ratio);
            int roundedLength;

            roundedLength = (int)(value - remainder);
            if (roundUp) { ++roundedLength; }

            return roundedLength;
        }

        /// <summary>
        /// Changes the length of a stint in the strategy
        /// </summary>
        /// <returns>The full list of stints in the new strategy</returns>
        public List<Stint> ChangeStintLength(int stintToChange, int newLength)
        {
            List<Stint> newListOfStints = listOfStints;

            int originalLength, lengthChange;
            originalLength = newListOfStints[stintToChange].stintLength;
            lengthChange = newLength - originalLength;

            if (stintToChange == noOfStints - 1)
            {
                newListOfStints[stintToChange].stintLength += lengthChange;
                newListOfStints[stintToChange - 1].stintLength -= lengthChange;

                pitStops.Remove(newListOfStints[stintToChange].startLap);
                newListOfStints[stintToChange].startLap += lengthChange;
                pitStops.Add(newListOfStints[stintToChange].startLap);

                int lapsThroughRace = newListOfStints[stintToChange - 1].startLap;

                newListOfStints[stintToChange - 1] = PopulateSingleStint(newListOfStints[stintToChange - 1], ref lapsThroughRace);
                newListOfStints[stintToChange] = PopulateSingleStint(newListOfStints[stintToChange], ref lapsThroughRace);
            }

            else
            {
                newListOfStints[stintToChange].stintLength += lengthChange;
                newListOfStints[stintToChange + 1].stintLength -= lengthChange;

                pitStops.Remove(newListOfStints[stintToChange + 1].startLap);
                newListOfStints[stintToChange + 1].startLap += lengthChange;
                pitStops.Add(newListOfStints[stintToChange + 1].startLap);

                int lapsThroughRace = newListOfStints[stintToChange].startLap;

                newListOfStints[stintToChange] = PopulateSingleStint(newListOfStints[stintToChange], ref lapsThroughRace);
                newListOfStints[stintToChange + 1] = PopulateSingleStint(newListOfStints[stintToChange + 1], ref lapsThroughRace);
            }

            return newListOfStints;
        }

        public void ChangeStintTyreType(int stintToChange, TyreType newTyreType)
        {
            listOfStints[stintToChange].tyreType = newTyreType;

            int lapsThroughRace = listOfStints[stintToChange].startLap;
            listOfStints[stintToChange] = PopulateSingleStint(Stints[stintToChange], ref lapsThroughRace);

            UpdateStrategyParameters();
            MyEvents.OnStrategyModified(Data.Drivers[driverIndex], this, false);
        }

        public List<Stint> AddPitStop(int lap)
        {
            List<Stint> newListOfStints = new List<Stint>();

            int splitStintIndex = listOfStints.FindLastIndex(s => s.startLap < lap - 1);
            if (splitStintIndex >= 0)
            {
                Stint stintToSplit = listOfStints[splitStintIndex];

                int totalStints = listOfStints.Count;
                int lapsThroughRace = 0;

                Stint[] splitStint = stintToSplit.Split(lap);

                //re-populate strategy:
                int stintIndex = 0;
                while (stintIndex < splitStintIndex)
                {
                    newListOfStints += listOfStints[stintIndex];
                    lapsThroughRace += listOfStints[stintIndex].stintLength;
                    stintIndex++;
                }

                foreach (Stint s in splitStint)
                {
                    newListOfStints += PopulateSingleStint(s, ref lapsThroughRace);
                }

                stintIndex++;

                while (stintIndex < totalStints)
                {
                    newListOfStints += listOfStints[stintIndex];
                    lapsThroughRace += listOfStints[stintIndex].stintLength;
                    stintIndex++;
                }

                pitStops.Add(lap);
            }
            else
            {
                newListOfStints = listOfStints;
            }

            return newListOfStints;
        }

        public List<Stint> RemovePitStop(int lap)
        {
            List<Stint> newListOfStints = new List<Stint>();
            Stint mergedStint;

            int removeStintIndex = listOfStints.FindIndex(s => s.startLap == lap);

            if (removeStintIndex >= 0)
            {
                if (removeStintIndex == 0)
                {
                    mergedStint = Stint.Merge(listOfStints[removeStintIndex], listOfStints[removeStintIndex + 1]);
                }
                else
                {
                    mergedStint = Stint.Merge(listOfStints[removeStintIndex - 1], listOfStints[removeStintIndex]);
                }

                int totalStints = listOfStints.Count;
                int lapsThroughRace = 0;

                int stintIndex = 0;
                while (stintIndex < removeStintIndex - 1)
                {
                    newListOfStints += listOfStints[stintIndex];
                    lapsThroughRace += listOfStints[stintIndex].stintLength;
                    stintIndex++;
                }

                newListOfStints += PopulateSingleStint(mergedStint, ref lapsThroughRace);
                stintIndex += 2;

                while (stintIndex < totalStints)
                {
                    newListOfStints += listOfStints[stintIndex];
                    lapsThroughRace += listOfStints[stintIndex].stintLength;
                    stintIndex++;
                }

                pitStops.Remove(lap);
            }
            else
            {
                newListOfStints = listOfStints;
            }

            return newListOfStints;
        }

        public int GetNearestPitStop(int lapNumber, int precision, out bool withinRange)
        {
            int nearestPitStopLap = -1;
            withinRange = false;

            foreach (int stop in pitStops)
            {
                if (stop + precision >= lapNumber && stop - precision <= lapNumber)
                {
                    withinRange = true;
                    nearestPitStopLap = stop;
                }
            }

            return nearestPitStopLap;
        }

        public void SwapStints(int stintIndexA, int stintIndexB)
        {
            Stint stintA = listOfStints[stintIndexA];
            Stint stintB = listOfStints[stintIndexB];

            int lapsThroughRace = stintA.startLap;

            Stint.Swap(ref stintA, ref stintB);

            listOfStints[stintIndexA] = PopulateSingleStint(stintA, ref lapsThroughRace);
            listOfStints[stintIndexB] = PopulateSingleStint(stintB, ref lapsThroughRace);

        }

        public List<Stint> Stints
        {
            get { return listOfStints; }
            set { listOfStints = value; }
        }

        public PaceParameterCollection PaceParameters
        {
            get { return paceParameters; }
            set { paceParameters = value; }
        }

        public int DriverIndex
        {
            get { return driverIndex; }
            private set { driverIndex = value; }
        }

        public int NoOfStints
        {
            get { return noOfStints; }
            set { noOfStints = value; }
        }

        public int NoOfStops
        {
            get { return noOfStints - 1; }
        }

        public float TotalTime
        {
            get { return totalTime; }
        }

        public float[] LapTimes
        {
            get { return lapTimes; }
            set { lapTimes = value; }
        }


        public int PrimeLaps
        { get { return primeLaps; } }
        public int OptionLaps
        { get { return optionLaps; } }

        public float FuelUsed
        { get { return lapsInRace * PaceParameters.PaceParameters[PaceParameterType.FuelConsumption]; } }

        public List<int> PitStops
        {
            get { return pitStops; }
            set { pitStops = value; }
        }
    }
}
