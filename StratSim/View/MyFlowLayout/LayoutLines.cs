using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace StratSim.View.MyFlowLayout
{
    public enum LineDirection { Vertical, Horizontal };
    public enum Locations { Top, Left, Bottom, Right };

    /// <summary>
    /// Contains data about a straight horizontal or vertical line
    /// Contains methods for adjusting the length or position of the line.
    /// </summary>
    public class Line
    {
        Point startPoint;
        Point endPoint;

        /// <summary>
        /// Creates a new instance of the line class that is either horizontal or vertical
        /// </summary>
        /// <param name="start">The start location of the line, on the axis parallel to the line direction</param>
        /// <param name="end">The end location of the line, on the axis parallel to the line direction</param>
        /// <param name="staticPosition">The location of the line on the axis perpendicular to the line direction</param>
        /// <param name="direction">The direction in which the line is oriented</param>
        public Line(int start, int end, int staticPosition, LineDirection direction)
        {
            if (direction == LineDirection.Horizontal)
            {
                if (start < end)
                {
                    startPoint = new Point(start, staticPosition);
                    endPoint = new Point(end, staticPosition);
                }
                else
                {
                    startPoint = new Point(end, staticPosition);
                    endPoint = new Point(start, staticPosition);
                }
            }
            else
            {
                if (start < end)
                {
                    startPoint = new Point(staticPosition, start);
                    endPoint = new Point(staticPosition, end);
                }
                else
                {
                    startPoint = new Point(staticPosition, end);
                    endPoint = new Point(staticPosition, start);
                }
            }
        }
        public Line() { }
        /// <summary>
        /// Copy constructor for the line class
        /// </summary>
        public Line(Line l)
        {
            startPoint = l.startPoint;
            endPoint = l.endPoint;
        }

        public void Draw(Graphics g, Color colour)
        {
            var Pen = new Pen(colour, 5);
            g.DrawLine(Pen, startPoint, endPoint);
            Pen.Dispose();
        }

        public void setStartPointY(int value)
        { startPoint.Y = value; }
        public void setStartPointX(int value)
        { startPoint.X = value; }
        public void setEndPointY(int value)
        { endPoint.Y = value; }
        public void setEndPointX(int value)
        { endPoint.X = value; }

        public Point StartPoint
        {
            get { return startPoint; }
            set { startPoint = value; }
        }
        public Point EndPoint
        {
            get { return endPoint; }
            set { endPoint = value; }
        }
        public int Length
        {
            get
            {
                if (endPoint.X == startPoint.X)
                { return (endPoint.Y - startPoint.Y); }
                else
                { return (endPoint.X - startPoint.X); }
            }
        }
        public int StaticLocation
        {
            get
            {
                if (endPoint.X == startPoint.X)
                { return endPoint.X; }
                else
                { return startPoint.Y; }
            }
            set
            {
                if (endPoint.X == startPoint.X)
                { startPoint.X = endPoint.X = value; }
                else
                { startPoint.Y = endPoint.Y = value; }
            }
        }
        public LineDirection Direction
        {
            get
            {
                if (endPoint.X == startPoint.X)
                { return LineDirection.Vertical; }
                else
                { return LineDirection.Horizontal; }
            }
        }
    }

    /// <summary>
    /// Contains a set of lines bounding the panels on the form, used for re-sizing the panels on a window flow panel.
    /// Contains methods for adjusting the lines when panels are added, and
    /// methods for finding the closest and furthest lines, allowing panels to be resized.
    /// </summary>
    class LayoutLines
    {
        Dictionary<Locations, List<Line>> lines;

        public LayoutLines(Rectangle container)
        {
            lines = new Dictionary<Locations, List<Line>>();
            foreach (var Location in (Locations[])Enum.GetValues(typeof(Locations)))
            {
                lines[Location] = new List<Line>();
            }
            SetupLinesAroundContainer(container);
        }

        void SetupLinesAroundContainer(Rectangle container)
        {
            foreach (var Location in (Locations[])Enum.GetValues(typeof(Locations)))
            {
                lines[Location].Clear();
            }

            lines[Locations.Left].Add(new Line(container.Top, container.Bottom, container.Left, LineDirection.Vertical));
            lines[Locations.Top].Add(new Line(container.Left, container.Right, container.Top, LineDirection.Horizontal));
            lines[Locations.Right].Add(new Line(container.Top, container.Bottom, container.Right, LineDirection.Vertical));
            lines[Locations.Bottom].Add(new Line(container.Left, container.Right, container.Bottom, LineDirection.Horizontal));
        }

        /// <summary>
        /// <para>Finds the indices of lines that are intersected by edges of a panel.</para>
        /// <para>These lines are the lines that must be re-sized to allow the panel to re-size in the given direction</para>
        /// </summary>
        /// <param name="panelToBeSized">The control that is to be re-sized on the form</param>
        /// <param name="directionToLookIn">The direction the panel is to be resized in</param>
        /// <returns>An array of two indices, in ascending order, of the indices of the lines that restrict panel sizing</returns>
        int[] GetIndicesOfLinesRestrictingPanel(Control panelToBeSized, Locations directionToLookIn)
        {
            int[] indicesToReturn = new int[2];
            int currentIndex = 0;
            int lastIndex = 0;
            bool firstIndexAllocated = false;

            //Find the set of lines to check for the line that restricts the panel
            switch (directionToLookIn)
            {
                case Locations.Top:
                    {
                        //Check all the lines
                        foreach (Line l in lines[directionToLookIn])
                        {
                            //if the line is the first one found with an end point after the left of the panel to be sized:
                            if (!firstIndexAllocated && (l.EndPoint.X > panelToBeSized.Left))
                            {
                                indicesToReturn[0] = currentIndex;
                                firstIndexAllocated = true;
                            }
                            //if the line start point is before the right side of the panel
                            if (l.StartPoint.X < panelToBeSized.Right)
                            {
                                lastIndex = currentIndex;
                            }

                            currentIndex++;
                        }
                        //the last index found before the right side of the panel is set as the second bounding index
                        indicesToReturn[1] = lastIndex;
                        break;
                    }
                case Locations.Left:
                    {
                        foreach (Line l in lines[directionToLookIn])
                        {
                            if (!firstIndexAllocated && (l.EndPoint.Y > panelToBeSized.Top))
                            {
                                indicesToReturn[0] = currentIndex;
                                firstIndexAllocated = true;
                            }
                            if (l.StartPoint.Y < panelToBeSized.Bottom)
                            {
                                lastIndex = currentIndex;
                            }

                            currentIndex++;
                        }
                        indicesToReturn[1] = lastIndex;
                        break;
                    }
                case Locations.Bottom:
                    {
                        foreach (Line l in lines[directionToLookIn])
                        {
                            if (!firstIndexAllocated && (l.EndPoint.X > panelToBeSized.Left))
                            {
                                indicesToReturn[0] = currentIndex;
                                firstIndexAllocated = true;
                            }
                            if (l.StartPoint.X < panelToBeSized.Right)
                            {
                                lastIndex = currentIndex;
                            }

                            currentIndex++;
                        }
                        indicesToReturn[1] = lastIndex;
                        break;
                    }
                case Locations.Right:
                    {
                        foreach (Line l in lines[directionToLookIn])
                        {
                            if (!firstIndexAllocated && (l.EndPoint.Y > panelToBeSized.Top))
                            {
                                indicesToReturn[0] = currentIndex;
                                firstIndexAllocated = true;
                            }
                            if (l.StartPoint.Y < panelToBeSized.Bottom)
                            {
                                lastIndex = currentIndex;
                            }

                            currentIndex++;
                        }
                        indicesToReturn[1] = lastIndex;
                        break;
                    }
            }

            return indicesToReturn;
        }

        /// <summary>
        /// Inserts a panel into the layout lines system, and updates the lines around it.
        /// </summary>
        /// <param name="p">The control to add to the layout lines system</param>
        /// <param name="DockType">The dock type of the control that is added to the system</param>
        public void AddPanel(Control p, DockTypes DockType)
        {
            int leftIndex = 0;
            int rightIndex = 0;
            int topIndex = 0;
            int bottomIndex = 0;
            int indexToInsertPanelLineAt;
            Line panelLineToInsert;
            int[] lineIndicesRestrictingPanel;
            List<int> lineIndicesToRemove = new List<int>();
            Locations horizontalLocationToAddLine, verticalLocationToAddLine;

            horizontalLocationToAddLine = (MyPanel.IsDockedAtLeft(DockType) ? Locations.Left : Locations.Right);
            verticalLocationToAddLine = (MyPanel.IsDockedAtTop(DockType) ? Locations.Top : Locations.Bottom);

            //VERTICAL POSITION
            lineIndicesRestrictingPanel = GetIndicesOfLinesRestrictingPanel(p, verticalLocationToAddLine);

            leftIndex = lineIndicesRestrictingPanel[0];
            rightIndex = lineIndicesRestrictingPanel[1];

            if (leftIndex == rightIndex)
            {
                lines[verticalLocationToAddLine].Insert(leftIndex + 1, new Line(lines[verticalLocationToAddLine][leftIndex]));
                rightIndex = leftIndex + 1;
            }

            lines[verticalLocationToAddLine][leftIndex].setEndPointX(p.Left);
            lines[verticalLocationToAddLine][rightIndex].setStartPointX(p.Right);

            if (lines[verticalLocationToAddLine][rightIndex].Length == 0)
            {
                lineIndicesToRemove.Add(rightIndex);
            }
            for (int i = rightIndex - 1; i > leftIndex; i--)
            {
                lineIndicesToRemove.Add(i);
            }
            if (lines[verticalLocationToAddLine][leftIndex].Length == 0)
            {
                lineIndicesToRemove.Add(leftIndex);
                indexToInsertPanelLineAt = leftIndex;
            }
            else
            {
                indexToInsertPanelLineAt = leftIndex + 1;
            }

            foreach (int index in lineIndicesToRemove)
            {
                lines[verticalLocationToAddLine].RemoveAt(index);
            }
            lineIndicesToRemove.Clear();

            panelLineToInsert = AddLineFromSideOfPanel(p, (verticalLocationToAddLine == Locations.Top ? Locations.Bottom : Locations.Top));
            lines[verticalLocationToAddLine].Insert(indexToInsertPanelLineAt, panelLineToInsert);


            //HORIZONTAL POSITION
            lineIndicesRestrictingPanel = GetIndicesOfLinesRestrictingPanel(p, horizontalLocationToAddLine);

            topIndex = lineIndicesRestrictingPanel[0];
            bottomIndex = lineIndicesRestrictingPanel[1];

            //If only one line index, the same layout line covers the whole of the top of the panel
            if (topIndex == bottomIndex)
            {
                //Add a copy of this line
                lines[horizontalLocationToAddLine].Insert(topIndex + 1, new Line(lines[horizontalLocationToAddLine][topIndex]));
                bottomIndex = topIndex + 1;
            }

            //Set the end of the left line and start of the right line to the corners of the panel.
            lines[horizontalLocationToAddLine][topIndex].setEndPointY(p.Top);
            lines[horizontalLocationToAddLine][bottomIndex].setStartPointY(p.Bottom);

            //Remove any zero length lines at the bottom of the panel
            if (lines[horizontalLocationToAddLine][bottomIndex].Length == 0)
            {
                lineIndicesToRemove.Add(bottomIndex);
            }
            //Set a flag to remove any lines contained within the bounds of the panel
            for (int lineIndex = bottomIndex - 1; lineIndex > topIndex; lineIndex--)
            {
                lineIndicesToRemove.Add(lineIndex);
            }
            //Remove zero length lines at the top of the panel
            if (lines[horizontalLocationToAddLine][topIndex].Length == 0)
            {
                lineIndicesToRemove.Add(topIndex);
                indexToInsertPanelLineAt = topIndex;
                //If lines to the left are removed, the insertion index is the top index
            }
            else
            {
                indexToInsertPanelLineAt = topIndex + 1;
                //Else the insertion index is one above the top index
            }

            //Remove the lines marked for removal
            foreach (int index in lineIndicesToRemove)
            {
                lines[horizontalLocationToAddLine].RemoveAt(index);
            }
            lineIndicesToRemove.Clear();

            //Insert the line based on the edge of the panel
            panelLineToInsert = AddLineFromSideOfPanel(p, (horizontalLocationToAddLine == Locations.Left ? Locations.Right : Locations.Left));
            lines[horizontalLocationToAddLine].Insert(indexToInsertPanelLineAt, panelLineToInsert);
        }
        public void ClearLinesInContainer(Rectangle container)
        {
            SetupLinesAroundContainer(container);
        }

        /// <summary>Gets the line index of a line that matches the specified line</summary>
        /// <returns>The index of the line that matches the specified line
        /// Returns -1 if the line does not exist</returns>
        public int GetLineIndex(Line lineToMatch, Locations lineLocationToFind)
        {
            int lineIndex, currentLineIndex;

            currentLineIndex = 0;
            lineIndex = -1;

            foreach (Line l in lines[lineLocationToFind])
            {
                if (l.StartPoint == lineToMatch.StartPoint && l.EndPoint == lineToMatch.EndPoint)
                {
                    lineIndex = currentLineIndex;
                }
                currentLineIndex++;
            }

            return lineIndex;
        }
        /// <summary>
        /// Gets the maximum amount that the panel's size can be increased in the selected location
        /// </summary>
        /// <returns>The number of pixels that the panel can be increased in size by</returns>
        public int GetSizeChangeAllowed(Control p, Locations location)
        {
            int sizeChange;

            List<Line> LinesAtEndOfEnlarge = FindLinesSurroundingPanel(p, location);
            sizeChange = GetGreatestDistanceFromEdge(LinesAtEndOfEnlarge, location);

            return sizeChange;
        }

        /// <returns>A collection of the lines within the range of the panel, in the selected direction</returns>
        List<Line> FindLinesSurroundingPanel(Control p, Locations direction)
        {
            int leftLimit = p.Left;
            int rightLimit = p.Right;
            int topLimit = p.Top;
            int bottomLimit = p.Bottom;

            List<Line> linesFound = new List<Line>();

            foreach (Line l in lines[direction])
            {
                if (direction == Locations.Bottom || direction == Locations.Top)
                {
                    if (IsWithinRangeHorizontal(leftLimit, rightLimit, l))
                    {
                        linesFound.Add(l);
                    }
                }
                else
                {
                    if (IsWithinRangeVertical(topLimit, bottomLimit, l))
                    {
                        linesFound.Add(l);
                    }
                }
            }

            return linesFound;
        }
        public int FindLineIndexToTruncate(Control p, Locations location)
        {
            int lineIndexFound = 0;

            switch (location)
            {
                case Locations.Bottom:
                    {
                        lineIndexFound = lines[Locations.Bottom].FindIndex(l => l.EndPoint.X >= p.Right);
                        break;
                    }
                case Locations.Left:
                    {
                        lineIndexFound = lines[Locations.Left].FindIndex(l => l.EndPoint.Y >= p.Bottom);
                        break;
                    }
                case Locations.Top:
                    {
                        lineIndexFound = lines[Locations.Top].FindIndex(l => l.EndPoint.X >= p.Right);
                        break;
                    }
                case Locations.Right:
                    {
                        lineIndexFound = lines[Locations.Right].FindIndex(l => l.EndPoint.Y >= p.Bottom);
                        break;
                    }
            }
            return lineIndexFound;
        }

        /// <returns>The distance away from the edge of the panel of the line that is furthest away from the edge</returns>
        int GetGreatestDistanceFromEdge(List<Line> Lines, Locations location)
        {
            int furthestDistance = 0;
            int linesChecked = 0;

            if (location == Locations.Left || location == Locations.Top)
            {
                foreach (Line l in Lines)
                {
                    if (linesChecked++ == 0) { furthestDistance = l.StaticLocation; }
                    if (l.StaticLocation > furthestDistance) { furthestDistance = l.StaticLocation; }
                }
            }
            else
            {
                foreach (Line l in Lines)
                {
                    if (linesChecked++ == 0) { furthestDistance = l.StaticLocation; }
                    if (l.StaticLocation < furthestDistance) { furthestDistance = l.StaticLocation; }
                }
            }

            return furthestDistance;
        }

        /// <returns>A line matching the location of the side of the panel</returns>
        public Line AddLineFromSideOfPanel(Control p, Locations sideToAddOn)
        {
            Line lineToAdd = new Line();

            switch (sideToAddOn)
            {
                case Locations.Bottom:
                    {
                        lineToAdd = new Line(p.Left, p.Right, p.Bottom, LineDirection.Horizontal);
                        break;
                    }
                case Locations.Left:
                    {
                        lineToAdd = new Line(p.Top, p.Bottom, p.Left, LineDirection.Vertical);
                        break;
                    }
                case Locations.Top:
                    {
                        lineToAdd = new Line(p.Left, p.Right, p.Top, LineDirection.Horizontal);
                        break;
                    }
                case Locations.Right:
                    {
                        lineToAdd = new Line(p.Top, p.Bottom, p.Right, LineDirection.Vertical);
                        break;
                    }
            }
            return lineToAdd;
        }

        bool IsWithinRangeHorizontal(int leftLimit, int rightLimit, Line l)
        {
            bool shouldBeAdded = false;

            if (l.StartPoint.X <= leftLimit && l.EndPoint.X >= rightLimit)
            {
                shouldBeAdded = true;
            }
            else
            {
                if (l.StartPoint.X >= leftLimit && l.StartPoint.X < rightLimit)
                {
                    shouldBeAdded = true;
                }
                else
                {
                    if (l.EndPoint.X > leftLimit && l.EndPoint.X <= rightLimit)
                    {
                        shouldBeAdded = true;
                    }
                }
            }

            return shouldBeAdded;
        }
        bool IsWithinRangeVertical(int topLimit, int bottomLimit, Line l)
        {
            bool shouldBeAdded = false;

            if (l.StartPoint.Y <= topLimit && l.EndPoint.Y >= bottomLimit)
            {
                shouldBeAdded = true;
            }
            else
            {
                if (l.StartPoint.Y >= topLimit && l.StartPoint.Y < bottomLimit)
                {
                    shouldBeAdded = true;
                }
                else
                {
                    if (l.EndPoint.Y > topLimit && l.EndPoint.Y <= bottomLimit)
                    {
                        shouldBeAdded = true;
                    }
                }
            }

            return shouldBeAdded;
        }

        public List<Line> Top
        {
            get { return lines[Locations.Top]; }
            set { lines[Locations.Top] = value; }
        }
        public List<Line> Left
        {
            get { return lines[Locations.Left]; }
            set { lines[Locations.Left] = value; }
        }
        public List<Line> Bottom
        {
            get { return lines[Locations.Bottom]; }
            set { lines[Locations.Bottom] = value; }
        }
        public List<Line> Right
        {
            get { return lines[Locations.Right]; }
            set { lines[Locations.Right] = value; }
        }
        public Dictionary<Locations, List<Line>> LinesForLayout
        {
            get { return lines; }
            set { lines = value; }
        }
    }
}
