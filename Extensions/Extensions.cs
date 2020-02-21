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

        public static IEnumerator GetAllIntersectionPoints(this Model.Solid solid, Geometry3d.CoordinateSystem coordinateSystem)
        {
            var point2 = new Geometry3d.Point(coordinateSystem.Origin);
            var point3 = new Geometry3d.Point(coordinateSystem.Origin);

            point2.Translate(coordinateSystem.AxisX);
            point3.Translate(coordinateSystem.AxisY);

            return solid.GetAllIntersectionPoints(coordinateSystem.Origin, point2, point3);
        }

        public static IEnumerator IntersectAllFaces(this Model.Solid solid, Geometry3d.CoordinateSystem coordinateSystem)
        {
            var point2 = new Geometry3d.Point(coordinateSystem.Origin);
            var point3 = new Geometry3d.Point(coordinateSystem.Origin);

            point2.Translate(coordinateSystem.AxisX);
            point3.Translate(coordinateSystem.AxisY);

            return solid.IntersectAllFaces(coordinateSystem.Origin, point2, point3);
        }
    }
}
