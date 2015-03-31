using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamStats.Statistics
{
    public enum SortType { Index, Percentage, Value };

    public class CannortSortByThisParameterException : Exception
    {
        public SortType FailedSortType { get; set; }
        public string MessageBoxString { get; set; }

        public CannortSortByThisParameterException(SortType failedSortType)
        {
            this.FailedSortType = failedSortType;
            this.MessageBoxString = "Cannot sort by " + Convert.ToString(failedSortType);
            new InvalidOperationException();
        }
    }
}
