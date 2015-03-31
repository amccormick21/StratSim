using StratSim.Model;
using StratSim.View.MyFlowLayout;
using StratSim.View.UserControls;
using StratSim.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DataSources;
using MyFlowLayout;
using Graphing;

namespace StratSim.View.Panels
{
    /// <summary>
    /// Represents a line graph of cumulative time per unit x-axis.
    /// Allows the graph to be normalised in different ways
    /// </summary>
    public class StrategyGraph : NewGraph
    {
        List<StrategyLine> tracesToDrawOnScreen = new List<StrategyLine>();
        List<StrategyLine> originalTraces = new List<StrategyLine>();
        List<lapDataPoint>[] coordinatesForLookupOnClick;

        int normalisedDriverIndex = 0;
        int raceLaps;
        int lapNumber;

        Color clearColour = global::MyFlowLayout.Properties.Settings.Default.BackColour;
        DriverSelectPanel DriverPanel;

        public delegate void GraphTraceClickEventHandler(int driverIndexClicked, int lapNumberOfClick);
        public event GraphTraceClickEventHandler GraphRightClick;

        //TODO: refactor to move parts from here into Graph, then this will inherit from graph.

        public StrategyGraph(NormalisationType normalisationType, MainForm formToLink)
            : base("Graph", formToLink, Properties.Resources.Graph)
        {
            SetRaceLaps(Data.GetRaceLaps());
            lapNumber = raceLaps;
            graphPanel.SetNormalisationType(normalisationType);

            PanelClosed += StrategyGraph_PanelClosed;
            PanelOpened += StrategyGraph_PanelOpened;

            SetupAxes(raceLaps);

            SetPanelProperties(DockTypes.Top2, AutosizeTypes.Free, FillStyles.None, this.Size);
            MyPadding = new Padding(5, 25, 5, 5);
        }

        public void UpdateGraph(int normalisedDriverIndex)
        {
            tracesToDrawOnScreen = GetTraces(originalTraces, normalisedDriverIndex);
            coordinatesForLookupOnClick = PopulateClickCoordinates();
            Invalidate();
        }

        /// <summary>
        /// Sets the axes to default locations proportional to the size of the graph panel
        /// </summary>
        public void SetupAxes(int raceLaps)
        {
            horizontalAxis.startLocation = 50;

            //Get the available graph area
            graphArea = SetGraphArea();

            horizontalAxis.baseOffset = 0;
            horizontalAxis.scaleFactor = (float)(graphArea.Width) / (float)(raceLaps + 1);
            horizontalAxis.axisLabelSpacing = (int)((25 * raceLaps) / (graphArea.Width));
            verticalAxis.baseOffset = 0;
            verticalAxis.scaleFactor = (float)((graphArea.Height) / 100);
            verticalAxis.startLocation = (graphArea.Height / 2) + 10;
            verticalAxis.axisLabelSpacing = (int)(50 / verticalAxis.scaleFactor);

            MyEvents.OnAxesModified(horizontalAxis, verticalAxis, graphNormalisationType, false);
        }

        /// <summary>
        /// Draws the graph to the screen
        /// </summary>
        /// <param name="tracesToShow">A list of StrategyLine traces to show on the graph</param>
        /// <param name="showAllOnGraph">Represents whether all traces should be shown, or only the traces specified in the driver select panel</param>
        /// <param name="changeNormalised">Represents whether the graph should be re-normalised on the fastest trace</param>
        public void DrawGraph(List<StrategyLine> tracesToShow, bool showAllOnGraph, bool changeNormalised)
        {
            int bestDriver;
            if (changeNormalised)
            {
                bestDriver = GetBestDriver(tracesToShow);
                Data.DriverIndex = bestDriver;

                DriverPanel.UpdateRadioButtons(bestDriver);
                DriverPanel.UpdateCheckBoxes(ShowTracesState.All);
            }
            else
            {
                bestDriver = Data.DriverIndex;
            }
            originalTraces = tracesToShow;

            UpdateGraph(bestDriver);
        }

        /// <summary>
        /// Gets a list of normalised strategy lines based on the driver selected for normalisation
        /// </summary>
        List<StrategyLine> GetTraces(List<StrategyLine> originalTraces, int normalisedDriverIndex)
        {
            List<StrategyLine> tracesToReturn = new List<StrategyLine>();
            float bestTime;
            if (originalTraces.Count != 0)
            { bestTime = originalTraces.Find(t => t.DriverIndex == normalisedDriverIndex).TotalTime; }
            else
            { bestTime = 0; }

            tracesToReturn = NormaliseAllLines(bestTime, raceLaps, originalTraces);

            return tracesToReturn;
        }

        /// <summary>
        /// Draws the graph onto the panel, based on the locally stored traces and axes
        /// </summary>
        /// <param name="e">The paint event arguments generated when the Paint event is fired</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            base.OnPaint(e);
            e.Graphics.Clear(clearColour);
            ClearLabels();
            DrawAxes(e.Graphics, lapNumber);
            DrawLines(e.Graphics, lapNumber);
        }

        /// <summary>
        /// Normalises all of the strategy lines in the list of strategy lines passed to the method
        /// </summary>
        /// <param name="bestTime">The time of the fastest driver</param>
        /// <param name="laps">The number of laps in the race</param>
        /// <param name="originalTraces">The traces to normalise</param>
        /// <returns>A list of strategy lines which are normalised using the selected normalisation method</returns>
        List<StrategyLine> NormaliseAllLines(float bestTime, float laps, List<StrategyLine> originalTraces)
        {
            float adjustPerLap = bestTime / laps;
            StrategyLine normalisationTrace = originalTraces.Find(t => t.DriverIndex == normalisedDriverIndex);

            List<StrategyLine> tracesToReturn = new List<StrategyLine>();

            foreach (StrategyLine s in originalTraces)
            {
                if (DriverPanel.SelectedDriverIndices.Exists(s.DriverIndex))
                {
                    switch (graphNormalisationType)
                    {
                        case NormalisationType.OnAverageValue:
                            {
                                tracesToReturn.Add(GetNormalisedStrategyLine(s, adjustPerLap));
                                break;
                            }
                        case NormalisationType.OnEveryValue:
                            {
                                tracesToReturn.Add(GetNormalisedStrategyLine(s, normalisationTrace));
                                break;
                            }
                    }
                }
            }

            return tracesToReturn;
        }

        /// <summary>
        /// Normalises a strategy on an average time
        /// </summary>
        /// <param name="line">The line to normalise</param>
        /// <param name="adjustPerLap">The amount by which to adjust the trace each lap</param>
        /// <returns>The new normalised line</returns>
        StrategyLine GetNormalisedStrategyLine(StrategyLine line, float adjustPerLap)
        {
            StrategyLine normalisedLine = new StrategyLine(line.DriverIndex);
            lapDataPoint tempPoint;

            float lappedCarAdjustment = 0;
            float actualTime = 0;

            int lapIndex = 0;
            foreach (lapDataPoint p in line.Coordinates)
            {
                //Sets the actual time taken for the strategy to complete,
                //before any adjustments are applied
                if (lapIndex == line.Coordinates.Count - 1)
                    actualTime = p.time;

                //set the properties of the new point
                tempPoint.driver = p.driver;
                tempPoint.lap = p.lap;
                tempPoint.time = (p.time - adjustPerLap * p.lap + lappedCarAdjustment);
                tempPoint.draw = p.draw;

                //Deals with cars being lapped
                //Decreases the time if they are too slow, or increases them if
                //they are too fast
                if (tempPoint.time > (adjustPerLap / 2))
                {
                    lappedCarAdjustment -= adjustPerLap;
                    tempPoint.time -= adjustPerLap;
                    tempPoint.draw = false;
                }
                if (tempPoint.time < -(adjustPerLap / 2))
                {
                    lappedCarAdjustment += adjustPerLap;
                    tempPoint.time += adjustPerLap;
                    tempPoint.draw = false;
                }

                normalisedLine.AddPoint(tempPoint);
                ++lapIndex;
            }
            //Reset the total time for the strategy to the original time.
            normalisedLine.TotalTime = actualTime;

            return normalisedLine;
        }

        /// <summary>
        /// Normalises a strategy line on every lap.
        /// </summary>
        /// <param name="line">The line to normalise</param>
        /// <param name="normalisationTrace">The trace used for normalisation</param>
        /// <returns>The new normalised line</returns>
        StrategyLine GetNormalisedStrategyLine(StrategyLine line, StrategyLine normalisationTrace)
        {
            StrategyLine normalisedLine = new StrategyLine(line.DriverIndex);
            lapDataPoint tempPoint;

            int normalisedDriver = normalisationTrace.DriverIndex;
            float lappedCarCyleLimit;

            float thisLapAdjust = 0;
            float lappedCarAdjustment = 0;
            float actualTime = 0;

            int lapIndex = 0;
            foreach (lapDataPoint p in line.Coordinates)
            {
                if (lapIndex == 0)
                {
                    lappedCarCyleLimit = normalisationTrace.Coordinates[lapIndex].time;
                }
                else
                {
                    lappedCarCyleLimit = normalisationTrace.Coordinates[lapIndex].time - normalisationTrace.Coordinates[lapIndex - 1].time;
                }
                thisLapAdjust = normalisationTrace.Coordinates[lapIndex].time;

                if (lapIndex == line.Coordinates.Count - 1)
                    actualTime = p.time;

                tempPoint.driver = p.driver;
                tempPoint.lap = p.lap;
                tempPoint.time = p.time - thisLapAdjust + lappedCarAdjustment;
                tempPoint.draw = p.draw;

                if (tempPoint.time > (lappedCarCyleLimit / 2))
                {
                    lappedCarAdjustment -= lappedCarCyleLimit;
                    tempPoint.time -= lappedCarCyleLimit;
                    tempPoint.draw = false;
                }
                if (tempPoint.time < -(lappedCarCyleLimit / 2))
                {
                    lappedCarAdjustment += lappedCarCyleLimit;
                    tempPoint.time += lappedCarCyleLimit;
                    tempPoint.draw = false;
                }

                normalisedLine.AddPoint(tempPoint);
                ++lapIndex;
            }
            normalisedLine.TotalTime = actualTime;

            return normalisedLine;
        }

        /// <summary>
        /// Removes all graph axis labels and clears the list of labels
        /// </summary>
        void ClearLabels()
        {
            foreach (Label l in horizontalAxisLabels)
            {
                this.Controls.Remove(l);
            }
            foreach (Label l in verticalAxisLabels)
            {
                this.Controls.Remove(l);
            }
            horizontalAxisLabels.Clear();
            verticalAxisLabels.Clear();
        }

        /// <summary>
        /// Draws all of the traces that are to be drawn to the screen
        /// </summary>
        /// <param name="g">The graphics to use to draw the lines on screen</param>
        /// <param name="upToLap">The number of laps to show on the graph</param>
        void DrawLines(Graphics g, int upToLap)
        {
            foreach (StrategyLine lineToDraw in tracesToDrawOnScreen)
            {
                lineToDraw.DrawLine(horizontalAxis, verticalAxis, g, upToLap);
            }
        }

        /// <summary>
        /// Gets the driver with the fastest trace out of a list of specified traces
        /// </summary>
        /// <param name="traces">The traces to search for the best driver</param>
        /// <returns>The index of the driver with the fastest trace</returns>
        public int GetBestDriver(List<StrategyLine> traces)
        {
            float bestTime = 0;
            int driversChecked = 0;
            int driverIndex = 0;

            foreach (StrategyLine s in traces)
            {
                //Most wanted holder loop
                if (DriverPanel.SelectedDriverIndices.Count == 0 || DriverPanel.SelectedDriverIndices.Exists(s.DriverIndex))
                {
                    if (driversChecked == 0)
                    {
                        bestTime = s.TotalTime;
                        driverIndex = s.DriverIndex;
                    }
                    if (s.TotalTime < bestTime)
                    {
                        bestTime = s.TotalTime;
                        driverIndex = s.DriverIndex;
                    }
                    driversChecked++;
                }
            }

            return driverIndex;
        }
        /// <summary>
        /// Gets the best driver out of the list of traces currently
        /// selected to be displayed on screen
        /// </summary>
        /// <returns>The index of the driver with the fastest trace</returns>
        public int GetBestDriver()
        {
            return GetBestDriver(tracesToDrawOnScreen);
        }

        public void StrategyGraph_MouseClick(object sender, MouseEventArgs e)
        {
            lapDataPoint clickLocation;
            int driverIndex;
            int lapNumber;

            if (e.Button == MouseButtons.Right)
            {
                //Convert the location of the point to a lapDataPoint
                clickLocation = GetPositionOfPoint(e.Location, horizontalAxis, verticalAxis);
                if (clickLocation.lap > 0 && clickLocation.lap < Data.GetRaceLaps())
                {
                    //Find the clicked driver index
                    driverIndex = FindClickedDriver(clickLocation);

                    //If the driver index is within range
                    if (driverIndex >= 0)
                    {
                        lapNumber = clickLocation.lap;

                        string confirmEvent = "Confirm event for " + Data.Drivers[driverIndex].DriverName + " on lap " + Convert.ToString(lapNumber);
                        bool fireEvent = Functions.StartDialog(confirmEvent, "Confirm");

                        if (GraphRightClick != null && fireEvent)
                            GraphRightClick(driverIndex, lapNumber);
                    }
                }
            }
        }

        /// <summary>
        /// Converts a panel coordinate location into a lapDataPoint
        /// </summary>
        /// <param name="point">The point to be converted</param>
        /// <param name="HorizontalAxis">The horizontal axis used for conversion</param>
        /// <param name="VerticalAxis">The vertical axis used for conversion</param>
        /// <returns>The lapDataPoint closest to the specified point</returns>
        public static lapDataPoint GetPositionOfPoint(Point point, axisParameters HorizontalAxis, axisParameters VerticalAxis)
        {
            lapDataPoint pointOnAxes;
            pointOnAxes.driver = -1;
            pointOnAxes.draw = true;

            int x = point.X;
            int y = point.Y;

            pointOnAxes.lap = (int)((x - HorizontalAxis.startLocation) / HorizontalAxis.scaleFactor) - HorizontalAxis.baseOffset + 1;
            pointOnAxes.time = ((y - VerticalAxis.startLocation) / VerticalAxis.scaleFactor) + VerticalAxis.baseOffset;

            return pointOnAxes;
        }

        /// <summary>
        /// Shows traces on the graph if the driver index is selected for show
        /// </summary>
        public void ShowTraces()
        {
            foreach (StrategyLine l in tracesToDrawOnScreen)
            {
                l.Show = DriverPanel.SelectedDriverIndices.Exists(l.DriverIndex);
            }

            UpdateGraph(normalisedDriverIndex);
        }

        /// <summary>
        /// Changes the trace on which the graph is normalised,
        /// and redraws the graph
        /// </summary>
        /// <param name="SelectedDriver">The new driver to normalise the graph on</param>
        public void ChangeNormalised(int SelectedDriver)
        {
            Data.DriverIndex = normalisedDriverIndex = SelectedDriver;
            DriverPanel.UpdateCheckBoxes(ShowTracesState.Selected);
            DriverPanel.UpdateRadioButtons(normalisedDriverIndex);
            UpdateGraph(SelectedDriver);
        }

        /// <returns>An array of lists of points representing the points on the graph at which a line exists.
        /// The array index is the lap number; the list index is an arbitrary driver index, not related to the
        /// actual driver array index</returns>
        List<lapDataPoint>[] PopulateClickCoordinates()
        {
            List<lapDataPoint>[] clickCoordinates = new List<lapDataPoint>[raceLaps];

            for (int lapNumber = 0; lapNumber < raceLaps; lapNumber++)
            {
                clickCoordinates[lapNumber] = new List<lapDataPoint>();
                foreach (StrategyLine driverTrace in tracesToDrawOnScreen)
                {
                    if (driverTrace.Show)
                    {
                        if (driverTrace.Coordinates[lapNumber].draw) { clickCoordinates[lapNumber].Add(driverTrace.Coordinates[lapNumber]); }
                    }
                }
            }

            return clickCoordinates;
        }

        /// <summary>
        /// Finds the driver index of the nearest driver to the location of a click.
        /// Most wanted holder routine.
        /// </summary>
        /// <param name="clickLocation">The point at which the graph was clicked</param>
        /// <returns>The index of the driver who is clicked</returns>
        int FindClickedDriver(lapDataPoint clickLocation)
        {
            int mostWantedDriverIndex = -1;
            double closestDistance = 0;
            double currentDistance = 0;

            int driversChecked = 0;

            //for each point on the requested lap
            foreach (lapDataPoint possibleMatch in coordinatesForLookupOnClick[clickLocation.lap])
            {
                currentDistance = GetDistanceFromClickLocation(possibleMatch, clickLocation);

                //if the get distance does not return an error
                if (currentDistance > 0)
                {
                    //if this is the first driver checked
                    if (driversChecked == 0)
                    {
                        closestDistance = currentDistance;
                        mostWantedDriverIndex = possibleMatch.driver;
                    }
                    else
                    {
                        //if the current driver is closer than the previous closest
                        if (currentDistance < closestDistance)
                        {
                            closestDistance = currentDistance;
                            mostWantedDriverIndex = possibleMatch.driver;
                        }
                    }
                }
                driversChecked++;
            }

            return mostWantedDriverIndex;
        }

        /// <summary></summary>
        /// <returns>A value representing the gap between the point being checked and the point being searched for</returns>
        double GetDistanceFromClickLocation(lapDataPoint pointBeingChecked, lapDataPoint pointToSearchFor)
        {
            double distanceFromIntendedLocation = -1;

            if (pointBeingChecked.lap == pointToSearchFor.lap)
            {
                distanceFromIntendedLocation = Math.Pow(pointBeingChecked.time - pointToSearchFor.time, 2);
            }

            return distanceFromIntendedLocation;
        }

        /// <summary>
        /// Sets the number of laps the graph will display
        /// </summary>
        /// <param name="value">The number of laps to set the graph to display</param>
        public void SetRaceLaps(int value)
        { raceLaps = value; }

        public void SetDriverPanel(DriverSelectPanel driverPanel)
        { DriverPanel = driverPanel; }

        public List<StrategyLine> Traces
        { get { return tracesToDrawOnScreen; } }

        public List<lapDataPoint>[] ClickCoordinates
        { get { return coordinatesForLookupOnClick; } }
    }
}
