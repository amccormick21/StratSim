using Graphing;
using StratSim.Model;
using StratSim.View.Panels;
using System.Collections.Generic;
using System.Drawing;

namespace StratSim.View.UserControls
{
    /// <summary>
    /// Represents a point to be drawn on a cumulative time graph
    /// </summary>
    public struct lapDataPoint
    {
        public int lap;
        public float time;
        public int driver;
        public bool draw;
    };

    /// <summary>
    /// No longer used but maintained as code is functional.
    /// Contains a collection of points that, when linked, represents a trace
    /// on a cumulative time graph.
    /// </summary>
    public class StrategyLine
    {
        List<lapDataPoint> points;
        int driverIndex;
        float sumOfTime = 0;
        bool show = true;

        public StrategyLine(List<lapDataPoint> Points, int driverIndex)
        {
            points = Points;
            this.driverIndex = driverIndex;
        }

        public StrategyLine(int driverIndex)
        {
            this.driverIndex = driverIndex;
            points = new List<lapDataPoint>();
        }

        /// <summary>
        /// Draws a the line represented by the list of points using the specified graphics
        /// </summary>
        /// <param name="horizontalAxis">The horizontal axis parameters</param>
        /// <param name="verticalAxis">The vertical axis parameters</param>
        /// <param name="g">The graphics to use to draw the line</param>
        /// <param name="upToLap">The last lap to display</param>
        public void DrawLine(AxisParameters horizontalAxis, AxisParameters verticalAxis, Graphics g, int upToLap)
        {
            //If the line is to be shown
            if (show)
            {
                Point startPoint = new Point();
                Point endPoint = new Point();
                Color lineColour = Data.Drivers[driverIndex].LineColour;
                Pen pen = new Pen(lineColour, 1);

                //Lap number is 0-based
                endPoint.X = GetPointOrdinate(points[0].lap, horizontalAxis, true);
                endPoint.Y = GetPointOrdinate(points[0].time, verticalAxis, false);

                //Cycle through the points
                for (int pointIndex = 1; pointIndex < points.Count; pointIndex++)
                {
                    //Set the new start point to the old end point
                    startPoint = endPoint;

                    //Update the end point
                    endPoint.X = GetPointOrdinate(points[pointIndex].lap, horizontalAxis, true);
                    endPoint.Y = GetPointOrdinate(points[pointIndex].time, verticalAxis, false);

                    //If the point is within range and is to be shown
                    if ((startPoint.X >= horizontalAxis.startLocation) && (points[pointIndex].draw) && (points[pointIndex].lap <= upToLap))
                        g.DrawLine(pen, startPoint, endPoint);
                }
                pen.Dispose();
            }
        }

        /// <summary>
        /// Gets the ordinate on the specified axis that is represented by the locating value
        /// </summary>
        /// <param name="locator">The ordinate that is to be displayed</param>
        /// <param name="axis">The axis on which the ordinate is required</param>
        /// <returns>The value of the ordinate that the point is to be drawn at</returns>
        int GetPointOrdinate(float locator, AxisParameters axis, bool horizontal)
        {
            float locatorToRepresent = locator + (horizontal ? -1 : 1) * axis.baseOffset;
            int position = (int)(axis.startLocation + (axis.scaleFactor * locatorToRepresent));

            return position;
        }

        /// <summary>
        /// Adds a point to the end of the list of points to be displayed
        /// </summary>
        /// <param name="pointToAdd">The point to add to the list</param>
        public void AddPoint(lapDataPoint pointToAdd)
        { points.Add(pointToAdd); }

        public List<lapDataPoint> Coordinates
        { get { return points; } }

        public int DriverIndex
        {
            get { return driverIndex; }
            set { driverIndex = value; }
        }
        /// <summary>
        /// Gets or sets a value representing the total required time for the specified driver to complete the race.
        /// </summary>
        public float TotalTime
        {
            get { return sumOfTime; }
            set { sumOfTime = value; }
        }
        public bool Show
        {
            get { return show; }
            set { show = value; }
        }
    }
}
