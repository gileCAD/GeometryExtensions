using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using System.Collections.Generic;
using System.Linq;

namespace Gile.AutoCAD.Geometry
{
    /// <summary>
    /// Provides extension methods for the Region type.
    /// </summary>
    public static class RegionExtension
    {
        /// <summary>
        /// Gets the Centroid of the Region (WCS coordinates).
        /// </summary>
        /// <param name="region">The instance to which this method applies.</param>
        /// <returns>The centroid of the Region.</returns>
        public static Point3d Centroid(this Region region)
        {
            var plane = region.GetPlane();
            var coordinateSystem = plane.GetCoordinateSystem();
            var origin = coordinateSystem.Origin;
            var xAxis = coordinateSystem.Xaxis;
            var yAxis = coordinateSystem.Yaxis;
            return region
                .AreaProperties(ref origin, ref xAxis, ref yAxis)
                .Centroid
                .Convert3d(plane); ;
        }

        /// <summary>
        /// Gets the curves constituting the boundaries of the region.
        /// </summary>
        /// <param name="region">The region this method applies to.</param>
        /// <returns>Curve collection.</returns>
        public static IEnumerable<Curve> GetCurves(this Region region)
        {
            using (var brep = new Brep(region))
            {
                var loops = brep.Faces.SelectMany(face => face.Loops);
                foreach (var loop in loops)
                {
                    var curves3d = loop.Edges.Select(edge => ((ExternalCurve3d)edge.Curve).NativeCurve);
                    if (1 < curves3d.Count())
                    {
                        if (curves3d.All(curve3d => curve3d is CircularArc3d || curve3d is LineSegment3d))
                        {
                            var pline = (Polyline)Curve.CreateFromGeCurve(new CompositeCurve3d(curves3d.ToOrderedArray()));
                            pline.Closed = true;
                            yield return pline;
                        }
                        else
                        {
                            foreach (Curve3d curve3d in curves3d)
                            {
                                yield return Curve.CreateFromGeCurve(curve3d);
                            }
                        }
                    }
                    else
                    {
                        yield return Curve.CreateFromGeCurve(curves3d.First());
                    }
                }
            }
        }

        /// <summary>
        /// Gets the PointContainment of the region for the supplied point. 
        /// </summary>
        /// <param name="region">The instance to which this method applies.</param>
        /// <param name="point">The point to be evaluated.</param>
        /// <returns>The PointContainment value.</returns>
        public static PointContainment GetPointContainment(this Region region, Point3d point)
        {
            using (Brep brep = new Brep(region))
            using (BrepEntity entity = brep.GetPointContainment(point, out PointContainment result))
            {
                switch (entity)
                {
                    case Autodesk.AutoCAD.BoundaryRepresentation.Face _:
                        return PointContainment.Inside;
                    case Edge _:
                        return PointContainment.OnBoundary;
                    default:
                        return PointContainment.Outside;
                }
            }
        }
    }
}
