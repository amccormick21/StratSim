using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSources.DataConnections
{
    public enum TimingDataType
    {
        LapTimeData = 0,
        SpeedData = 1,
        GridData = 2,
        HistoryData = 3
    }

    public static class TimingDataTypeExtensionMethods
    {
        public static string GetPDFFileName(this TimingDataType timingDataType, Session session)
        {
            if (session == Session.Race)
            {
                return "Lap Analysis";
            }
            switch (timingDataType)
            {
                case TimingDataType.LapTimeData:
                    return "Lap Times";
                case TimingDataType.SpeedData:
                    return "Speed Trap";
                case TimingDataType.GridData:
                    return "Preliminary Classification";
                case TimingDataType.HistoryData:
                    return "History Chart";
                default:
                    return "";
            }
        }

        public static string GetTextFileName(this TimingDataType timingDataType)
        {
            switch (timingDataType)
            {
                case TimingDataType.LapTimeData:
                    return "Lap Times";
                case TimingDataType.SpeedData:
                    return "Speed Trap";
                case TimingDataType.GridData:
                    return "Grid";
                case TimingDataType.HistoryData:
                    return "History Chart";
                default:
                    return "";
            }
        }
    }
}
