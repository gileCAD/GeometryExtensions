using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using System.Collections.Generic;
using System.Linq;

using static System.Math;

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

            if (tolerance.Equals(default(Tolerance)))
                tolerance = Tolerance.Global;

            using var brep = new Brep(region);
            foreach (var loop in brep.Faces.SelectMany(face => face.Loops))
            {
                var curves3d = loop.Edges.Select(edge => ((ExternalCurve3d)edge.Curve).NativeCurve);
                if (!curves3d.Skip(1).Any())
                {
                    yield return Curve.CreateFromGeCurve(curves3d.First());
                }
                else if (curves3d.TryConvertToCompositeCurve(out CompositeCurve3d? compositeCurve, tolerance, c => c is LineSegment3d || c is CircularArc3d))
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

            if (tolerance.Equals(default(Tolerance)))
                tolerance = Tolerance.Global;

            using var brep = new Brep(region); 
            foreach (var loop in brep.Faces.SelectMany(f => f.Loops))
            {
                var curves3d = loop.Edges.Select(edge => ((ExternalCurve3d)edge.Curve).NativeCurve);
                if (!curves3d.Skip(1).Any())
                {
                    yield return (loop.LoopType, [Curve.CreateFromGeCurve(curves3d.First())]);
                }
                else if (curves3d.TryConvertToCompositeCurve(out CompositeCurve3d? compositeCurve, tolerance, c => c is LineSegment3d || c is CircularArc3d))
                {
                    yield return (loop.LoopType, new[] { (Polyline)Curve.CreateFromGeCurve(compositeCurve) });
                }
                else
                {
                    yield return (loop.LoopType, curves3d.Select(c => Curve.CreateFromGeCurve(c)).ToArray());
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
            System.ArgumentNullException.ThrowIfNull(region);

            if (tolerance.Equals(default(Tolerance)))
                tolerance = Tolerance.Global;

            var plane = new Plane(Point3d.Origin, region.Normal);

            double twoPI = PI * 2.0;

            double Standardise(double angle) =>
                angle < 0 ? angle + twoPI :
                twoPI < angle ? angle - twoPI :
                angle;

            using var brep = new Brep(region);
            foreach (var complex in brep.Complexes)
            {
                foreach (var loop in complex.Shells.First().Faces.First().Loops)
                {
                    var edgePtrCollection = new Curve2dCollection();
                    var edgeTypeCollection = new IntegerCollection();
                    foreach (var edge in loop.Edges.Select(e => ((ExternalCurve3d)e.Curve).NativeCurve).ToOrderedArray(tolerance))
                    {
                        switch (edge)
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
