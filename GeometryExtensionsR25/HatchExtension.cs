using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using System.Collections.Generic;

using static System.Math;

namespace Gile.AutoCAD.R25.Geometry
{
    /// <summary>
    /// Provides extension methods for the Hatch type.
    /// </summary>
    public static class HatchExtension
    {
        /// <summary>
        /// Gets the hatch boundary.
        /// </summary>
        /// <param name="hatch">The instance to which this method applies.</param>
        /// <returns>The list of the boundary curves.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentNullException is thrown if <paramref name="hatch"/> is null.</exception>
        public static List<Curve> GetBoundary(this Hatch hatch)
        {
            System.ArgumentNullException.ThrowIfNull(hatch);
            var result = new List<Curve>();
            for (int i = 0; i < hatch.NumberOfLoops; i++)
            {
                var loop = hatch.GetLoopAt(i);
                if (loop.IsPolyline)
                {
                    var bulges = loop.Polyline;
                    var pline = new Polyline(bulges.Count);
                    for (int j = 0; j < bulges.Count; j++)
                    {
                        var vertex = bulges[j];
                        pline.AddVertexAt(j, vertex.Vertex, vertex.Bulge, 0.0, 0.0);
                    }
                    pline.Elevation = hatch.Elevation;
                    pline.Normal = hatch.Normal;
                    result.Add(pline);
                }
                else
                {
                    var plane = hatch.GetPlane();
                    var curves = loop.Curves;
                    foreach (Curve2d curve in curves)
                    {
                        switch (curve)
                        {
                            case LineSegment2d lineSegment:
                                var line = new Line(
                                    lineSegment.StartPoint.Convert3d(plane),
                                    lineSegment.EndPoint.Convert3d(plane));
                                result.Add(line);
                                break;
                            case CircularArc2d circularArc:
                                double startAngle =
                                    circularArc.IsClockWise ? -circularArc.EndAngle : circularArc.StartAngle;
                                double endAngle =
                                    circularArc.IsClockWise ? -circularArc.StartAngle : circularArc.EndAngle;
                                var arc = new Arc(
                                    circularArc.Center.Convert3d(plane),
                                    hatch.Normal,
                                    circularArc.Radius,
                                    circularArc.ReferenceVector.Angle + startAngle,
                                    circularArc.ReferenceVector.Angle + endAngle);
                                result.Add(arc);
                                break;
                            case EllipticalArc2d ellipticalArc:
                                double majorRadius = ellipticalArc.MajorRadius;
                                double minorRadius = ellipticalArc.MinorRadius;
                                double ratio = minorRadius / majorRadius;
                                double startParam =
                                    ellipticalArc.IsClockWise ? -ellipticalArc.EndAngle : ellipticalArc.StartAngle;
                                double endParam =
                                    ellipticalArc.IsClockWise ? -ellipticalArc.StartAngle : ellipticalArc.EndAngle;
                                var ellipse = new Ellipse(
                                    ellipticalArc.Center.Convert3d(plane),
                                    hatch.Normal,
                                    ellipticalArc.MajorAxis.Convert3d(plane) * majorRadius,
                                    ratio,
                                    Atan2(Sin(startParam) * minorRadius, Cos(startParam) * majorRadius),
                                    Atan2(Sin(endParam) * minorRadius, Cos(endParam) * majorRadius));
                                result.Add(ellipse);
                                break;
                            case NurbCurve2d nurbCurve:
                                var points = new Point3dCollection();
                                for (int j = 0; j < nurbCurve.NumControlPoints; j++)
                                {
                                    points.Add(nurbCurve.GetControlPointAt(j).Convert3d(plane));
                                }
                                var knots = new DoubleCollection();
                                for (int k = 0; k < nurbCurve.NumKnots; k++)
                                {
                                    knots.Add(nurbCurve.GetKnotAt(k));
                                }
                                var weights = new DoubleCollection();
                                for (int l = 0; l < nurbCurve.NumWeights; l++)
                                {
                                    weights.Add(nurbCurve.GetWeightAt(l));
                                }
                                var spline = new Spline(
                                    nurbCurve.Degree,
                                    nurbCurve.IsRational,
                                    nurbCurve.IsClosed(),
                                    false,
                                    points,
                                    knots,
                                    weights,
                                    0.0,
                                    0.0);
                                result.Add(spline);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return result;
        }
    }
}
