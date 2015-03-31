using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSources.DataConnections
{
    public class ResultsUpdatedEventArgs
    {
        public Result[,] results;
        public Session session;

        public ResultsUpdatedEventArgs(Result[,] results, Session session)
        {
            this.results = results;
            this.session = session;
        }
    }
}
