using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StratSim.Model
{
    public class PracticeTimesCollection
    {
        List<Stint>[] practiceSessionStints;
        float topSpeed;
        int gridPosition;

        public PracticeTimesCollection(List<Stint>[] PracticeSessionStints, float TopSpeed, int GridPosition)
        {
            this.PracticeSessionStints = PracticeSessionStints;
            this.TopSpeed = TopSpeed;
            this.GridPosition = GridPosition;
        }

        public float GetFastestLapInSession(int sessionIndex)
        {
            float fastestLapHolder = 0;
            float fastestLap;

            for (int lapIndex = 0; lapIndex < practiceSessionStints[sessionIndex].Count; lapIndex++)
            {
                if (lapIndex == 0)
                {
                    fastestLapHolder = practiceSessionStints[sessionIndex][lapIndex].FastestLap();
                }
                else
                {
                    fastestLap = practiceSessionStints[sessionIndex][lapIndex].FastestLap();
                    if (fastestLap < fastestLapHolder)
                    {
                        fastestLapHolder = fastestLap;
                    }
                }
            }
            return fastestLapHolder;
        }

        public List<Stint>[] PracticeSessionStints
        {
            get { return practiceSessionStints; }
            set { practiceSessionStints = value; }
        }
        public float TopSpeed
        {
            get { return topSpeed; }
            set { topSpeed = value; }
        }
        /// <summary>
        /// Gets or sets the zero based grid position of a driver for the race.
        /// </summary>
        public int GridPosition
        {
            get { return gridPosition; }
            set { gridPosition = value; }
        }
    }
}
