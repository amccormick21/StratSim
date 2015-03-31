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
    public interface IDualSessionStatistic : IStatistic
    {
        void SetSessions(Session firstSession, Session secondSession);
        void SetFirstSession(Session firstSession);
        void SetSecondSession(Session secondSession);
        void DisplayStatistic(TeamStatsPanelControlEvents events, MainForm form, Session firstSession, Session secondSession);
        Session FirstSession { get; }
        Session SecondSession { get; }
    }
}
