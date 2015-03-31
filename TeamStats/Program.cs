using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyFlowLayout;
using TeamStats.View;
using TeamStats.MyFlowLayout;

namespace TeamStats
{
    public static class Program
    {
        public static ContentTabControl ContentTabControl;

        /// <summary>
        /// The main entry point for the program.
        /// </summary>
        static void Main()
        {
            //Do Nothing
        }

        /// <summary>
        /// Starts the project and provides an IO controller for forms relating to this project.
        /// </summary>
        /// <returns>An instance of an IO controller used for forms for this project</returns>
        public static TeamStatsFormIOController StartProject()
        {
            Main();
            WindowFlowPanel MainPanel = new WindowFlowPanel();
            TeamStatsPanelControlEvents Events = new TeamStatsPanelControlEvents();
            TeamStatsMyToolbar Toolbar = new TeamStatsMyToolbar(MainPanel, Events);
            return new TeamStatsFormIOController(MainPanel, Toolbar, Events, "Team Stats");
        }
    }
}
