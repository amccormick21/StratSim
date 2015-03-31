using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace StratSim.View.MyFlowLayout
{
    /// <summary>
    /// Panel to be displayed on the main form.
    /// Contains methods for dynamically arranging 'MyPanel' constituents.
    /// Contains data about the panels on the form
    /// </summary>
    public class WindowFlowPanel : Panel
    {
        List<IDockableControl> dockableControlsInPanel = new List<IDockableControl>(); //list of controls on form
        List<MyPanel> myPanelsOnForm = new List<MyPanel>();

        LayoutLines myLayoutLines;
        Rectangle remainingSize;

        public MainForm _parent;

        Dictionary<DockTypes, Point> dockPointLocations = new Dictionary<DockTypes, Point>();

        int panelsOnForm;
        int alexMcCormickBackgroundState;
        int easterEggBackgroundImage;

        public WindowFlowPanel(MainForm parentForm)
        {
            //Define the parent
            _parent = parentForm;

            //Add a background
            this.BackgroundImage = Properties.Resources.RedBull;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.DoubleBuffered = true;

            //Locate this panel on the form
            Location = new Point(0, _parent.header.Height);
            Size = new Size(_parent.ClientRectangle.Width, _parent.ClientSize.Height - _parent.header.Height);

            //Define the initial dock points
            SetInitialLocationsForDockPoints();

            //Setup local variables
            panelsOnForm = 0;
            remainingSize.Size = this.ClientSize;
            remainingSize.Location = new Point(0, 0);
            alexMcCormickBackgroundState = 0;
            easterEggBackgroundImage = 0;
            myLayoutLines = new LayoutLines(this.ClientRectangle);

            //Subscribe to events fired when layout changes
            FlowLayoutEvents.MainPanelLayoutChanged += MyEvents_MainPanelLayoutChanged;
            FlowLayoutEvents.MainPanelResizeEnd += FlowLayoutEvents_MainPanelResizeEnd;
        }

        void FlowLayoutEvents_MainPanelResizeEnd()
        {
            this.Focus();
        }

        /// <summary>
        /// Easter egg for changing the background of the panel
        /// </summary>
        internal void AlexMcCormickEasterEgg(Keys k)
        {
            switch (alexMcCormickBackgroundState)
            {
                case 0:
                case 1:
                    {
                        if (k == Keys.Up) { alexMcCormickBackgroundState++; }
                        else { alexMcCormickBackgroundState = 0; }
                        break;
                    }
                case 2:
                case 3:
                    {
                        if (k == Keys.Down) { alexMcCormickBackgroundState++; }
                        else { alexMcCormickBackgroundState = 0; }
                        break;
                    }
                case 4:
                case 6:
                    {
                        if (k == Keys.Left) { alexMcCormickBackgroundState++; }
                        else { alexMcCormickBackgroundState = 0; }
                        break;
                    }
                case 5:
                case 7:
                    {
                        if (k == Keys.Right) { alexMcCormickBackgroundState++; }
                        else { alexMcCormickBackgroundState = 0; }
                        break;
                    }
                case 8:
                    {
                        if (k == Keys.B) { alexMcCormickBackgroundState++; }
                        else { alexMcCormickBackgroundState = 0; }
                        break;
                    }
                case 9:
                    {
                        if (k == Keys.A)
                        {
                            switch (easterEggBackgroundImage)
                            {
                                case 0: this.BackgroundImage = Properties.Resources.MercedesAMG; break;
                                case 1: this.BackgroundImage = Properties.Resources.Ferrari; break;
                                case 2: this.BackgroundImage = Properties.Resources.Lotus; break;
                                case 3: this.BackgroundImage = Properties.Resources.Mclaren; break;
                                case 4: this.BackgroundImage = Properties.Resources.ForceIndia; break;
                                case 5: this.BackgroundImage = Properties.Resources.Sauber; break;
                                case 6: this.BackgroundImage = Properties.Resources.TorroRosso; break;
                                case 7: this.BackgroundImage = Properties.Resources.Williams; break;
                                case 8: this.BackgroundImage = Properties.Resources.Marussia; break;
                                case 9: this.BackgroundImage = Properties.Resources.Caterham; break;
                                case 10: this.BackgroundImage = Properties.Resources.RedBull; break;
                            }
                            easterEggBackgroundImage = (easterEggBackgroundImage + 1) % 11;
                            alexMcCormickBackgroundState = 0;
                        }
                        else { alexMcCormickBackgroundState = 0; }
                        break;
                    }
            }

        }

        /// <summary>
        /// Lays out the panel after a user change to the layout,
        /// which requires a change to panel sizes or locations
        /// </summary>
        void MyEvents_MainPanelLayoutChanged()
        {
            this.Size = new Size(_parent.ClientSize.Width, _parent.ClientSize.Height - _parent.header.Height);

            remainingSize.Size = this.Size;
            remainingSize.Location = new Point(0, 0);

            //Re-layout the panel and resize components on it
            RepeatLayout();
            ResizeUsingLayoutLines();
        }

        /// <summary>
        /// Should be called after panels have been added to the window requiring it to be laid out again
        /// </summary>
        public void FinishedAddingPanels()
        {
            //Set the size to the minimum size allowed
            foreach (IDockableControl c in dockableControlsInPanel)
            {
                ((Control)c).Size = c.OriginalSize;
            }

            //Set the remaining size on the panel to the size of the container
            remainingSize.Size = this.Size;
            remainingSize.Location = new Point(0, 0);

            //Re-layout the panel and resize components on it
            RepeatLayout();
            ResizeUsingLayoutLines();
        }

        /// <summary>
        /// Physically adds a control to the panel
        /// </summary>
        /// <param name="controlToAdd"></param>
        /// <param name="notAlreadyOnForm"></param>
        public void AddControl(IDockableControl controlToAdd, bool notAlreadyOnForm)
        {
            Control controlCopy = (Control)controlToAdd;

            if (notAlreadyOnForm) //if the panel is not already on the form, add it to the form
            {
                dockableControlsInPanel.Add(controlToAdd);
                if (controlToAdd.Type == typeof(MyPanel))
                {
                    myPanelsOnForm.Add((MyPanel)controlCopy);
                    ((MyPanel)controlCopy).PanelIndex = panelsOnForm;
                    _parent.OnPanelAdded(panelsOnForm++);
                }
            }

            if (controlToAdd.MyVisible) //if the panel is to be displayed
            {
                //add the control to the form
                this.Controls.Add(controlCopy);

                controlCopy.Size = controlToAdd.OriginalSize;
                controlCopy.Location = GetLocation(controlToAdd);

                //If the control takes the full size of the window, set the size now
                if (controlToAdd.FillStyle == FillStyles.FullWidth)
                {
                    controlCopy.Width = remainingSize.Width;
                    controlCopy.Left = remainingSize.Left;
                }
                if (controlToAdd.FillStyle == FillStyles.FullHeight)
                {
                    controlCopy.Height = remainingSize.Height;
                    controlCopy.Top = remainingSize.Top;
                }
                if (controlToAdd.FillStyle == FillStyles.FullScreen)
                {
                    controlCopy.Location = remainingSize.Location;
                    controlCopy.Size = remainingSize.Size;
                }

                //Updates the remaining size for other controls which take the full size of the window
                UpdateRemainingSize(controlToAdd);
                //Update the dock point locations
                UpdatePoints(controlToAdd);

                //for the purposes of the layout lines:
                myLayoutLines.AddPanel(controlCopy, controlToAdd.DockType);
            }

        }
        /// <summary>
        /// Removes a control from the panel and therefore the form.
        /// </summary>
        /// <param name="panelToRemove">The control to remove from the form</param>
        public void RemoveControl(IDockableControl panelToRemove)
        {
            //Remove the control from the lists of controls to draw
            dockableControlsInPanel.Remove(panelToRemove);
            myPanelsOnForm.Remove((MyPanel)panelToRemove);
            this.Controls.Remove((MyPanel)panelToRemove);
            panelsOnForm--;
        }

        void SetInitialLocationsForDockPoints()
        {
            Point topLeft = new Point(ClientRectangle.Left, ClientRectangle.Top);
            Point bottomLeft = new Point(ClientRectangle.Left, ClientRectangle.Bottom);
            Point bottomRight = new Point(ClientRectangle.Right, ClientRectangle.Bottom);
            Point topRight = new Point(ClientRectangle.Right, ClientRectangle.Top);

            dockPointLocations[DockTypes.TopLeft] = topLeft; //Top Left
            dockPointLocations[DockTypes.Left] = topLeft; //Left
            dockPointLocations[DockTypes.BottomLeft] = bottomLeft; //BottomLeft
            dockPointLocations[DockTypes.Top] = topLeft; //Top
            dockPointLocations[DockTypes.None] = topLeft; //None
            dockPointLocations[DockTypes.Bottom] = bottomLeft; //Bottom
            dockPointLocations[DockTypes.TopRight] = topRight; //TopRight
            dockPointLocations[DockTypes.Right] = topRight; //Right
            dockPointLocations[DockTypes.BottomRight] = bottomRight; //BottomRight
            dockPointLocations[DockTypes.Top2] = topRight; //TopRightCentre
            dockPointLocations[DockTypes.Bottom2] = bottomRight; //BottomRightCentre
        }

        /// <summary>
        /// Lays out the controls dynamically
        /// Adds the controls to their respective lists for FillStyle
        /// Adds the controls in order so that their maximum size is observed
        /// Then adds all other controls using the layout lines to size the panels
        /// </summary>
        void RepeatLayout()
        {
            List<IDockableControl> FullScreens = new List<IDockableControl>();
            List<IDockableControl> FullWidths = new List<IDockableControl>();
            List<IDockableControl> FullHeights = new List<IDockableControl>();
            List<IDockableControl> Others = new List<IDockableControl>();
            IDockableControl toolbar = null;

            myLayoutLines.ClearLinesInContainer(this.ClientRectangle);
            SetInitialLocationsForDockPoints();

            //Remove all controls and assign them to relevant lists
            foreach (IDockableControl d in dockableControlsInPanel)
            {
                this.Controls.Remove((Control)d);
                if (d.Type == typeof(MyToolbar)) { toolbar = d; }
                if (d.FillStyle == FillStyles.FullScreen) { FullScreens.Add(d); }
                if (d.FillStyle == FillStyles.FullWidth) { FullWidths.Add(d); }
                if (d.FillStyle == FillStyles.FullHeight) { FullHeights.Add(d); }
                if (d.FillStyle == FillStyles.None) { Others.Add(d); }
            }

            //Work through the lists adding controls again

            //If any control is fullscreen, only it and the toolbar should be added
            if (FullScreens.Count != 0)
            {
                AddControl(toolbar, false);
                foreach (IDockableControl d in FullScreens)
                {
                    AddControl(d, false);
                }
            }
            else
            {
                foreach (IDockableControl d in FullWidths)
                {
                    AddControl(d, false);
                }
                foreach (IDockableControl d in FullHeights)
                {
                    AddControl(d, false);
                }
                foreach (IDockableControl d in Others)
                {
                    AddControl(d, false);
                }
            }
        }

        /// <returns>The location at which to dock the panel at</returns>
        Point GetLocation(IDockableControl c)
        {
            Control controlCopy = (Control)c;

            Point drawPoint = new Point(); //point at which to draw control
            Point dockPoint = new Point(); //point at which control is docked.

            DockTypes DockType = c.DockType;

            dockPoint = dockPointLocations[DockType];

            //set the property of the control to the location at which it is docked.
            c.DockPointLocation = dockPoint;

            //Finding controlCopy.Location:
            //if docked at bottom, drawpoint is higher than dockpoint
            if ((DockType == DockTypes.Bottom) || (DockType == DockTypes.BottomLeft) || (DockType == DockTypes.BottomRight) || (DockType == DockTypes.Bottom2))
            {
                drawPoint.Y = dockPoint.Y - controlCopy.Height;
            }
            else //drawpoint.Y is the same as dockpoint.Y
            {
                drawPoint.Y = dockPoint.Y;
            }

            //if docked at right, drawpoint is to the left of dockpoint
            if ((DockType == DockTypes.TopRight) || (DockType == DockTypes.Right) || (DockType == DockTypes.BottomRight) || (DockType == DockTypes.Bottom2) || (DockType == DockTypes.Top2))
            {
                drawPoint.X = dockPoint.X - controlCopy.Width;
            }
            else
            {
                drawPoint.X = dockPoint.X;
            }

            return drawPoint;
        }

        void UpdatePoints(IDockableControl controlBeingAdded)
        {
            Control controlCopy = (Control)controlBeingAdded;

            //Depending on how the control fills the screen,
            //update the location of the dock points
            switch (controlBeingAdded.FillStyle)
            {
                case FillStyles.FullWidth:
                    {
                        //Change the locatoin of the dock points that are affected by the control
                        if (MyPanel.IsDockedAtTop(controlBeingAdded.DockType))
                        {
                            dockPointLocations[DockTypes.TopLeft] = AddHeightToDockPoint(DockTypes.TopLeft, controlCopy.Height);
                            dockPointLocations[DockTypes.Left] = AddHeightToDockPoint(DockTypes.Left, controlCopy.Height);
                            dockPointLocations[DockTypes.Top] = AddHeightToDockPoint(DockTypes.Top, controlCopy.Height);
                            dockPointLocations[DockTypes.None] = AddHeightToDockPoint(DockTypes.None, controlCopy.Height);
                            dockPointLocations[DockTypes.TopRight] = AddHeightToDockPoint(DockTypes.TopRight, controlCopy.Height);
                            dockPointLocations[DockTypes.Right] = AddHeightToDockPoint(DockTypes.Right, controlCopy.Height);
                            dockPointLocations[DockTypes.Top2] = AddHeightToDockPoint(DockTypes.Top2, controlCopy.Height);
                        }
                        else
                        {
                            dockPointLocations[DockTypes.BottomLeft] = AddHeightToDockPoint(DockTypes.BottomLeft, -controlCopy.Height);
                            dockPointLocations[DockTypes.Bottom] = AddHeightToDockPoint(DockTypes.Bottom, -controlCopy.Height);
                            dockPointLocations[DockTypes.BottomRight] = AddHeightToDockPoint(DockTypes.BottomRight, -controlCopy.Height);
                            dockPointLocations[DockTypes.Bottom2] = AddHeightToDockPoint(DockTypes.Bottom2, -controlCopy.Height);
                        }
                        break;
                    }

                case FillStyles.FullHeight:
                    {
                        if (MyPanel.IsDockedAtLeft(controlBeingAdded.DockType))
                        {
                            dockPointLocations[DockTypes.TopLeft] = AddWidthToDockPoint(DockTypes.TopLeft, controlCopy.Width);
                            dockPointLocations[DockTypes.Left] = AddWidthToDockPoint(DockTypes.Left, controlCopy.Width);
                            dockPointLocations[DockTypes.BottomLeft] = AddWidthToDockPoint(DockTypes.BottomLeft, controlCopy.Width);
                            dockPointLocations[DockTypes.Top] = AddWidthToDockPoint(DockTypes.Top, controlCopy.Width);
                            dockPointLocations[DockTypes.None] = AddWidthToDockPoint(DockTypes.None, controlCopy.Width);
                            dockPointLocations[DockTypes.Bottom] = AddWidthToDockPoint(DockTypes.Bottom, controlCopy.Width);
                        }
                        else
                        {
                            dockPointLocations[DockTypes.TopRight] = AddWidthToDockPoint(DockTypes.TopRight, -controlCopy.Width);
                            dockPointLocations[DockTypes.Right] = AddWidthToDockPoint(DockTypes.Right, -controlCopy.Width);
                            dockPointLocations[DockTypes.BottomRight] = AddWidthToDockPoint(DockTypes.BottomRight, -controlCopy.Width);
                            dockPointLocations[DockTypes.Top2] = AddWidthToDockPoint(DockTypes.Top2, -controlCopy.Width);
                            dockPointLocations[DockTypes.Bottom2] = AddWidthToDockPoint(DockTypes.Bottom2, -controlCopy.Width);
                        }
                        break;
                    }
                case FillStyles.None:
                    {
                        //If the panel being added will affect the dock point:
                        if ((controlBeingAdded.DockPointLocation == dockPointLocations[DockTypes.TopLeft]) || (controlBeingAdded.DockPointLocation == dockPointLocations[DockTypes.Left]) || (controlBeingAdded.DockPointLocation == dockPointLocations[DockTypes.None]))
                        {
                            dockPointLocations[DockTypes.TopLeft] = AddHeightToDockPoint(DockTypes.TopLeft, controlCopy.Height);
                            dockPointLocations[DockTypes.Left] = AddHeightToDockPoint(DockTypes.Left, controlCopy.Height);
                            dockPointLocations[DockTypes.None] = AddHeightToDockPoint(DockTypes.None, controlCopy.Height);
                        }
                        if (controlBeingAdded.DockPointLocation == dockPointLocations[DockTypes.BottomLeft])
                        {
                            dockPointLocations[DockTypes.BottomLeft] = AddHeightToDockPoint(DockTypes.BottomLeft, -controlCopy.Height);
                        }
                        if (controlBeingAdded.DockPointLocation == dockPointLocations[DockTypes.Top])
                        {
                            dockPointLocations[DockTypes.Top] = AddWidthToDockPoint(DockTypes.Top, controlCopy.Width);
                        }
                        if (controlBeingAdded.DockPointLocation == dockPointLocations[DockTypes.Bottom])
                        {
                            dockPointLocations[DockTypes.Bottom] = AddWidthToDockPoint(DockTypes.Bottom, controlCopy.Width);
                        }
                        if (controlBeingAdded.DockPointLocation == dockPointLocations[DockTypes.TopRight])
                        {
                            dockPointLocations[DockTypes.TopRight] = AddHeightToDockPoint(DockTypes.TopRight, controlCopy.Height);
                        }
                        if (controlBeingAdded.DockPointLocation == dockPointLocations[DockTypes.Right])
                        {
                            dockPointLocations[DockTypes.Right] = AddHeightToDockPoint(DockTypes.Right, controlCopy.Height);
                        }
                        if (controlBeingAdded.DockPointLocation == dockPointLocations[DockTypes.BottomRight])
                        {
                            dockPointLocations[DockTypes.BottomRight] = AddHeightToDockPoint(DockTypes.BottomRight, -controlCopy.Height);
                        }
                        if (controlBeingAdded.DockPointLocation == dockPointLocations[DockTypes.Top2])
                        {
                            dockPointLocations[DockTypes.Top2] = AddWidthToDockPoint(DockTypes.Top2, -controlCopy.Width);
                        }
                        if (controlBeingAdded.DockPointLocation == dockPointLocations[DockTypes.Bottom2])
                        {
                            dockPointLocations[DockTypes.Bottom2] = AddWidthToDockPoint(DockTypes.Bottom2, -controlCopy.Width);
                        }
                        break;
                    }
            }
        }
        /// <summary>
        /// Updates the rectangle of remaining area on the panel after a control is added
        /// </summary>
        /// <param name="addedControl">The control that was added</param>
        void UpdateRemainingSize(IDockableControl addedControl)
        {
            Control controlCopy = (Control)addedControl;

            if (addedControl.FillStyle == FillStyles.FullHeight)
            {
                remainingSize.Width -= controlCopy.Width;
                if (MyPanel.IsDockedAtLeft(addedControl.DockType))
                {
                    remainingSize.Location = new Point(remainingSize.Left + controlCopy.Width, remainingSize.Top);
                }
            }
            if (addedControl.FillStyle == FillStyles.FullWidth)
            {
                remainingSize.Height -= controlCopy.Height;
                if (MyPanel.IsDockedAtLeft(addedControl.DockType))
                {
                    remainingSize.Location = new Point(remainingSize.Left, remainingSize.Top + controlCopy.Height);
                }
            }
        }

        /// <summary>
        /// Resizes all panels on the form using the layout lines
        /// All panels are displayed at the maximum size possible
        /// </summary>
        void ResizeUsingLayoutLines()
        {
            Dictionary<Locations, int> indicesOfLinesAroundPanel;
            Dictionary<Locations, bool> canSizeInDirection;

            foreach (IDockableControl controlToSize in dockableControlsInPanel)
            {
                indicesOfLinesAroundPanel = new Dictionary<Locations, int>();
                canSizeInDirection = new Dictionary<Locations, bool>();
                int newPositionOfEdge;

                Control copyOfControl = (Control)controlToSize;
                if (controlToSize.AutoSizeType != AutosizeTypes.Constant)
                {
                    //Check around the panel
                    foreach (var direction in (Locations[])Enum.GetValues(typeof(Locations)))
                    {
                        indicesOfLinesAroundPanel[direction] = myLayoutLines.GetLineIndex(myLayoutLines.AddLineFromSideOfPanel(copyOfControl, direction), InvertLocation(direction));

                        //The panel can size if a layout line exists that matches the side of the panel,
                        //and the autosize type allows the panel to be resized
                        canSizeInDirection[direction] = ((indicesOfLinesAroundPanel[direction] != -1)
                            && AutosizeTypeAllowsResize(direction, controlToSize));
                    }
                    //Size the panel in each direction
                    foreach (var direction in (Locations[])Enum.GetValues(typeof(Locations)))
                    {
                        if (canSizeInDirection[direction])
                        {
                            //Gets the furthest position to which a panel can be resized.
                            newPositionOfEdge = myLayoutLines.GetSizeChangeAllowed(copyOfControl, direction);

                            switch (direction)
                            {
                                case Locations.Top:
                                    {
                                        //Resize the panel by the required amount
                                        //The height is increased by the difference between the possible position
                                        //and the current position of the edge
                                        copyOfControl.Height += copyOfControl.Top - newPositionOfEdge;
                                        //The location of the top of the control is reduced by the same amount
                                        copyOfControl.Top -= copyOfControl.Top - newPositionOfEdge;

                                        //Move the relevant layout line to the new position
                                        if (indicesOfLinesAroundPanel[Locations.Top] != -1)
                                        {
                                            myLayoutLines.LinesForLayout[Locations.Bottom][(indicesOfLinesAroundPanel[Locations.Top])].StaticLocation = newPositionOfEdge;
                                        }
                                        //Update the length of the lines to the sides of the panel.
                                        if (indicesOfLinesAroundPanel[Locations.Right] != -1)
                                        {
                                            //Set the start point of the line to the side to the new position
                                            myLayoutLines.LinesForLayout[Locations.Left][(indicesOfLinesAroundPanel[Locations.Right])].setStartPointY(newPositionOfEdge);
                                            //Set the end point of the line to the side to the new position,
                                            //maintining the link between the start point of one line and the end point of the previous line.
                                            if (indicesOfLinesAroundPanel[Locations.Right] > 0)
                                                myLayoutLines.LinesForLayout[Locations.Left][(indicesOfLinesAroundPanel[Locations.Right]) - 1].setEndPointY(newPositionOfEdge);
                                        }
                                        if (indicesOfLinesAroundPanel[Locations.Left] != -1)
                                        {
                                            myLayoutLines.LinesForLayout[Locations.Right][(indicesOfLinesAroundPanel[Locations.Left])].setStartPointY(newPositionOfEdge);
                                            if (indicesOfLinesAroundPanel[Locations.Left] > 0)
                                                myLayoutLines.LinesForLayout[Locations.Right][(indicesOfLinesAroundPanel[Locations.Left]) - 1].setEndPointY(newPositionOfEdge);
                                        }

                                        break;
                                    }
                                case Locations.Left: //See comments in above sections
                                    {
                                        copyOfControl.Width += copyOfControl.Left - newPositionOfEdge;
                                        copyOfControl.Left -= copyOfControl.Left - newPositionOfEdge;

                                        if (indicesOfLinesAroundPanel[Locations.Left] != -1)
                                        {
                                            myLayoutLines.LinesForLayout[Locations.Right][(indicesOfLinesAroundPanel[Locations.Left])].StaticLocation = newPositionOfEdge;
                                        }
                                        if (indicesOfLinesAroundPanel[Locations.Bottom] != -1)
                                        {
                                            myLayoutLines.LinesForLayout[Locations.Top][(indicesOfLinesAroundPanel[Locations.Bottom])].setStartPointX(newPositionOfEdge);
                                            if (indicesOfLinesAroundPanel[Locations.Bottom] > 0)
                                                myLayoutLines.LinesForLayout[Locations.Top][(indicesOfLinesAroundPanel[Locations.Bottom]) - 1].setEndPointX(newPositionOfEdge);
                                        }
                                        if (indicesOfLinesAroundPanel[Locations.Top] != -1)
                                        {
                                            myLayoutLines.LinesForLayout[Locations.Bottom][(indicesOfLinesAroundPanel[Locations.Top])].setStartPointX(newPositionOfEdge);
                                            if (indicesOfLinesAroundPanel[Locations.Top] > 0)
                                                myLayoutLines.LinesForLayout[Locations.Bottom][(indicesOfLinesAroundPanel[Locations.Top]) - 1].setEndPointX(newPositionOfEdge);
                                        }

                                        break;
                                    }
                                case Locations.Bottom: //See comments in above sections
                                    {
                                        copyOfControl.Height += newPositionOfEdge - copyOfControl.Bottom;

                                        if (indicesOfLinesAroundPanel[Locations.Bottom] != -1)
                                        {
                                            myLayoutLines.LinesForLayout[Locations.Top][(indicesOfLinesAroundPanel[Locations.Bottom])].StaticLocation = newPositionOfEdge;
                                        }
                                        if (indicesOfLinesAroundPanel[Locations.Left] != -1)
                                        {
                                            myLayoutLines.LinesForLayout[Locations.Right][(indicesOfLinesAroundPanel[Locations.Left])].setEndPointY(newPositionOfEdge);
                                            if (indicesOfLinesAroundPanel[Locations.Left] < myLayoutLines.LinesForLayout[Locations.Right].Count - 1)
                                                myLayoutLines.LinesForLayout[Locations.Right][(indicesOfLinesAroundPanel[Locations.Left]) + 1].setStartPointY(newPositionOfEdge);
                                        }
                                        if (indicesOfLinesAroundPanel[Locations.Right] != -1)
                                        {
                                            myLayoutLines.LinesForLayout[Locations.Left][(indicesOfLinesAroundPanel[Locations.Right])].setEndPointY(newPositionOfEdge);
                                            if (indicesOfLinesAroundPanel[Locations.Right] < myLayoutLines.LinesForLayout[Locations.Left].Count - 1)
                                                myLayoutLines.LinesForLayout[Locations.Left][(indicesOfLinesAroundPanel[Locations.Right]) + 1].setStartPointY(newPositionOfEdge);
                                        }

                                        break;
                                    }
                                case Locations.Right: //See comments in above sections
                                    {
                                        copyOfControl.Width += newPositionOfEdge - copyOfControl.Right;

                                        if (indicesOfLinesAroundPanel[Locations.Right] != -1)
                                        {
                                            myLayoutLines.LinesForLayout[Locations.Left][(indicesOfLinesAroundPanel[Locations.Right])].StaticLocation = newPositionOfEdge;
                                        }
                                        if (indicesOfLinesAroundPanel[Locations.Top] != -1)
                                        {
                                            myLayoutLines.LinesForLayout[Locations.Bottom][(indicesOfLinesAroundPanel[Locations.Top])].setEndPointX(newPositionOfEdge);
                                            if (indicesOfLinesAroundPanel[Locations.Top] < myLayoutLines.LinesForLayout[Locations.Bottom].Count - 1)
                                                myLayoutLines.LinesForLayout[Locations.Bottom][(indicesOfLinesAroundPanel[Locations.Top]) + 1].setStartPointX(newPositionOfEdge);
                                        }
                                        if (indicesOfLinesAroundPanel[Locations.Bottom] != -1)
                                        {
                                            myLayoutLines.LinesForLayout[Locations.Top][(indicesOfLinesAroundPanel[Locations.Bottom])].setEndPointX(newPositionOfEdge);
                                            if (indicesOfLinesAroundPanel[Locations.Bottom] < myLayoutLines.LinesForLayout[Locations.Top].Count - 1)
                                                myLayoutLines.LinesForLayout[Locations.Top][(indicesOfLinesAroundPanel[Locations.Bottom]) + 1].setStartPointX(newPositionOfEdge);
                                        }

                                        break;
                                    }
                            }


                        }
                    }
                }
            }

            FlowLayoutEvents.OnMainPanelResize();
        }

        /// <summary>
        /// Checks if the AutoSizeType for a panel allows it to resize in the specified direction
        /// </summary>
        /// <param name="direction">The direction the panel is to be sized in</param>
        /// <param name="controlToResize">The control to be resized</param>
        /// <returns>True if the panel can be resized in the specified direction</returns>
        static bool AutosizeTypeAllowsResize(Locations direction, IDockableControl controlToResize)
        {
            bool typeCorrect = false;

            if (controlToResize.AutoSizeType == AutosizeTypes.AutoHeight)
            {
                if (direction == Locations.Top || direction == Locations.Bottom)
                {
                    typeCorrect = true;
                }
            }
            if (controlToResize.AutoSizeType == AutosizeTypes.AutoWidth)
            {
                if (direction == Locations.Left || direction == Locations.Right)
                {
                    typeCorrect = true;
                }
            }
            if (controlToResize.AutoSizeType == AutosizeTypes.Free)
            {
                typeCorrect = true;
            }

            return typeCorrect;
        }
        /// <returns>The direction that is opposite to the specified direction</returns>
        static Locations InvertLocation(Locations location)
        {
            Locations returnLocation;

            switch (location)
            {
                case Locations.Top: returnLocation = Locations.Bottom; break;
                case Locations.Left: returnLocation = Locations.Right; break;
                case Locations.Bottom: returnLocation = Locations.Top; break;
                case Locations.Right: returnLocation = Locations.Left; break;
                default: returnLocation = Locations.Top; break;
            }

            return returnLocation;
        }

        /// <summary>
        /// Adds a height, in pixels, to the y-coordinate of the dock point at the specified dock type
        /// </summary>
        /// <returns>The new point with the height added</returns>
        Point AddHeightToDockPoint(DockTypes dockType, int heightToAdd)
        {
            return new Point(dockPointLocations[dockType].X, dockPointLocations[dockType].Y + heightToAdd);
        }
        /// <summary>
        /// Adds a width, in pixels, to the x-coordinate of the dock point at the specified dock type
        /// </summary>
        /// <returns>The new point with the width added</returns>

        Point AddWidthToDockPoint(DockTypes dockType, int widthToAdd)
        {
            return new Point(dockPointLocations[dockType].X + widthToAdd, dockPointLocations[dockType].Y);
        }


        public List<MyPanel> VisiblePanels
        {
            get { return myPanelsOnForm; }
            set { myPanelsOnForm = value; }
        }
        public int PanelsOnForm
        {
            get { return panelsOnForm; }
            set { panelsOnForm = value; }
        }
        public Dictionary<DockTypes, Point> DockPoints
        { get { return dockPointLocations; } }

    }
}
