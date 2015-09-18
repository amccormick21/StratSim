using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Graphing
{
    public class CyclingGraph : NormalisedGraph
    {
        GraphLine cycleValue;
        public event EventHandler<GraphLine> CycleValueChanged;
        public GraphLine CycleValue
        {
            get { return cycleValue; }
            set
            {
                cycleValue = value;
                if (CycleValueChanged != null)
                    CycleValueChanged(this, value);
            }
        }

        bool cycleGraph;
        public event EventHandler<bool> CycleChanged;
        public bool CycleGraph
        {
            get { return cycleGraph; }
            set
            {
                cycleGraph = value;
                if (CycleChanged != null)
                    CycleChanged(this, value);
            }
        }

        public CyclingGraph()
            : base()
        {
            cycleGraph = true;
            CycleChanged += CyclingGraph_CycleChanged;
            CycleValueChanged += CyclingGraph_CycleValueChanged;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        void CyclingGraph_CycleChanged(object sender, bool e)
        {
            Invalidate();
        }

        void CyclingGraph_CycleValueChanged(object sender, GraphLine e)
        {
            Invalidate();
        }

        protected override GraphLine[] CalculateLines(List<GraphLine> tracesToCalculateFrom)
        {
            var newTraces = tracesToCalculateFrom;
            if (cycleGraph)
            {
                newTraces = GetCycledLines();
            }

            return base.CalculateLines(newTraces);
        }

        private List<GraphLine> GetCycledLines()
        {
            //The traces are still in data form; they have not yet been normalised.
            //For now, we assume that the traces are cumulative. If they are not then the mathematics becomes somewhat simpler.
            //We also assume that we want to cycle based on the trace value, rather than a constant. This can be altered later too.
            //The upper and lower cycling limits could be based on a different calculation (e.g. lap ahead, lap behind calculations,
            // or even the difference between the line ahead and the line behind being a percentage of the line ahead.
            //TODO: work out what sort of model accurately represents line cycling.

            //Use the normalisation trace for cycling on.
            GraphLine cycleLine = traces.Find(l => l.Index == NormalisationIndex);

            List<GraphLine> newTraces = new List<GraphLine>();
            List<DataPoint> cycledPoints;
            DataPoint pointToAlter;
            int pointIndex;
            int cycles;
            for (int lineIndex = 0; lineIndex < traces.Count; lineIndex++)
            {
                cycles = 0;
                pointIndex = 0;
                cycledPoints = new List<DataPoint>();
                if (traces[lineIndex].DataPoints.Count > 2)
                {
                    while (pointIndex < traces[lineIndex].DataPoints.Count)
                    {
                        pointToAlter = traces[lineIndex].DataPoints[pointIndex];
                        //Cycling is evaluated on every point.
                        if (pointIndex < cycleLine.DataPoints.Count)
                        {
                            if (pointIndex == 0) //The first points are never cycled
                            {
                                cycledPoints.Add(new DataPoint(pointToAlter.X, pointToAlter.Y, pointToAlter.index, cycles));
                            }
                            else
                            {
                                if (pointIndex + cycles >= 0 && pointIndex + cycles < cycleLine.DataPoints.Count)
                                {
                                    if (pointToAlter.Y - cycleLine.DataPoints[pointIndex + cycles].Y > (cycleLine.DataPoints[pointIndex + cycles].Y - cycleLine.DataPoints[pointIndex - 1 + cycles].Y) / 2)
                                    {
                                        //Cycles needs to increase
                                        cycles++;
                                    }
                                    else if (pointToAlter.Y - cycleLine.DataPoints[pointIndex + cycles].Y < -(cycleLine.DataPoints[pointIndex + cycles].Y - cycleLine.DataPoints[pointIndex - 1 + cycles].Y) / 2)
                                    {
                                        //Cycles needs to be reduced
                                        cycles--;
                                    }
                                    // else no change to cycles
                                }
                                //For positive lap deficits, add past laps from normalisation trace (this cancels out the normalisation).
                                //For negative lap deficits, subtract future laps from normalisation trace.
                                MakeCyclingAdjustments(cycleLine, cycledPoints, pointToAlter, pointIndex, cycles);
                            }
                        }
                        else
                        {
                            //Within the trace to alter but outside of the cycling trace
                            //Assume that there are no further changes to the cycling
                            MakeCyclingAdjustments(cycleLine, cycledPoints, pointToAlter, pointIndex, cycles);
                        }

                        pointIndex++;
                    }
                }

                newTraces.Add(new GraphLine(cycledPoints, traces[lineIndex].Index, traces[lineIndex].Show, traces[lineIndex].LineColour));
            }

            return newTraces;
        }

        private void MakeCyclingAdjustments(GraphLine cycleLine, List<DataPoint> cycledPoints, DataPoint pointToAlter, int pointIndex, int cycles)
        {
            int lowerBound = (cycles >= 0 ? 0 : cycles);
            int upperBound = (cycles >= 0 ? cycles : 0);
            for (int cycleAdjustment = lowerBound; cycleAdjustment != upperBound; cycleAdjustment++)
            {
                if (pointIndex + cycleAdjustment > 0 && pointIndex + cycleAdjustment < cycleLine.DataPoints.Count)
                {
                    pointToAlter.Y -= Math.Sign(cycles) * (cycleLine.DataPoints[pointIndex + cycleAdjustment].Y - cycleLine.DataPoints[pointIndex - 1 + cycleAdjustment].Y);
                }
                //Otherwise no change
            }
            cycledPoints.Add(new DataPoint(pointToAlter.X, pointToAlter.Y, pointToAlter.index, cycles));
        }
    }
}
