using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using System;
using System.Collections.Generic;

using AcRx = Autodesk.AutoCAD.Runtime;

namespace Gile.AutoCAD.Geometry
{
    /// <summary>
    /// Provides extension methods for the Polyline2d type.
    /// </summary>
    public static class Polyline2dExtension
    {
        /// <summary>
        /// Gets the list of verices.
        /// </summary>
        /// <param name="pl">The instance to which this method applies.</param>
        /// <returns>The list of vertices.</returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        /// eNoActiveTransactions is thrown if the lethod is called outside of a Transaction.</exception>
        public static List<Vertex2d> GetVertices(this Polyline2d pl)
        {
            Transaction tr = pl.Database.TransactionManager.TopTransaction;
            if (tr == null)
                throw new AcRx.Exception(AcRx.ErrorStatus.NoActiveTransactions);

            List<Vertex2d> vertices = new List<Vertex2d>();
            foreach (ObjectId id in pl)
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
        /// <param name="pl">The instance to which this method applies.</param>
        /// <param name="index">The segment index.</param>
        /// <returns>An instance of LineSegment3d (WCS coordinates).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException if the index id out of the indices range.</exception>
        public static LineSegment3d GetLineSegmentAt(this Polyline2d pl, int index)
        {
            try
            {
                return new LineSegment3d(
                    pl.GetPointAtParameter(index),
                    pl.GetPointAtParameter(index + 1));
            }
            catch
            {
                throw new ArgumentOutOfRangeException("Out of range index");
            }
        }

        /// <summary>
        /// Gets the linear 2D segment at specified index.
        /// </summary>
        /// <param name="pl">The instance to which this method applies.</param>
        /// <param name="index">The segment index.</param>
        /// <returns>An instance of LineSegment2d (OCS coordinates).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException if the index id out of the indices range.</exception>
        public static LineSegment2d GetLineSegment2dAt(this Polyline2d pl, int index)
        {
            try
            {
                Matrix3d WCS2ECS = pl.Ecs.Inverse();
                return new LineSegment2d(
                    pl.GetPointAtParameter(index).TransformBy(WCS2ECS).Convert2d(),
                    pl.GetPointAtParameter(index + 1.0).TransformBy(WCS2ECS).Convert2d());
            }
            catch
            {
                throw new ArgumentOutOfRangeException("Out of range index");
            }
        }

        /// <summary>
        /// Gets the circular arc 3D segment at specified index.
        /// </summary>
        /// <param name="pl">The instance to which this method applies.</param>
        /// <param name="index">The segment index.</param>
        /// <returns>An instance of CircularArc3d (WCS coordinates).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException if the index id out of the indices range.</exception>
        public static CircularArc3d GetArcSegmentAt(this Polyline2d pl, int index)
        {
            try
            {
                return new CircularArc3d(
                    pl.GetPointAtParameter(index),
                    pl.GetPointAtParameter(index + 0.5),
                    pl.GetPointAtParameter(index + 1));
            }
            catch
            {
                throw new ArgumentOutOfRangeException("index");
            }
        }

        /// <summary>
        /// Gets the circular arc 2D segment at specified index.
        /// </summary>
        /// <param name="pl">The instance to which this method applies.</param>
        /// <param name="index">The segment index.</param>
        /// <returns>An instance of CircularArc2d (OCS coordinates).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// ArgumentOutOfRangeException if the index id out of the indices range.</exception>
        public static CircularArc2d GetArcSegment2dAt(this Polyline2d pl, int index)
        {
            try
            {
                Matrix3d WCS2ECS = pl.Ecs.Inverse();
                return new CircularArc2d(
                    pl.GetPointAtParameter(index).TransformBy(WCS2ECS).Convert2d(),
                    pl.GetPointAtParameter(index + 0.5).TransformBy(WCS2ECS).Convert2d(),
                    pl.GetPointAtParameter(index + 1.0).TransformBy(WCS2ECS).Convert2d());
            }
            catch
            {
                throw new ArgumentOutOfRangeException("index");
            }
        }

        /// <summary>
        /// Gets the centroid.
        /// </summary>
        /// <param name="pl">The instance to which this method applies.</param>
        /// <returns>The centroid (WCS coordinates).</returns>
        public static Point3d Centroid(this Polyline2d pl)
        {
            Vertex2d[] vertices = pl.GetVertices().ToArray();
            int last = vertices.Length - 1;
            Vertex2d vertex = vertices[0];
            Point2d p0 = vertex.Position.Convert2d();
            double elev = pl.Elevation;
            Point2d cen = new Point2d(0.0, 0.0);
            double area = 0.0;
            double bulge = vertex.Bulge;
            double tmpArea;
            Point2d tmpPt;
            Triangle2d tri = new Triangle2d();
            CircularArc2d arc = new CircularArc2d();
            if (bulge != 0.0)
            {
                arc = pl.GetArcSegment2dAt(0);
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
                    arc = pl.GetArcSegment2dAt(i);
                    tmpArea = arc.SignedArea();
                    tmpPt = arc.Centroid();
                    area += tmpArea;
                    cen += (new Point2d(tmpPt.X, tmpPt.Y) * tmpArea).GetAsVector();
                }
            }
            bulge = vertices[last].Bulge;
            if ((bulge != 0.0) && (pl.Closed == true))
            {
                arc = pl.GetArcSegment2dAt(last);
                tmpArea = arc.SignedArea();
                tmpPt = arc.Centroid();
                area += tmpArea;
                cen += (new Point2d(tmpPt.X, tmpPt.Y) * tmpArea).GetAsVector();
            }
            cen = cen.DivideBy(area);
            return new Point3d(cen.X, cen.Y, pl.Elevation).TransformBy(Matrix3d.PlaneToWorld(pl.Normal));
        }

        /// <summary>
        /// Creates a new Polyline which is the result of the projection of the Polyline2d on the specified plane in the specified direction. 
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <param name="plane">The projection plane.</param>
        /// <param name="direction">The projection direction (WCS coordinates).</param>
        /// <returns>The projected Polyline.</returns>
        public static Polyline GetProjectedPolyline(this Polyline2d pline, Plane plane, Vector3d direction)
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
        /// Creates a new Polyline which is the result of the orthogonal projection of the Polyline2d on the specified plane.
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <param name="plane">The projection plane.</param>
        /// <returns>The projected Polyline.</returns>
        public static Polyline GetOrthoProjectedPolyline(this Polyline2d pline, Plane plane) =>
            pline.GetProjectedPolyline(plane, plane.Normal);
    }
}
