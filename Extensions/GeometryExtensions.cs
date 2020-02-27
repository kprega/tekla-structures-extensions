using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

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
        /// Creates a point in average coordinates of all points in specified list.
        /// </summary>
        /// <param name="pointList">Point list to be processed.</param>
        /// <returns>Average point.</returns>
        public static Point Average(this Drawing.PointList pointList)
        {
            var array = pointList.ToArray();
            return new Point(array.Average(p => p.X), array.Average(p => p.Y), array.Average(p => p.Z));
        }

        /// <summary>
        /// Creates a point in average coordinates of all points in specified collection.
        /// </summary>
        /// <param name="points">Collection of points to be processed.</param>
        /// <returns>Average point.</returns>
        public static Point Average(this ICollection<Point> points)
        {
            return new Point(points.Average(p => p.X), points.Average(p => p.Y), points.Average(p => p.Z));
        }

        /// <summary>
        /// Creates oriented bounding box around given part object.
        /// </summary>
        /// <param name="part">Part-type object.</param>
        /// <returns>Part-oriented bounding box.</returns>
        public static OBB GetOrientedBoundingBox(this Part part)
        {
            var wph = new Model.Model().GetWorkPlaneHandler();
            var coordSys = part.GetCoordinateSystem();
            var tp = new TransformationPlane(coordSys);
            wph.SetCurrentTransformationPlane(tp);
            var solid = part.GetSolid();
            var centerPoint = new Point
            {
                X = (solid.MaximumPoint.X + solid.MinimumPoint.X) / 2.0,
                Y = (solid.MaximumPoint.Y + solid.MinimumPoint.Y) / 2.0,
                Z = (solid.MaximumPoint.Z + solid.MinimumPoint.Z) / 2.0
            };
            var extents = new double[]
            {
                Math.Abs((solid.MaximumPoint.X - solid.MinimumPoint.X) / 2.0),
                Math.Abs((solid.MaximumPoint.Y - solid.MinimumPoint.Y) / 2.0),
                Math.Abs((solid.MaximumPoint.Z - solid.MinimumPoint.Z) / 2.0)
            };
            centerPoint = tp.TransformationMatrixToGlobal.Transform(centerPoint);
            wph.SetCurrentTransformationPlane(new TransformationPlane());
            var vectors = new Vector[3];
            vectors[0] = coordSys.AxisX;
            vectors[1] = coordSys.AxisY;
            vectors[2] = Vector.Cross(vectors[0], vectors[1]);

            return new OBB(centerPoint, vectors, extents);
        }
    }


}
