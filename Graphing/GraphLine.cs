using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphing
{
    public class GraphLine
    {
        public List<DataPoint> DataPoints { get; private set; }
        public int Index { get; set; }
        internal bool Show { get; set; }
        internal Color LineColour { get; set; }

        public GraphLine(IEnumerable<DataPoint> points, int index, bool show, Color color)
        {
            this.DataPoints = new List<DataPoint>(points);
            this.Index = index;
            this.Show = show;
            this.LineColour = color;
        }
        public GraphLine(int index, bool show, Color color)
        {
            this.DataPoints = new List<DataPoint>();
            this.Index = index;
            this.Show = show;
            this.LineColour = color;
        }

        internal double HighestX()
        {
            double mostWantedHolder;

            if (DataPoints.Count > 0)
            {
                mostWantedHolder = DataPoints[0].X;
                for (int i = 1; i < DataPoints.Count; ++i)
                {
                    if (DataPoints[i].X > mostWantedHolder)
                    {
                        mostWantedHolder = DataPoints[i].X;
                    }
                }
                return mostWantedHolder;
            }
            else
            {
                return 1;
            }
        }

        internal double HighestY()
        {
            double mostWantedHolder;

            if (DataPoints.Count > 0)
            {
                mostWantedHolder = DataPoints[0].Y;
                for (int i = 1; i < DataPoints.Count; ++i)
                {
                    if (DataPoints[i].Y > mostWantedHolder)
                    {
                        mostWantedHolder = DataPoints[i].Y;
                    }
                }
                return mostWantedHolder;
            }
            else
            {
                return 1;
            }
        }

        internal double LowestX()
        {
            double mostWantedHolder;

            if (DataPoints.Count > 0)
            {
                mostWantedHolder = DataPoints[0].X;
                for (int i = 1; i < DataPoints.Count; ++i)
                {
                    if (DataPoints[i].X < mostWantedHolder)
                    {
                        mostWantedHolder = DataPoints[i].X;
                    }
                }
                return mostWantedHolder;
            }
            else
            {
                return 0;
            }
        }

        internal double LowestY()
        {
            double mostWantedHolder;

            if (DataPoints.Count > 0)
            {
                mostWantedHolder = DataPoints[0].Y;
                for (int i = 1; i < DataPoints.Count; ++i)
                {
                    if (DataPoints[i].Y < mostWantedHolder)
                    {
                        mostWantedHolder = DataPoints[i].Y;
                    }
                }
                return mostWantedHolder;
            }
            else
            {
                return 0;
            }
        }

        internal virtual void DrawLine(Graphics g)
        {
            //If the line is to be shown
            if (Show)
            {
                DataPoint startPoint = new DataPoint();
                DataPoint endPoint = new DataPoint();
                Pen pen = new Pen(LineColour, 1);

                //Lap number is 0-based
                endPoint = DataPoints[0];

                //Cycle through the points
                for (int pointIndex = 1; pointIndex < DataPoints.Count; pointIndex++)
                {
                    if (DataPoints[pointIndex].isCycled)
                    {
                        pen.DashPattern = new float[] { 1, 2 };
                        pen.Width = 1F;
                        //pen.Color = Color.FromArgb((int)(LineColour.A / 2F), (int)(LineColour.R / 2F), (int)(LineColour.G / 2F), (int)(LineColour.B / 2F));
                    }
                    else
                    {
                        pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                        pen.Width = 2F;
                        //pen.Color = LineColour;
                    }

                    //Set the new start point to the old end point
                    startPoint = endPoint;

                    //Update the end point
                    endPoint = DataPoints[pointIndex];

                    //If the point is to be shown
                    if (DataPoints[pointIndex - 1].isCycled == DataPoints[pointIndex].isCycled)
                    {
                        if (g.ClipBounds.Contains(startPoint.Point) && g.ClipBounds.Contains(endPoint.Point))
                        {
                            g.DrawLine(pen, startPoint.Point, endPoint.Point);
                        }
                    }
                }
                pen.Dispose();
            }

        }

        internal void AddDataPoint(int X, int Y, bool draw)
        {
            DataPoints.Add(new DataPoint(X, Y, Index, draw));
        }
    }
}
