using StratSim.ViewModel;
using DataSources.DataConnections;

namespace StratSim.Model.Files
{
    /// <summary>
    /// Specifies that the class has methods for interpreting and writing data
    /// loaded from PDF files
    /// </summary>
    public interface ISessionData : IFileController<string[]>
    {
        void AnalyseData(Session session);
        void WriteArchiveData(Session session, int raceIndex);
        void RetrieveArchiveData(Session session, int raceIndex);
        void UpdateDatabaseWithData(Session session, int raceIndex);
    }
}