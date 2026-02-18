using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using System.Collections.Generic;
using System.Linq;

namespace Gile.AutoCAD.R25.Geometry
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
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="region"/> is null.</exception>
        public static Point3d Centroid(this Region region)
        {
            System.ArgumentNullException.ThrowIfNull(region);
            var plane = region.GetPlane();
            var coordinateSystem = plane.GetCoordinateSystem();
            var origin = coordinateSystem.Origin;
            var xAxis = coordinateSystem.Xaxis;
            var yAxis = coordinateSystem.Yaxis;
            return region
                .AreaProperties(ref origin, ref xAxis, ref yAxis)
                .Centroid
                .Convert3d(plane);
        }

        /// <summary>
        /// Gets the distance of the Region's plane from the WCS origin.
        /// </summary>
        /// <param name="region">The instance to which this method applies.</param>
        /// <returns>The elevation of the Region.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="region"/> is null.</exception>
        public static double Elevation(this Region region)
        {
            System.ArgumentNullException.ThrowIfNull(region);
            return region.GetPlane().PointOnPlane.TransformBy(Matrix3d.WorldToPlane(region.Normal)).Z;
        }

        /// <summary>
        /// Gets the curves constituting the boundaries of the region.
        /// </summary>
        /// <param name="region">The instance to which this method applies.</param>
        /// <param name="tolerance">Tolerance used in curve end points comparison.</param>
        /// <returns>Curve collection.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="region"/> is null.</exception>
        public static IEnumerable<Curve> GetCurves(this Region region, Tolerance tolerance = default)
        {
            System.ArgumentNullException.ThrowIfNull(region);

            using var brep = new Brep(region);
            foreach (var curve in brep.Faces.SelectMany(face => face.Loops).SelectMany(loop => loop.GetCurves(tolerance)))
            {
                yield return curve;
            }
        }

        /// <summary>
        /// Gets the curves constituting the boundaries of the region by loop.
        /// </summary>
        /// <param name="region">The instance to which this method applies.</param>
        /// <param name="tolerance">Tolerance used to compare end points.</param>
        /// <returns>A sequence containing one tuple (LoopType, Curve[]) for each loop.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="region"/> is null.</exception>
        public static IEnumerable<(LoopType, Curve[])> GetCurvesByLoop(this Region region, Tolerance tolerance = default)
        {
            System.ArgumentNullException.ThrowIfNull(region);

            using var brep = new Brep(region);
            foreach (var loop in brep.Faces.SelectMany(face => face.Loops))
            {
                yield return (loop.LoopType, loop.GetCurves(tolerance).ToArray());
            }
        }

        /// <summary>
        /// Gets the hatch loops data for the supplied region.
        /// </summary>
        /// <param name="region">The instance to which this method applies.</param>
        /// <param name="tolerance">Tolerance used in curve end points comparison.</param>
        /// <returns>A collection of tuples containing the loop data.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="region"/> is null.</exception>
        public static IEnumerable<(HatchLoopTypes, Curve2dCollection, IntegerCollection)> GetHatchLoops(this Region region, Tolerance tolerance = default)
        {
            System.ArgumentNullException.ThrowIfNull(region);

            using var brep = new Brep(region);
            foreach (var item in brep.Faces.SelectMany(face => face.GetHatchLoops(tolerance)))
            {
                yield return item;
            }
        }

        /// <summary>
        /// Gets the PointContainment of the region for the supplied point. 
        /// </summary>
        /// <param name="region">The instance to which this method applies.</param>
        /// <param name="point">The point to be evaluated.</param>
        /// <returns>The PointContainment value.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="region"/> is null.</exception>
        public static PointContainment GetPointContainment(this Region region, Point3d point)
        {
            System.ArgumentNullException.ThrowIfNull(region);
            using Brep brep = new(region);
            using BrepEntity entity = brep.GetPointContainment(point, out PointContainment result);
            return entity switch
            {
                Autodesk.AutoCAD.BoundaryRepresentation.Face _ => PointContainment.Inside,
                Edge _ => PointContainment.OnBoundary,
                _ => PointContainment.Outside,
            };
        }
    }
}
