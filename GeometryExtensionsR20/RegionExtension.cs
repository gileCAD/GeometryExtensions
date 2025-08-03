using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using System.Collections.Generic;
using System.Linq;

using static System.Math;

namespace Gile.AutoCAD.R20.Geometry
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
            Assert.IsNotNull(region, nameof(region));

            using (var brep = new Brep(region))
            {
                var face = brep.Faces.First();
                var plane = (Plane)((ExternalBoundedSurface)face.Surface).BaseSurface;
                var coordinateSystem = plane.GetCoordinateSystem();
                var origin = coordinateSystem.Origin;
                var xAxis = coordinateSystem.Xaxis;
                var yAxis = coordinateSystem.Yaxis;
                return region
                    .AreaProperties(ref origin, ref xAxis, ref yAxis)
                    .Centroid
                    .Convert3d(plane);
            }
        }

        /// <summary>
        /// Gets the distance of the Region's plane from the WCS origin.
        /// </summary>
        /// <param name="region">The instance to which this method applies.</param>
        /// <returns>The elevation of the Region.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="region"/> is null.</exception>
        public static double Elevation(this Region region)
        {
            Assert.IsNotNull(region, nameof(region));

            using (var brep = new Brep(region))
            {
                return brep.Vertices.First().Point.TransformBy(Matrix3d.WorldToPlane(region.Normal)).Z;
            }
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
            Assert.IsNotNull(region, nameof(region));

            if (tolerance.Equals(default(Tolerance)))
                tolerance = Tolerance.Global;

            using (var brep = new Brep(region))
            {
                foreach (var loop in brep.Faces.SelectMany(face => face.Loops))
                {
                    var curves3d = loop.GetNativeCurves().ToArray();
                    if (curves3d.Length == 1)
                    {
                        yield return Curve.CreateFromGeCurve(curves3d[0]);
                    }
                    else if (curves3d.TryConvertToCompositeCurve(out CompositeCurve3d compositeCurve, tolerance, c => c is LineSegment3d || c is CircularArc3d))
                    {
                        yield return (Polyline)Curve.CreateFromGeCurve(compositeCurve);
                    }
                    else
                    {
                        foreach (Curve3d curve3d in curves3d)
                        {
                            yield return Curve.CreateFromGeCurve(curve3d);
                        }
                    }
                }
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
            Assert.IsNotNull(region, nameof(region));

            if (tolerance.Equals(default(Tolerance)))
                tolerance = Tolerance.Global;

            using (var brep = new Brep(region))
            {
                foreach (var loop in brep.Faces.SelectMany(f => f.Loops))
                {
                    var curves3d = loop.GetNativeCurves().ToArray();
                    if (curves3d.Length == 1)
                    {
                        yield return (loop.LoopType, new[] { Curve.CreateFromGeCurve(curves3d[0]) });
                    }
                    else if (curves3d.TryConvertToCompositeCurve(out CompositeCurve3d compositeCurve, tolerance, c => c is LineSegment3d || c is CircularArc3d))
                    {
                        yield return (loop.LoopType, new[] { (Polyline)Curve.CreateFromGeCurve(compositeCurve) });
                    }
                    else
                    {
                        yield return (loop.LoopType, curves3d.Select(c => Curve.CreateFromGeCurve(c)).ToArray());
                    }
                }
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
            Assert.IsNotNull(region, nameof(region));

            if (tolerance.Equals(default(Tolerance)))
                tolerance = Tolerance.Global;

            var plane = new Plane(Point3d.Origin, region.Normal);

            double twoPI = PI * 2.0;

            double Standardise(double angle) =>
                angle < 0 ? angle + twoPI :
                twoPI < angle ? angle - twoPI :
                angle;

            using (var brep = new Brep(region))
            {
                foreach (var complex in brep.Complexes)
                {
                    foreach (var shell in complex.Shells)
                    {
                        foreach (var face in shell.Faces)
                        {
                            foreach (var loop in face.Loops)
                            {
                                var edgePtrCollection = new Curve2dCollection();
                                var edgeTypeCollection = new IntegerCollection();
                                foreach (var curve3d in loop.GetNativeCurves().ToOrderedArray(tolerance))
                                {
                                    switch (curve3d)
                                    {
                                        case LineSegment3d lineSegment3D:
                                            edgePtrCollection.Add(
                                                new LineSegment2d(
                                                    lineSegment3D.StartPoint.Convert2d(plane),
                                                    lineSegment3D.EndPoint.Convert2d(plane)));
                                            edgeTypeCollection.Add(1);
                                            break;
                                        case CircularArc3d circularArc3D:
                                            if (circularArc3D.EndAngle - circularArc3D.StartAngle == twoPI)
                                            {
                                                edgePtrCollection.Add(
                                                    new CircularArc2d(
                                                        circularArc3D.Center.Convert2d(plane),
                                                        circularArc3D.Radius));
                                            }
                                            else
                                            {
                                                bool isClockwise = circularArc3D.Normal.IsEqualTo(region.Normal.Negate());
                                                double angle = isClockwise ?
                                                    -circularArc3D.ReferenceVector.Convert2d(plane).Angle :
                                                    circularArc3D.ReferenceVector.Convert2d(plane).Angle;
                                                edgePtrCollection.Add(
                                                    new CircularArc2d(
                                                        circularArc3D.Center.Convert2d(plane),
                                                        circularArc3D.Radius,
                                                        Standardise(circularArc3D.StartAngle + angle),
                                                        Standardise(circularArc3D.EndAngle + angle),
                                                        Vector2d.XAxis,
                                                        isClockwise));
                                            }
                                            edgeTypeCollection.Add(2);
                                            break;
                                        case EllipticalArc3d ellipticalArc3D:
                                            edgePtrCollection.Add(
                                                new EllipticalArc2d(
                                                    ellipticalArc3D.Center.Convert2d(plane),
                                                    ellipticalArc3D.MajorAxis.Convert2d(plane),
                                                    ellipticalArc3D.MinorAxis.Convert2d(plane),
                                                    ellipticalArc3D.MajorRadius,
                                                    ellipticalArc3D.MinorRadius,
                                                    ellipticalArc3D.StartAngle,
                                                    ellipticalArc3D.EndAngle));
                                            edgeTypeCollection.Add(3);
                                            break;
                                        case NurbCurve3d nurbCurve3D:
                                            var ctrlPts = new Point2dCollection();
                                            for (int i = 0; i < nurbCurve3D.NumberOfControlPoints; i++)
                                            {
                                                ctrlPts.Add(nurbCurve3D.ControlPointAt(i).Convert2d(plane));
                                            }
                                            edgePtrCollection.Add(
                                                new NurbCurve2d(
                                                    nurbCurve3D.Degree,
                                                    nurbCurve3D.Knots,
                                                    ctrlPts,
                                                    nurbCurve3D.IsPeriodic(out double _)));
                                            edgeTypeCollection.Add(4);
                                            break;
                                        default:
                                            break;
                                    }
                                }

                                if (loop.LoopType == LoopType.LoopExterior)
                                    yield return (HatchLoopTypes.External, edgePtrCollection, edgeTypeCollection);
                                else
                                    yield return (HatchLoopTypes.Default, edgePtrCollection, edgeTypeCollection);
                            }
                        }
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
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="region"/> is null.</exception>
        public static PointContainment GetPointContainment(this Region region, Point3d point)
        {
            Assert.IsNotNull(region, nameof(region));
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
