using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using System;
using System.Collections.Generic;

using AcBr = Autodesk.AutoCAD.BoundaryRepresentation;

namespace Gile.AutoCAD.R25.Geometry
{
    /// <summary>
    /// Provides extension methods for the BoundaryRepresentation.Face type.
    /// </summary>
    public static class BrepFaceExtension
    {
        /// <summary>
        /// Gets the hatch loops data for the supplied Face.
        /// </summary>
        /// <param name="face">The instance to which this method applies.</param>
        /// <param name="tolerance">Tolerance used in curve end points comparison.</param>
        /// <returns>A collection of tuples containing the loop data.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="face"/> is null.</exception>
        public static IEnumerable<(HatchLoopTypes, Curve2dCollection, IntegerCollection)> GetHatchLoops(
            this AcBr.Face face,
            Tolerance tolerance = default)
        {
            ArgumentNullException.ThrowIfNull(face);

            if (tolerance.Equals(default(Tolerance)))
                tolerance = Tolerance.Global;

            var boundedSurface = (ExternalBoundedSurface)face.Surface;
            if (!boundedSurface.IsPlane)
                throw new ArgumentException("Face non plane");

            var normal = ((Plane)boundedSurface.BaseSurface).Normal;
            var plane = new Plane(Point3d.Origin, normal);

            double twoPI = Math.PI * 2.0;

            double Standardise(double angle) =>
                angle < 0 ? angle + twoPI :
                twoPI < angle ? angle - twoPI :
                angle;

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
                                bool isClockwise = circularArc3D.Normal.IsEqualTo(normal.Negate());
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
