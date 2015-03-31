using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamStats.MyStatistics
{
    public enum PointScoringSystem
    {
        Post1990,
        Post2002,
        Post2009
    };

    public static class PointScoringSystemExtensionMethods
    {
        public static int GetPoints(this PointScoringSystem pointSystem, int position, bool doublePoints)
        {
            int pointsScored = 0;

            switch (pointSystem)
            {
                case PointScoringSystem.Post2009:
                    switch (position)
                    {
                        case 1: pointsScored = 25; break;
                        case 2: pointsScored = 18; break;
                        case 3: pointsScored = 15; break;
                        case 4: pointsScored = 12; break;
                        case 5: pointsScored = 10; break;
                        case 6: pointsScored = 8; break;
                        case 7: pointsScored = 6; break;
                        case 8: pointsScored = 4; break;
                        case 9: pointsScored = 2; break;
                        case 10: pointsScored = 1; break;
                        default: pointsScored = 0; break;
                    }
                    break;
                case PointScoringSystem.Post2002:
                    switch (position)
                    {
                        case 1: pointsScored = 10; break;
                        case 2: pointsScored = 8; break;
                        case 3: pointsScored = 6; break;
                        case 4: pointsScored = 5; break;
                        case 5: pointsScored = 4; break;
                        case 6: pointsScored = 3; break;
                        case 7: pointsScored = 2; break;
                        case 8: pointsScored = 1; break;
                        default: pointsScored = 0; break;
                    }
                    break;
                case PointScoringSystem.Post1990:
                    switch (position)
                    {
                        case 1: pointsScored = 10; break;
                        case 2: pointsScored = 6; break;
                        case 3: pointsScored = 4; break;
                        case 4: pointsScored = 3; break;
                        case 5: pointsScored = 2; break;
                        case 6: pointsScored = 1; break;
                        default: pointsScored = 0; break;
                    }
                    break;
            }

            if (doublePoints)
                pointsScored *= 2;

            return pointsScored;
        }

    }
}
