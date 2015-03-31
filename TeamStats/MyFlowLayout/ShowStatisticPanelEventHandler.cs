using MyFlowLayout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamStats.Statistics;

namespace TeamStats.MyFlowLayout
{
    public delegate void ShowStatisticPanelEventHandler<TStatisticInterface>(MainForm form, TStatisticInterface statisticInterface, Competitor[] tabsToShow);
}
