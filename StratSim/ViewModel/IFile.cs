using DataSources.DataConnections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StratSim.ViewModel
{
    /// <summary>
    /// Interface for any class that controls data input and output.
    /// </summary>
    /// <typeparam name="T">The type of data that is controlled by the class</typeparam>
    public interface IFileController<T>
    {
        void ReadFromFile(string fileName);
        void WriteToFile(string fileName);
        T FileData { get; }
    }
}
