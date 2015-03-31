
namespace StratSim.Model
{
    public class RaceStrategy : Strategy
    {
        Driver driver;
        float cumulativeTime;
        float[] raceLapTimes;
        int gridPosition;

        public RaceStrategy(Strategy Strategy, int GridPosition)
            : base(Strategy)
        {
            driver = Data.Drivers[base.DriverIndex];
            cumulativeTime = 0;
            raceLapTimes = SetLapTimes(false);
            this.gridPosition = GridPosition;
        }

        public Driver Driver
        {
            get { return driver; }
            set { driver = value; }
        }

        public float CumulativeTime
        {
            get { return cumulativeTime; }
            set { cumulativeTime = value; }
        }

        public float[] RaceLapTimes
        {
            get { return raceLapTimes; }
            set { raceLapTimes = value; }
        }

        public int GridPosition
        {
            get { return gridPosition; }
            set { gridPosition = value; }
        }
    }
}
