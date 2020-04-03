using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Model;

namespace Tekla.Structures.OpenApi
{
    /// <summary>
    /// Class providing extentions for Tekla Structures using OpenAPI.
    /// </summary>
    public static class Extensions
    {
        #region Private members
        /// <summary>
        /// Random instance.
        /// </summary>
        private static Random random = new Random();

        /// <summary>
        /// Creates a new instance of <see cref="Geometry3d.Vector"/>, whose values are randomized. Returned vector is normalized.
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
        /// Creates list of objects with given type from object implementing <see cref="IEnumerator"/> interface.
        /// </summary>
        /// <typeparam name="T">Type of objects in the list.</typeparam>
        /// <param name="enumerator">Object implementing <see cref="IEnumerator"/> interface.</param>
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
        public static ArrayList ToArrayList(this IEnumerable<ModelObject> collection)
        {
            var arraylist = new ArrayList();
            foreach (var item in collection)
            {
                arraylist.Add(item);
            }
            return arraylist;
        }

        /// <summary>
        /// Gets given object's <see cref="Phase"/>.
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
        /// Overload of Tekla's method, using an instance of <see cref="Geometry3d.AABB"/> as input parameter instead of 2 points.
        /// </summary>
        /// <param name="selector">Model object selector to select nearby objects.</param>
        /// <param name="box">Axis-aligned bounding box as input.</param>
        /// <returns>Enumerator of model objects being near to given AABB.</returns>
        public static ModelObjectEnumerator GetObjectsByBoundingBox(this ModelObjectSelector selector, Geometry3d.AABB box)
        {            
            return selector.GetObjectsByBoundingBox(box.MinPoint, box.MaxPoint);
        }

        /// <summary>
        /// Overload of Tekla's method, using an instance of <see cref="Geometry3d.OBB"/> as input parameter instead of 2 points.
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
        /// <param name="collection">Object implementing generic <see cref="IEnumerable{T}"/> interface.</param>
        /// <returns>True on success, false otherwise.</returns>
        public static bool Select(this Model.UI.ModelObjectSelector selector, IEnumerable<ModelObject> collection)
        {
            return selector.Select(collection.ToArrayList());
        }

        /// <summary>
        /// Overload of Tekla's method, selects objects using given collection.
        /// </summary>
        /// <param name="selector">Model object selector to select objects in UI.</param>
        /// <param name="collection">Object implementing generic <see cref="IEnumerable{T}"/> interface.</param>
        /// <param name="showDimensions">Defines whether to show dimensions of selected objects.</param>
        /// <returns>True on success, false otherwise.</returns>
        public static bool Select(this Model.UI.ModelObjectSelector selector, IEnumerable<ModelObject> collection, bool showDimensions)
        {
            return selector.Select(collection.ToArrayList(), showDimensions);
        }

        /// <summary>
        /// Tekla's method overload, using coordinate system object instead of 3 points.
        /// Used to get all the intersection points between the solid and a plane. Does not arrange the points into polygons, thus a lot faster.
        /// </summary>
        /// <param name="solid">Solid to be intersected with a plane.</param>
        /// <param name="coordinateSystem">Coordinate system defining a plane.</param>
        /// <returns>Enumerator of points.</returns>
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
        /// <returns>Enumerator of points.</returns>
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
        /// <returns>Enumerator of point lists.</returns>
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
        /// <returns>Enumerator of point lists.</returns>
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
            var partNormalSolid = part.GetSolid();
            var partPlanecuttedSolid = part.GetSolid(Model.Solid.SolidCreationTypeEnum.PLANECUTTED);
            var cuts = part.GetBooleans().ToList<BooleanPart>().Where(c => c.Type == BooleanPart.BooleanTypeEnum.BOOLEAN_CUT);
            var result = new List<BooleanPart>();
            foreach (var cut in cuts)
            {
                // Collect identifiers for cut and its added materials.
                var cutIds = cut.OperativePart.GetBooleans().ToList<BooleanPart>().Where(b => b.Type == BooleanPart.BooleanTypeEnum.BOOLEAN_ADD).Select(b => b.OperativePart.Identifier.ID).ToList();
                cutIds.Add(cut.Identifier.ID);

                // Skip when there are faces in the part created by given cut.
                if (partNormalSolid.GetFaceEnumerator().ToList<Solid.Face>().Any(f => cutIds.Contains(f.OriginPartId.ID))) continue;

                // In some cases Tekla returns invalid IDs and cut is marked as redundant, despite cutting through part.
                // Workaround: execute cutting operation and check if resulting shells have faces created by cutting solid.
                var faces = new List<Solid.Face>();
                partPlanecuttedSolid.GetCutPart(cut.OperativePart.GetSolid()).ToList<Solid.Shell>().ForEach(s => faces.AddRange(s.GetFaceEnumerator().ToList<Solid.Face>()));
                if (faces.Any(f => cutIds.Contains(f.OriginPartId.ID))) continue;
                
                // If both tests couldn't find face created by given cut, therefore cut doesn't go through the part.
                result.Add(cut);
            }
            return result;
        }

        /// <summary>
        /// Creates Tekla's <see cref="Drawing.PointList"/> collection.
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

        /// <summary>
        /// Gets report directory path from given model.
        /// </summary>
        /// <param name="model">Model being in use.</param>
        /// <returns>Report directory path or empty string if failed to retrieve.</returns>
        public static string GetReportDirectory(this Model.Model model)
        {
            string reportDir = string.Empty;
            TeklaStructuresSettings.GetAdvancedOption("XS_REPORT_OUTPUT_DIRECTORY", ref reportDir);
            // If report directory is model-related, then there should be ".\" at the beginning of returned string
            if (reportDir.StartsWith(".\\"))
            {
                reportDir = model.GetInfo().ModelPath + reportDir.Substring(1);
            }
            // Check if report directory path is valid in both model-related and fixed path scenarios
            if (System.IO.Directory.Exists(reportDir)) return reportDir;
            else return string.Empty;
        }

        /// <summary>
        /// Tekla's method overload, using <see cref="Geometry3d.Line"/> as input object.
        /// Returns a list of line - solid intersection points. 
        /// </summary>
        /// <param name="solid">Solid to be intersect.</param>
        /// <param name="line">The intersection line to be used.</param>
        /// <returns>An array list of intersection points.</returns>
        public static ArrayList Intersect(this Model.Solid solid, Geometry3d.Line line)
        {
            var points = new Geometry3d.Point[]
            {
                new Geometry3d.Point(line.Origin),
                new Geometry3d.Point(line.Origin) + line.Direction
            };
            return solid.Intersect(points[0], points[1]);
        }

        /// <summary>
        /// Determines whether cut is located on edge, corner or internal area of its father part.
        /// </summary>
        /// <param name="booleanPart">Tekla boolean part - cut object.</param>
        /// <returns>Location of the cut.</returns>
        public static CutLocationEnum GetCutLocation(this BooleanPart booleanPart)
        {
            if (booleanPart.Type != BooleanPart.BooleanTypeEnum.BOOLEAN_CUT) throw new InvalidOperationException("Boolean part must be a cut.");
            var fatherRawSolid = (booleanPart.Father as Part).GetSolid(Model.Solid.SolidCreationTypeEnum.RAW);
            var solidEdges = fatherRawSolid.GetEdgeEnumerator().ToList<Solid.Edge>();
            var cuttingPartSolid = booleanPart.OperativePart.GetSolid();
            var edgesCut = solidEdges.Where(e => cuttingPartSolid.Intersect(e.StartPoint, e.EndPoint).Count != 0);

            if (edgesCut.Count() == 0) return CutLocationEnum.Internal;
            if (edgesCut.Count() == 1) return CutLocationEnum.Edge;

            // If 2 or more edges are cut, it could be either edge or corner cut.
            // Corner cut will contain a vertex (i.e. point common for at least 2 edges) inside the cutting solid.
            var vertices = new List<Geometry3d.Point>();
            foreach (var edge in edgesCut)
            {
                vertices.Add(edge.StartPoint);
                vertices.Add(edge.EndPoint);
            }
            vertices = vertices.GroupBy(n => n).Where(n => n.Count() > 1).Select(n => n.Key).ToList();
            return vertices.Any(v => v.IsInside(cuttingPartSolid)) ? CutLocationEnum.Corner : CutLocationEnum.Edge;
        }

        /// <summary>
        /// Ray tracing algorithm implementation for Tekla Structures.
        /// </summary>
        /// <param name="point">Point to be checked.</param>
        /// <param name="solid">Solid, against which the point is going to be checked.</param>
        /// <returns>True if point is inside or on the surface of the solid, false otherwise.</returns>
        public static bool IsInside(this Geometry3d.Point point, Model.Solid solid)
        {
            var randomVector = GetRandomVector() * 1000; // Vector length must be increased, otherwise Tekla will not find any intersection points.
            var intersection = solid.Intersect(point, point + randomVector).Cast<Geometry3d.Point>();
            // Tekla does not support rays, a line will be used instead. All points found on opposite direction must be excluded.
            var pointsOnRay = intersection.Where(x =>
            {
                var vectorToIntersectionPoint = new Geometry3d.Vector(x - point);
                var factor = Math.Round(vectorToIntersectionPoint.X / randomVector.X, 3);
                return factor == Math.Round(vectorToIntersectionPoint.Y / randomVector.Y, 3) && factor == Math.Round(vectorToIntersectionPoint.Z / randomVector.Z, 3);
            });
            // If point is inside solid, there will be odd numbers of intersections between solid faces and any ray originated in given point.
            return pointsOnRay.Count() % 2 != 0;
        }
        #endregion
    }
}
