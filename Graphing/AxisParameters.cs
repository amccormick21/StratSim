using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphing
{
    /// <summary>
    /// Structure containing data to fully define:
    ///     <para>- Position</para>
    ///     <para>- Offset</para>
    ///     <para>- Scale</para>
    ///     <para>- Label spacing</para>
    /// of a graph axis
    /// </summary>
    public struct AxisParameters
    {
        public int startLocation;
        public int baseOffset;
        public double scaleFactor;
        public int axisLabelSpacing;
    };
}
