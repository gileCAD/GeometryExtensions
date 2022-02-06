using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Gile.AutoCAD.Geometry
{
    /// <summary>
    /// Provides extension methods for the Polyline type.
    /// </summary>
    public static class PolylineExtension
    {
        /// <summary>
        /// Cuts the Polyline at specified point (closest point if the point does not lies on the polyline).
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <param name="brkPt">the point where to cut the Polyline.</param>
        /// <returns>An array containig the two resulting polylines.</returns>
        public static Polyline[] BreakAtPoint(this Polyline pline, Point3d brkPt)
        {
            brkPt = pline.GetClosestPointTo(brkPt, false);

            if (brkPt.IsEqualTo(pline.StartPoint))
                return new Polyline[2] { null, (Polyline)pline.Clone() };

            if (brkPt.IsEqualTo(pline.EndPoint))
                return new Polyline[2] { (Polyline)pline.Clone(), null };

            double param = pline.GetParameterAtPoint(brkPt);
            int index = (int)param;
            int num = pline.NumberOfVertices;
            Polyline pl1 = (Polyline)pline.Clone();
            if (pline.Closed)
            {
                pl1.AddVertexAt(
                    pline.NumberOfVertices,
                    pline.GetPoint2dAt(0),
                    pline.GetStartWidthAt(num - 1),
                    pline.GetEndWidthAt(num - 1),
                    pline.GetBulgeAt(num - 1));
                pl1.Closed = false;
            }
            Polyline pl2 = (Polyline)pl1.Clone();

            if (Math.Round(param, 6) == index)
            {
                for (int i = pl1.NumberOfVertices - 1; i > index; i--)
                {
                    pl1.RemoveVertexAt(i);
                }
                for (int i = 0; i < index; i++)
                {
                    pl2.RemoveVertexAt(0);
                }
                return new Polyline[2] { pl1, pl2 };
            }

            Point2d pt = brkPt.Convert2d(new Plane(Point3d.Origin, pline.Normal));
            for (int i = pl1.NumberOfVertices - 1; i > index + 1; i--)
            {
                pl1.RemoveVertexAt(i);
            }
            pl1.SetPointAt(index + 1, pt);
            for (int i = 0; i < index; i++)
            {
                pl2.RemoveVertexAt(0);
            }
            pl2.SetPointAt(0, pt);
            if (pline.GetBulgeAt(index) != 0.0)
            {
                double bulge = pline.GetBulgeAt(index);
                pl1.SetBulgeAt(index, ScaleBulge(bulge, param - index));
                pl2.SetBulgeAt(0, ScaleBulge(bulge, index + 1 - param));
            }
            return new Polyline[2] { pl1, pl2 };
        }

        /// <summary>
        /// Gets the Polyline centroid.
        /// </summary>
        /// <param name="pl">The instance to which this method applies.</param>
        /// <returns>The Polyline centroid (OCS coordinates).</returns>
        public static Point2d Centroid2d(this Polyline pl)
        {
            Point2d cen = new Point2d();
            Triangle2d tri = new Triangle2d();
            CircularArc2d arc = new CircularArc2d();
            double tmpArea;
            double area = 0.0;
            int last = pl.NumberOfVertices - 1;
            Point2d p0 = pl.GetPoint2dAt(0);

            if (pl.GetSegmentType(0) == SegmentType.Arc)
            {
                arc = pl.GetArcSegment2dAt(0);
                area = arc.SignedArea();
                cen = arc.Centroid() * area;
            }
            for (int i = 1; i < last; i++)
            {
                tri = new Triangle2d(p0, pl.GetPoint2dAt(i), pl.GetPoint2dAt(i + 1));
                tmpArea = tri.SignedArea;
                cen += (tri.Centroid * tmpArea).GetAsVector();
                area += tmpArea;
                if (pl.GetSegmentType(i) == SegmentType.Arc)
                {
                    arc = pl.GetArcSegment2dAt(i);
                    tmpArea = arc.SignedArea();
                    area += tmpArea;
                    cen += (arc.Centroid() * tmpArea).GetAsVector();
                }
            }
            if ((pl.GetSegmentType(0) == SegmentType.Arc) && (pl.Closed == true))
            {
                arc = pl.GetArcSegment2dAt(last);
                tmpArea = arc.SignedArea();
                area += tmpArea;
                cen += (arc.Centroid() * tmpArea).GetAsVector();
            }
            return cen.DivideBy(area);
        }

        /// <summary>
        /// Gets the Polyline centroid.
        /// </summary>
        /// <param name="pl">The instance to which this method applies.</param>
        /// <returns>The Polyline centroid (WCS coordinates).</returns>
        public static Point3d Centroid(this Polyline pl)
        {
            return pl.Centroid2d().Convert3d(pl.Normal, pl.Elevation);
        }

        /// <summary>
        /// Adds an arc (fillet), when possible, at each vertex.
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <param name="radius">The arc radius.</param>
        public static void FilletAll(this Polyline pline, double radius)
        {
            int n = pline.Closed ? 0 : 1;
            for (int i = n; i < pline.NumberOfVertices - n; i += 1 + pline.FilletAt(i, radius))
            { }
        }

        /// <summary>
        /// Adds an arc (fillet), when possible, at specified vertex.
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <param name="index">The vertex index.</param>
        /// <param name="radius">The arc radius.</param>
        /// <returns>1, if the operation succeded; 0, if it failed</returns>
        public static int FilletAt(this Polyline pline, int index, double radius)
        {
            int prev = index == 0 && pline.Closed ? pline.NumberOfVertices - 1 : index - 1;
            if (pline.GetSegmentType(prev) != SegmentType.Line ||
                pline.GetSegmentType(index) != SegmentType.Line)
            {
                return 0;
            }
            LineSegment2d seg1 = pline.GetLineSegment2dAt(prev);
            LineSegment2d seg2 = pline.GetLineSegment2dAt(index);
            Vector2d vec1 = seg1.StartPoint - seg1.EndPoint;
            Vector2d vec2 = seg2.EndPoint - seg2.StartPoint;
            double angle = (Math.PI - vec1.GetAngleTo(vec2)) / 2.0;
            double dist = radius * Math.Tan(angle);
            if (dist == 0.0 || dist > seg1.Length || dist > seg2.Length)
            {
                return 0;
            }
            Point2d pt1 = seg1.EndPoint + vec1.GetNormal() * dist;
            Point2d pt2 = seg2.StartPoint + vec2.GetNormal() * dist;
            double bulge = Math.Tan(angle / 2.0);
            if (Clockwise(seg1.StartPoint, seg1.EndPoint, seg2.EndPoint))
            {
                bulge = -bulge;
            }
            pline.AddVertexAt(index, pt1, bulge, 0.0, 0.0);
            pline.SetPointAt(index + 1, pt2);
            return 1;
        }

        /// <summary>
        /// Creates a new Polyline which is the result of the projection of the Polyline on the specified plane in the specified direction. 
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <param name="plane">The projection plane.</param>
        /// <param name="direction">The projection direction (WCS coordinates).</param>
        /// <returns>The projected Polyline.</returns>
        public static Polyline GetProjectedPolyline(this Polyline pline, Plane plane, Vector3d direction)
        {
            Tolerance tol = new Tolerance(1e-9, 1e-9);
            if (plane.Normal.IsPerpendicularTo(direction, tol))
                return null;

            if (pline.Normal.IsPerpendicularTo(direction, tol))
            {
                Plane dirPlane = new Plane(Point3d.Origin, direction);
                if (!pline.IsWriteEnabled) pline.UpgradeOpen();
                pline.TransformBy(Matrix3d.WorldToPlane(dirPlane));
                Extents3d extents = pline.GeometricExtents;
                pline.TransformBy(Matrix3d.PlaneToWorld(dirPlane));
                return GeometryExtension.ProjectExtents(extents, plane, direction, dirPlane);
            }

            return GeometryExtension.ProjectPolyline(pline, plane, direction);
        }

        /// <summary>
        /// Creates a new Polyline which is the result of the orthogonal projection of the Polyline on the specified plane.
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <param name="plane">The projection plane.</param>
        /// <returns>The projected Polyline.</returns>
        public static Polyline GetOrthoProjectedPolyline(this Polyline pline, Plane plane) => 
            pline.GetProjectedPolyline(plane, plane.Normal);

        /// <summary>
        /// Defines the way the point is contained. 
        /// </summary>
        public enum PointContainment
        {
            /// <summary>
            /// The point is inside the boundary.
            /// </summary>
            Inside,

            /// <summary>
            /// The point is outside the boundary.
            /// </summary>
            OutSide,

            /// <summary>
            /// The point is on the boundary.
            /// </summary>
            OnBoundary
        }

        /// <summary>
        /// Evaluates if the point is inside, outside or on the Polyline using Tolerance.Global.
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <param name="point">The point to evaluate.</param>
        /// <returns>A value of PointContainment.</returns>
        public static PointContainment GetPointContainment(this Polyline pline, Point3d point)
        {
            return pline.GetPointContainment(point, Tolerance.Global.EqualPoint);
        }

        /// <summary>
        /// Evaluates if the point is inside, outside or on the Polyline using the specified Tolerance.
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <param name="point">The point to evaluate.</param>
        /// <param name="tolerance">The tolerance used for comparison.</param>
        /// <returns>A value of PointContainment.</returns>
        public static PointContainment GetPointContainment(this Polyline pline, Point3d point, double tolerance)
        {
            if (pline == null)
                throw new ArgumentNullException("pline");

            if (!pline.Closed)
                throw new InvalidOperationException("Polyline must be closed");

            string filename = "AcMPolygonObj" + Application.Version.Major + ".dbx";
            if (!SystemObjects.DynamicLinker.IsModuleLoaded(filename))
                SystemObjects.DynamicLinker.LoadModule(filename, false, false);

            using (MPolygon mPolygon = new MPolygon())
            {
                mPolygon.AppendLoopFromBoundary(pline, false, tolerance);
                mPolygon.Elevation = pline.Elevation;
                mPolygon.Normal = pline.Normal;
                for (int i = 0; i < mPolygon.NumMPolygonLoops; i++)
                {
                    if (mPolygon.IsPointOnLoopBoundary(point, i, tolerance))
                        return PointContainment.OnBoundary;
                }
                if (mPolygon.IsPointInsideMPolygon(point, tolerance).Count == 1)
                    return PointContainment.Inside;
                return PointContainment.OutSide;
            }
        }

        /// <summary>
        /// Applies a scale factor to a bulge value.
        /// </summary>
        /// <param name="bulge">The bulge value.</param>
        /// <param name="factor">The scale factor.</param>
        /// <returns>The scaled bulge value.</returns>
        public static double ScaleBulge(double bulge, double factor)
        {
            return Math.Tan(Math.Atan(bulge) * factor);
        }

        /// <summary>
        /// Converts the Polyline into a Spline.
        /// </summary>
        /// <param name="polyline">The instance to which this method applies.</param>
        /// <returns>The newly created instance of Spline.</returns>
        public static Spline ToSpline(this Polyline polyline)
        {
            using (Polyline2d poly2d = polyline.ConvertTo(false))
            {
                return poly2d.Spline;
            }
        }

        /// <summary>
        /// Evaluates if the points are clockwise.
        /// </summary>
        /// <param name="p1">First point.</param>
        /// <param name="p2">Second point</param>
        /// <param name="p3">Third point</param>
        /// <returns>true, if the points are clockwise; false, otherwise.</returns>
        private static bool Clockwise(Point2d p1, Point2d p2, Point2d p3)
        {
            return ((p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X)) < 1e-8;
        }
    }
}
