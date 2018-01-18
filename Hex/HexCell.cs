using System.Linq;
using System.Windows;
using System.Windows.Media;
using static System.Math;

namespace Hex
{
    class HexCell
    {
        // Represents the points in a hex cell with a side at the top:
        //        ---
        //       /   \
        //       \   /
        //        ---
        // First point is the one at the upper right and they're listed
        // clockwise, repeating that first point at the end.
	    internal static readonly double S3D2 = Sqrt(3) / 2;
	    private static readonly Point[] _pointsHorz = {
		    new Point(0.5, S3D2),
			new Point(1, 0),
			new Point(0.5, -S3D2),
			new Point(-0.5, -S3D2),
			new Point(-1, 0),
			new Point(-0.5, S3D2),
		    new Point(0.5, S3D2),
	    };

        // Represents the points in a hex cell with a vertex at the top:
        //         /\
        //       /    \
        //       |    |
        //       \    /
        //         \/
        // First point is the one at the top and they're listed
        // clockwise, repeating that first point at the end.
	    private static readonly Point[] _pointsVert =
	    {
		    new Point(0, 1),
		    new Point(S3D2, 0.5),
		    new Point(S3D2, -0.5),
		    new Point(0, -1),
		    new Point(-S3D2, -0.5),
		    new Point(-S3D2, 0.5),
		    new Point(0, 1),
	    };

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        ///  <summary>   Produce a PathGeometry for a hex cell </summary>
        /// 
        ///  <remarks>   sideFlags is a set of binary flags.  Each flag represents a side.  If the flag bit
        ///              is 1 the side is drawn.  If 0 then it is not drawn.  The lowest bit represents the
        ///              side between vertex 0 and vertex 1 in PointsVert of PointsHorz above and the 
        ///              higher order bits circle around the cell clockwise.
        ///              TODO: Better to use lighter weight StreamGeometry rather than PathGeometry?
        ///              Can you fill a broken StreamGeometry?  Do i care?  How much lighter weight?
        ///              Is it worth even considering?  Might be a bit more complex.  I think this comes down
        ///              to whether we want to just draw the hexes and then calculate mouse hits or do we
        ///              we want to make a bunch of shape objects, one per hex, and allow WPF to do the
        ///              hit testing on the individual objects?  The former is more efficient but more complex
        ///              and probably unnecessary.  We can use the geometry produced here either way.
        ///              Darrell Plank, 1/15/2018. </remarks>
        /// 
        ///  <param name="center">       The center of the hex cell. </param>
        ///  <param name="sideLength">   (Optional) Length of a side of the cell. </param>
        ///  <param name="sideFlags">    (Optional) Which sides to draw. </param>
        ///  <param name="fHorizontal">  (Optional) If true, there's a side on top - else a vertex on top. </param>
        /// <returns>   A Geometry representing the sides of the cell to be drawn. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal static Geometry Cell(Vector center, double sideLength = 1.0, byte sideFlags = 0b111111, bool fHorizontal = true)
        {
            var ret = new PathGeometry();
            var pts = (fHorizontal ? _pointsHorz : _pointsVert).Select(p => new Point(sideLength * p.X, sideLength * p.Y) + center).ToArray();
            var pathFigure = new PathFigure {StartPoint = pts[0]};

            for (var iSide = 0; iSide < 6; iSide++)
            {
                var fDrawSide = (sideFlags & 1) != 0;
                sideFlags >>= 1;
                pathFigure.Segments.Add(new LineSegment(pts[iSide + 1], fDrawSide));
            }
            ret.Figures.Add(pathFigure);
            return ret;
        }
    }
}
