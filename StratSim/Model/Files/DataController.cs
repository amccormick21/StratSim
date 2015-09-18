
using DataSources.DataConnections;
using System.Collections.Generic;
namespace StratSim.Model.Files
{
    /// <summary>
    /// Controls the loading of data from a file and the processing of the data
    /// in the file
    /// Can be made private once testing is complete.
    /// </summary>
    public class DataController
    {
        public Dictionary<Session, bool> sessionDataLoaded = new Dictionary<Session, bool>();

        public DataController()
        { }

        /// <summary>
        /// Controls the processing of data loaded from a timing data file.
        /// </summary>
        /// <param name="data">The complete file contents</param>
        /// <param name="fileType">The type of data contained by the file</param>
        public void ProcessData(string data, Session session, int raceIndex)
        {
            ISessionData dataToAnalyse = GetDataType(data, session.GetTimingDataType());

            dataToAnalyse.AnalyseData(session);
            dataToAnalyse.WriteArchiveData(session, raceIndex);
            dataToAnalyse.UpdateDatabaseWithData(session, raceIndex);

            sessionDataLoaded[session] = true;
        }

        /// <summary>
        /// Gets the type of data that is to be loaded from the given file type
        /// </summary>
        /// <param name="data">The loaded data string that will be used to generate information</param>
        /// <param name="fileType">The type of file that was loaded</param>
        /// <returns>A populated instance of the correct class for the type of data that was passed</returns>
        public ISessionData GetDataType(string data, TimingDataType fileType)
        {
            switch (fileType)
            {
                case TimingDataType.LapTimeData:
                    return new LapData(data);
                case TimingDataType.SpeedData:
                    return new SpeedData(data);
                case TimingDataType.GridData:
                    return new GridData(data);
                case TimingDataType.HistoryData:
                    return new HistoryData(data);
                default:
                    return new LapData();
            }
        }

        public ISessionData GetDataType(TimingDataType fileType)
        {
            switch (fileType)
            {
                case TimingDataType.LapTimeData:
                    return new LapData();
                case TimingDataType.SpeedData:
                    return new SpeedData();
                case TimingDataType.GridData:
                    return new GridData();
                case TimingDataType.HistoryData:
                    return new HistoryData();
                default:
                    return new LapData();
            }

        }
    }
}
