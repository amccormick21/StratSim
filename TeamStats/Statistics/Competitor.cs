using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamStats.Statistics
{
    public enum Competitor
    {
        Driver,
        Comparison,
        Team
    }

    public static class Competitors
    {
        public static Competitor[] All()
        {
            return new Competitor[] { Competitor.Driver, Competitor.Comparison, Competitor.Team };
        }
    }
}
