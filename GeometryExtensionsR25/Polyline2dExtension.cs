using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using System;
using System.Collections.Generic;

using AcRx = Autodesk.AutoCAD.Runtime;

namespace Gile.AutoCAD.R25.Geometry
{
    /// <summary>
    /// Provides extension methods for the Polyline2d type.
    /// </summary>
    public static class Polyline2dExtension
    {
        /// <summary>
        /// Gets the list of verices.
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <returns>The list of vertices.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="pline"/> is null.</exception>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        /// eNoActiveTransactions is thrown if the lethod is called outside of a Transaction.</exception>
        public static List<Vertex2d> GetVertices(this Polyline2d pline)
        {
            ArgumentNullException.ThrowIfNull(pline);
            Transaction tr = 
                pline.Database.TransactionManager.TopTransaction ?? 
                throw new AcRx.Exception(AcRx.ErrorStatus.NoActiveTransactions);
            List<Vertex2d> vertices = [];
            foreach (ObjectId id in pline)
            {
                Vertex2d vx = (Vertex2d)tr.GetObject(id, OpenMode.ForRead);
                if (vx.VertexType != Vertex2dType.SplineControlVertex)
                    vertices.Add(vx);
            }
            return vertices;
        }

        /// <summary>
        /// Gets the linear 3D segment at specified index.
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <param name="index">The segment index.</param>
        /// <returns>An instance of LineSegment3d (WCS coordinates).</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="pline"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException if <paramref name="index"/> is out of the indices range.</exception>
        public static LineSegment3d GetLineSegmentAt(this Polyline2d pline, int index)
        {
            ArgumentNullException.ThrowIfNull(pline);
            try
            {
                return new LineSegment3d(
                    pline.GetPointAtParameter(index),
                    pline.GetPointAtParameter(index + 1));
            }
            catch
            {
                throw new ArgumentOutOfRangeException(nameof(pline), "Out of range index");
            }
        }

        /// <summary>
        /// Gets the linear 2D segment at specified index.
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <param name="index">The segment index.</param>
        /// <returns>An instance of LineSegment2d (OCS coordinates).</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="pline"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException if the index id out of the indices range.</exception>
        public static LineSegment2d GetLineSegment2dAt(this Polyline2d pline, int index)
        {
            ArgumentNullException.ThrowIfNull(pline);
            try
            {
                Matrix3d WCS2ECS = pline.Ecs.Inverse();
                return new LineSegment2d(
                    pline.GetPointAtParameter(index).TransformBy(WCS2ECS).Convert2d(),
                    pline.GetPointAtParameter(index + 1.0).TransformBy(WCS2ECS).Convert2d());
            }
            catch
            {
                throw new ArgumentOutOfRangeException(nameof(pline), "Out of range index");
            }
        }

        /// <summary>
        /// Gets the circular arc 3D segment at specified index.
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <param name="index">The segment index.</param>
        /// <returns>An instance of CircularArc3d (WCS coordinates).</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="pline"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException if the index id out of the indices range.</exception>
        public static CircularArc3d GetArcSegmentAt(this Polyline2d pline, int index)
        {
            ArgumentNullException.ThrowIfNull(pline);
            try
            {
                return new CircularArc3d(
                    pline.GetPointAtParameter(index),
                    pline.GetPointAtParameter(index + 0.5),
                    pline.GetPointAtParameter(index + 1));
            }
            catch
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Gets the circular arc 2D segment at specified index.
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <param name="index">The segment index.</param>
        /// <returns>An instance of CircularArc2d (OCS coordinates).</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="pline"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException if the index id out of the indices range.</exception>
        public static CircularArc2d GetArcSegment2dAt(this Polyline2d pline, int index)
        {
            ArgumentNullException.ThrowIfNull(pline);
            try
            {
                Matrix3d WCS2ECS = pline.Ecs.Inverse();
                return new CircularArc2d(
                    pline.GetPointAtParameter(index).TransformBy(WCS2ECS).Convert2d(),
                    pline.GetPointAtParameter(index + 0.5).TransformBy(WCS2ECS).Convert2d(),
                    pline.GetPointAtParameter(index + 1.0).TransformBy(WCS2ECS).Convert2d());
            }
            catch
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Gets the centroid.
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <returns>The centroid (WCS coordinates).</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="pline"/> is null.</exception>
        public static Point3d Centroid(this Polyline2d pline)
        {
            ArgumentNullException.ThrowIfNull(pline);
            Vertex2d[] vertices = [.. pline.GetVertices()];
            int last = vertices.Length - 1;
            Vertex2d vertex = vertices[0];
            Point2d p0 = vertex.Position.Convert2d();
            Point2d cen = new(0.0, 0.0);
            double area = 0.0;
            double bulge = vertex.Bulge;
            double tmpArea;
            Point2d tmpPt;
            Triangle2d tri;
            CircularArc2d arc;
            if (bulge != 0.0)
            {
                arc = pline.GetArcSegment2dAt(0);
                tmpArea = arc.SignedArea();
                tmpPt = arc.Centroid();
                area += tmpArea;
                cen += (new Point2d(tmpPt.X, tmpPt.Y) * tmpArea).GetAsVector();
            }
            for (int i = 1; i < last; i++)
            {
                Point2d p1 = vertices[i].Position.Convert2d();
                Point2d p2 = vertices[i + 1].Position.Convert2d();
                tri = new Triangle2d(p0, p1, p2);
                tmpArea = tri.SignedArea;
                area += tmpArea;
                cen += (tri.Centroid * tmpArea).GetAsVector();
                bulge = vertices[i].Bulge;
                if (bulge != 0.0)
                {
                    arc = pline.GetArcSegment2dAt(i);
                    tmpArea = arc.SignedArea();
                    tmpPt = arc.Centroid();
                    area += tmpArea;
                    cen += (new Point2d(tmpPt.X, tmpPt.Y) * tmpArea).GetAsVector();
                }
            }
            bulge = vertices[last].Bulge;
            if ((bulge != 0.0) && (pline.Closed == true))
            {
                arc = pline.GetArcSegment2dAt(last);
                tmpArea = arc.SignedArea();
                tmpPt = arc.Centroid();
                area += tmpArea;
                cen += (new Point2d(tmpPt.X, tmpPt.Y) * tmpArea).GetAsVector();
            }
            cen = cen.DivideBy(area);
            return new Point3d(cen.X, cen.Y, pline.Elevation).TransformBy(Matrix3d.PlaneToWorld(pline.Normal));
        }

        /// <summary>
        /// Creates a new Polyline which is the result of the projection of the Polyline2d on the specified plane in the specified direction. 
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <param name="plane">The projection plane.</param>
        /// <param name="direction">The projection direction (WCS coordinates).</param>
        /// <returns>The projected Polyline.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="pline"/> is null.</exception>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="plane"/> is null.</exception>
        public static Polyline? GetProjectedPolyline(this Polyline2d pline, Plane plane, Vector3d direction)
        {
            ArgumentNullException.ThrowIfNull(pline);
            ArgumentNullException.ThrowIfNull(pline);
            Tolerance tol = new(1e-9, 1e-9);
            if (plane.Normal.IsPerpendicularTo(direction, tol))
                return null;

            if (pline.Normal.IsPerpendicularTo(direction, tol))
            {
                Plane dirPlane = new(Point3d.Origin, direction);
                if (!pline.IsWriteEnabled) pline.UpgradeOpen();
                pline.TransformBy(Matrix3d.WorldToPlane(dirPlane));
                Extents3d extents = pline.GeometricExtents;
                pline.TransformBy(Matrix3d.PlaneToWorld(dirPlane));
                return GeometryExtension.ProjectExtents(extents, plane, direction, dirPlane);
            }

            return GeometryExtension.ProjectPolyline(pline, plane, direction);
        }

        /// <summary>
        /// Creates a new Polyline which is the result of the orthogonal projection of the Polyline2d on the specified plane.
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <param name="plane">The projection plane.</param>
        /// <returns>The projected Polyline.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="pline"/> is null.</exception>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="plane"/> is null.</exception>
        public static Polyline? GetOrthoProjectedPolyline(this Polyline2d pline, Plane plane) =>
            pline.GetProjectedPolyline(plane, plane.Normal);
    }
}
