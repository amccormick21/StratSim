using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamStats.Statistics
{
    public enum DisplayType
    {
        Value,
        Percentage
    }

    public class CannortDisplayByThisParameterException : Exception
    {
        public DisplayType FailedDisplayType { get; set; }
        public string MessageBoxString { get; set; }

        public CannortDisplayByThisParameterException(DisplayType failedDisplayType)
        {
            this.FailedDisplayType = failedDisplayType;
            this.MessageBoxString = "Cannot display by " + Convert.ToString(failedDisplayType);
            new InvalidOperationException();
        }
    }
}
