using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Model;

namespace Tekla.Structures.OpenApi
{
    public static class Extensions
    {
        #region Private members
        /// <summary>
        /// Random instance.
        /// </summary>
        private static Random random = new Random();

        /// <summary>
        /// Creates a new vector, whose values are randomized. Returned vector is normalized.
        /// </summary>
        /// <returns>Random vector.</returns>
        private static Geometry3d.Vector GetRandomVector()
        {
            var randomVector = new Geometry3d.Vector(1, 1, 1);
            randomVector.X *= random.NextDouble();
            randomVector.Y *= random.NextDouble();
            randomVector.Z *= random.NextDouble();
            return randomVector.GetNormal();
        }
        #endregion

        #region Public members
        /// <summary>
        /// Creates list of objects with given type from object implementing IEnumerator interface.
        /// </summary>
        /// <typeparam name="T">Type of objects in the list.</typeparam>
        /// <param name="enumerator">Object implementing IEnumerator interface.</param>
        /// <returns>List of objects of given type.</returns>
        public static List<T> ToList<T>(this IEnumerator enumerator)
        {
            var list = new List<T>();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is T) list.Add((T)enumerator.Current);
            }
            return list;
        }

        /// <summary>
        /// Creates an array list using collection of model objects.
        /// </summary>
        /// <param name="collection">Collection of model objects.</param>
        /// <returns>Array list containing model objects.</returns>
        public static ArrayList ToArrayList(this ICollection<ModelObject> collection)
        {
            var arraylist = new ArrayList();
            foreach (var item in collection)
            {
                arraylist.Add(item);
            }
            return arraylist;
        }

        /// <summary>
        /// Gets given object's phase.
        /// </summary>
        /// <param name="modelObject">Model object to read phase from.</param>
        /// <returns>Model object's phase.</returns>
        public static Phase GetPhase(this ModelObject modelObject)
        {
            var phase = new Phase();
            modelObject.GetPhase(out phase);
            return phase;
        }

        /// <summary>
        /// Overload of Tekla's method, using an AABB as input parameter instead of 2 points.
        /// </summary>
        /// <param name="selector">Model object selector to select nearby objects.</param>
        /// <param name="box">Axis-aligned bounding box as input.</param>
        /// <returns>Enumerator of model objects being near to given AABB.</returns>
        public static ModelObjectEnumerator GetObjectsByBoundingBox(this ModelObjectSelector selector, Geometry3d.AABB box)
        {            
            return selector.GetObjectsByBoundingBox(box.MinPoint, box.MaxPoint);
        }

        /// <summary>
        /// Overload of Tekla's method, using an OBB as input parameter instead of 2 points.
        /// </summary>
        /// <param name="selector">Model object selector to select nearby objects.</param>
        /// <param name="box">Oriented bounding box as input.</param>
        /// <returns>Enumerator of model objects being near to given AABB.</returns>
        public static ModelObjectEnumerator GetObjectsByBoundingBox(this ModelObjectSelector selector, Geometry3d.OBB box)
        {
            var workPlaneHandler = new Model.Model().GetWorkPlaneHandler();
            var currentTransformationPlane = workPlaneHandler.GetCurrentTransformationPlane();
            workPlaneHandler.SetCurrentTransformationPlane(new TransformationPlane(box.Center, box.Axis0, box.Axis1));
            var result = selector.GetObjectsByBoundingBox(
                MinPoint: new Geometry3d.Point(-box.Extent0, -box.Extent1, -box.Extent2), 
                MaxPoint: new Geometry3d.Point( box.Extent0,  box.Extent1,  box.Extent2));
            workPlaneHandler.SetCurrentTransformationPlane(currentTransformationPlane);
            return result;
        }

        /// <summary>
        /// Overload of Tekla's method, selects objects using given collection.
        /// </summary>
        /// <param name="selector">Model object selector to select objects in UI.</param>
        /// <param name="collection">Object implementing generic ICollection interface.</param>
        /// <returns>True on success, false otherwise.</returns>
        public static bool Select(this Model.UI.ModelObjectSelector selector, ICollection<ModelObject> collection)
        {
            return selector.Select(collection.ToArrayList());
        }

        /// <summary>
        /// Overload of Tekla's method, selects objects using given collection.
        /// </summary>
        /// <param name="selector">Model object selector to select objects in UI.</param>
        /// <param name="collection">Object implementing generic ICollection interface.</param>
        /// <param name="showDimensions">Defines whether to show dimensions of selected objects.</param>
        /// <returns>True on success, false otherwise.</returns>
        public static bool Select(this Model.UI.ModelObjectSelector selector, ICollection<ModelObject> collection, bool showDimensions)
        {
            return selector.Select(collection.ToArrayList(), showDimensions);
        }

        /// <summary>
        /// Tekla's method overload, using coordinate system object instead of 3 points.
        /// Used to get all the intersection points between the solid and a plane. Does not arrange the points into polygons, thus a lot faster.
        /// </summary>
        /// <param name="solid">Solid to be intersected with a plane.</param>
        /// <param name="coordinateSystem">Coordinate system defining a plane.</param>
        /// <returns></returns>
        public static IEnumerator GetAllIntersectionPoints(this Model.Solid solid, Geometry3d.CoordinateSystem coordinateSystem)
        {
            var point2 = new Geometry3d.Point(coordinateSystem.Origin);
            var point3 = new Geometry3d.Point(coordinateSystem.Origin);

            point2.Translate(coordinateSystem.AxisX);
            point3.Translate(coordinateSystem.AxisY);

            return solid.GetAllIntersectionPoints(coordinateSystem.Origin, point2, point3);
        }

        /// <summary>
        /// Tekla's method overload, using geometric plane object instead of 3 points.
        /// Used to get all the intersection points between the solid and a plane. Does not arrange the points into polygons, thus a lot faster.
        /// </summary>
        /// <param name="solid">Solid to be intersected with a plane.</param>
        /// <param name="plane">Geometric plane.</param>
        /// <returns></returns>
        public static IEnumerator GetAllIntersectionPoints(this Model.Solid solid, Geometry3d.GeometricPlane plane)
        {
            var normalVector = plane.GetNormal();
            var randomVector = GetRandomVector();

            while (normalVector.GetAngleBetween(randomVector) == 0 || normalVector.GetAngleBetween(randomVector) == Math.PI)
            {
                randomVector = GetRandomVector();
            }

            var firstVectorOnPlane = normalVector.Cross(randomVector);
            var secondVectorOnPlane = normalVector.Cross(firstVectorOnPlane);

            var point2 = new Geometry3d.Point(plane.Origin);
            var point3 = new Geometry3d.Point(plane.Origin);

            point2.Translate(firstVectorOnPlane);
            point3.Translate(secondVectorOnPlane);

            return solid.GetAllIntersectionPoints(plane.Origin, point2, point3);
        }

        /// <summary>
        /// Tekla's method overload, using coordinate system object instead of 3 points.
        /// Returns an enumerator for an array list of lists of plane - solid intersection points from all intersecting faces.
        /// The first item of one list contains points of the outmost intersection polygon and then the inner polygons (if there are any).
        /// </summary>
        /// <param name="solid">Solid to be intersected with a plane.</param>
        /// <param name="coordinateSystem">Coordinate system defining a plane.</param>
        /// <returns></returns>
        public static IEnumerator IntersectAllFaces(this Model.Solid solid, Geometry3d.CoordinateSystem coordinateSystem)
        {
            var point2 = new Geometry3d.Point(coordinateSystem.Origin);
            var point3 = new Geometry3d.Point(coordinateSystem.Origin);

            point2.Translate(coordinateSystem.AxisX);
            point3.Translate(coordinateSystem.AxisY);

            return solid.IntersectAllFaces(coordinateSystem.Origin, point2, point3);
        }

        /// <summary>
        /// Tekla's method overload, using geometric plane object instead of 3 points.
        /// Returns an enumerator for an array list of lists of plane - solid intersection points from all intersecting faces.
        /// The first item of one list contains points of the outmost intersection polygon and then the inner polygons (if there are any).
        /// </summary>
        /// <param name="solid">Solid to be intersected with a plane.</param>
        /// <param name="plane">Geometric plane.</param>
        /// <returns></returns>
        public static IEnumerator IntersectAllFaces(this Model.Solid solid, Geometry3d.GeometricPlane plane)
        {
            var normalVector = plane.GetNormal();
            var randomVector = GetRandomVector();

            while(normalVector.GetAngleBetween(randomVector) == 0 || normalVector.GetAngleBetween(randomVector) == Math.PI)
            {
                randomVector = GetRandomVector();
            }

            var firstVectorOnPlane = normalVector.Cross(randomVector);
            var secondVectorOnPlane = normalVector.Cross(firstVectorOnPlane);

            var point2 = new Geometry3d.Point(plane.Origin);
            var point3 = new Geometry3d.Point(plane.Origin);

            point2.Translate(firstVectorOnPlane);
            point3.Translate(secondVectorOnPlane);

            return solid.IntersectAllFaces(plane.Origin, point2, point3);
        }

        /// <summary>
        /// Searches for boolean cut-parts attached to given part, yet not cutting it, i.e. not changing its volume.
        /// </summary>
        /// <param name="part">Model part to be analyzed.</param>
        /// <returns>List of BooleanParts not reducing part's volume.</returns>
        public static List<BooleanPart> GetRedundantCuts(this Part part)
        {
            var partSolid = part.GetSolid();
            var cuts = part.GetBooleans().ToList<BooleanPart>().Where(c => c.Type == BooleanPart.BooleanTypeEnum.BOOLEAN_CUT);
            var result = new List<BooleanPart>();
            foreach (var cut in cuts)
            {
                // skip when there are faces in the part created by given cut, otherwise add cut to the list
                if (partSolid.GetFaceEnumerator().ToList<Solid.Face>().Any(f => f.OriginPartId.ID == cut.Identifier.ID)) continue;
                result.Add(cut);
            }
            return result;
        }

        /// <summary>
        /// Creates Tekla's PointList collection out of point IEnumerable.
        /// </summary>
        /// <param name="collection">Point enumerable.</param>
        /// <returns>Tekla's PointList object.</returns>
        public static Drawing.PointList ToPointList(this IEnumerable<Geometry3d.Point> collection)
        {
            var list = new Drawing.PointList();
            foreach (var item in collection)
            {
                list.Add(item);
            }
            return list;
        }

        #endregion
    }
}
