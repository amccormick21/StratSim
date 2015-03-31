using System.Collections.Generic;

namespace StratSim.Model
{
    public class PitStop
    {
        private int position, lapNumber;
        float pitStopLoss;

        public PitStop(int position, int lapNumber, float pitStopLoss)
        {
            this.position = position;
            this.lapNumber = lapNumber;
            this.pitStopLoss = pitStopLoss;
        }

        /// <summary>
        /// Updates timings in a race when a pit stop is made
        /// </summary>
        /// <param name="strategies">The array of strategies representing a race</param>
        public void SimulateStop(ref RaceStrategy[] strategies)
        {
            AddTimeDueToStop(ref strategies, position, lapNumber, pitStopLoss);
        }
        public void AddTimeDueToStop(ref RaceStrategy[] strategies, int position, int lapNumber, float pitStopTimeLoss)
        {
            strategies[position].RaceLapTimes[lapNumber] += pitStopTimeLoss;
        }

        /// <summary>
        /// Updates the positions when a pit stop is made using a simple insert sort.
        /// Caution, O(n) for the size of the race.
        /// </summary>
        /// <param name="strategies">The list of strategies representing a race</param>
        /// <param name="pitStops">The list of pit stops to process</param>
        /// <param name="trackIndex">The index of the track on which the race is taking place</param>
        public static void UpdateRacePositionsAfterPitStop(ref RaceStrategy[] strategies, List<PitStop> pitStops)
        {
            RaceStrategy temp;

            foreach (PitStop s in pitStops)
            {
                int overtakeDownTo = s.Position;

                while ((overtakeDownTo + 1 < strategies.Length) && (strategies[overtakeDownTo + 1].CumulativeTime < strategies[overtakeDownTo].CumulativeTime + strategies[overtakeDownTo].PaceParameters.PitStopLoss))
                {
                    temp = strategies[overtakeDownTo];
                    strategies[overtakeDownTo] = strategies[overtakeDownTo + 1];
                    strategies[overtakeDownTo + 1] = temp;

                    ++overtakeDownTo;

                    foreach (PitStop p in pitStops)
                    {
                        if (p.Position == overtakeDownTo) { --p.Position; }
                    }
                }

            }
        }

        /// <summary>
        /// Gets or sets the position from which a driver makes the pit stop
        /// </summary>
        public int Position
        {
            get { return position; }
            set { position = value; }
        }
        /// <summary>
        /// Gets the lap number on which a driver makes a pit stop
        /// </summary>
        public int LapNumber
        { get { return lapNumber; } }
    }
}
