using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Geometry3d;

namespace Tekla.Structures.OpenApi
{
    public static class GeometryExtensions
    {
        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="vector">Vector to add to.</param>
        /// <param name="vectorToBeAdded">Vector to be added.</param>
        /// <returns>Sum of given vectors.</returns>
        public static Vector Add(this Vector vector, Vector vectorToBeAdded)
        {
            vector.X += vectorToBeAdded.X;
            vector.Y += vectorToBeAdded.Y;
            vector.Z += vectorToBeAdded.Z;
            return vector;
        }

        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="vector">Vector to subtract from.</param>
        /// <param name="vectorToBeSubtracted">Vector to be subtracted.</param>
        /// <returns>Difference of given vectors.</returns>
        public static Vector Subtract(this Vector vector, Vector vectorToBeSubtracted)
        {
            return vector.Add(-1 * vectorToBeSubtracted);
        }

        /// <summary>
        /// Translates point by given vector.
        /// </summary>
        /// <param name="point">Point to be translated.</param>
        /// <param name="vector">Translation vector.</param>
        public static void Translate(this Point point, Vector vector)
        {
            point.Translate(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Finds if two line segments cross. If so, an intersecting point is returned.
        /// </summary>
        /// <param name="lineSegment">First line segment.</param>
        /// <param name="anotherLine">Second line segment.</param>
        /// <returns>Intersection point if lines cross, null otherwise.</returns>
        public static Point Intersect(this LineSegment lineSegment, LineSegment anotherLine)
        {
            var lineA = new Line(lineSegment);
            var lineB = new Line(anotherLine);
            var intersection = Intersection.LineToLine(lineA, lineB);
            if (intersection == null || intersection.Length() != 0) return null; // return null in case of parallel lines or when lines do not intersect
            // check if intersection point is within both line segments
            var point = intersection.Point1;

            var withinFirstLine = (point.X >= lineSegment.Point1.X && point.X <= lineSegment.Point2.X) || (point.X >= lineSegment.Point2.X && point.X <= lineSegment.Point1.X) &&
                                  (point.Y >= lineSegment.Point1.Y && point.Y <= lineSegment.Point2.Y) || (point.Y >= lineSegment.Point2.Y && point.Y <= lineSegment.Point1.Y) &&
                                  (point.Z >= lineSegment.Point1.Z && point.Z <= lineSegment.Point2.Z) || (point.Z >= lineSegment.Point2.Z && point.Z <= lineSegment.Point1.Z);
            var withinSecondLine = (point.X >= anotherLine.Point1.X && point.X <= anotherLine.Point2.X) || (point.X >= anotherLine.Point2.X && point.X <= anotherLine.Point1.X) &&
                                   (point.Y >= anotherLine.Point1.Y && point.Y <= anotherLine.Point2.Y) || (point.Y >= anotherLine.Point2.Y && point.Y <= anotherLine.Point1.Y) &&
                                   (point.Z >= anotherLine.Point1.Z && point.Z <= anotherLine.Point2.Z) || (point.Z >= anotherLine.Point2.Z && point.Z <= anotherLine.Point1.Z);

            if (withinFirstLine && withinSecondLine) return point;
            else return null;
        }
    }


}
