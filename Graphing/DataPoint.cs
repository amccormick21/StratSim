using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphing
{
    public struct DataPoint
    {
        public DataPoint(double x, double y, int index, bool isCycled)
        {
            X = x;
            Y = y;
            this.index = index;
            this.isCycled = isCycled;
        }

        public double X;
        public double Y;
        public int index;
        public bool isCycled;

        public Point Point
        {
            get { return new Point((int)Math.Round(X), (int)Math.Round(Y)); }
        }
    }
}
