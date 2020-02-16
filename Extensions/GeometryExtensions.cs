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
    }


}
