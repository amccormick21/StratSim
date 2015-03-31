using DataSources.DataConnections;
using MyFlowLayout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStats.MyFlowLayout;

namespace TeamStats.Statistics
{
    public interface ISingleSessionStatistic : IStatistic
    {
        void SetSession(Session session);
        void DisplayStatistic(TeamStatsPanelControlEvents events, MainForm form, Session session);
        Session DisplaySession { get; }
    }
}
