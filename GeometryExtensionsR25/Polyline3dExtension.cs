using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Gile.AutoCAD.R25.Geometry
{
    /// <summary>
    /// Provides extension methods for the Polyline3d type.
    /// </summary>
    public static class Polyline3dExtension
    {
        /// <summary>
        /// Creates a new Polyline which is the result of the projection of the Polyline3d on the specified plane in the specified direction. 
        /// </summary>
        /// <param name="pline3d">The instance to which this method applies.</param>
        /// <param name="plane">The projection plane.</param>
        /// <param name="direction">The projection direction (WCS coordinates).</param>
        /// <returns>The projected Polyline.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="pline3d"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="plane"/> is null.</exception>
        public static Polyline? GetProjectedPolyline(this Polyline3d pline3d, Plane plane, Vector3d direction)
        {
            System.ArgumentNullException.ThrowIfNull(pline3d);
            System.ArgumentNullException.ThrowIfNull(plane);
            if (plane.Normal.IsPerpendicularTo(direction, new Tolerance(1e-9, 1e-9)))
                return null;

            var pline = new Polyline();
            int i = 0;
            using (var tr = new OpenCloseTransaction())
            {
                foreach (ObjectId id in pline3d)
                {
                    var vertex = (PolylineVertex3d)tr.GetObject(id, OpenMode.ForRead);
                    {
                        if (vertex.VertexType != Vertex3dType.ControlVertex)
                        {
                            var point2d = vertex.Position.Project(plane, direction).Convert2d(plane);
                            pline.AddVertexAt(i++, point2d, 0.0, 0.0, 0.0);
                        }
                    }
                }
            }
            pline.Closed = pline3d.Closed;
            pline.Normal = plane.Normal;
            pline.Elevation = plane.PointOnPlane.TransformBy(Matrix3d.WorldToPlane(new Plane(Point3d.Origin, plane.Normal))).Z;
            return pline;
        }

        /// <summary>
        /// Creates a new Polyline which is the result of the orthogonal projection of the Polyline3d on the specified plane.
        /// </summary>
        /// <param name="pline3d">The instance to which this method applies.</param>
        /// <param name="plane">The projection plane.</param>
        /// <returns>The projected Polyline.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="pline3d"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="plane"/> is null.</exception>
        public static Polyline GetOrthoProjectedPolyline(this Polyline3d pline3d, Plane plane)
        {
            System.ArgumentNullException.ThrowIfNull(pline3d);
            System.ArgumentNullException.ThrowIfNull(pline3d);
            var pline = new Polyline();
            int i = 0;
            using (var tr = new OpenCloseTransaction())
            {
                foreach (ObjectId id in pline3d)
                {
                    var vertex = (PolylineVertex3d)tr.GetObject(id, OpenMode.ForRead);
                    {
                        if (vertex.VertexType != Vertex3dType.ControlVertex)
                        {
                            var point2d = vertex.Position.Convert2d(plane);
                            pline.AddVertexAt(i++, point2d, 0.0, 0.0, 0.0);
                        }
                    }
                }
            }
            pline.Closed = pline3d.Closed;
            pline.Normal = plane.Normal;
            pline.Elevation = plane.PointOnPlane.TransformBy(Matrix3d.WorldToPlane(new Plane(Point3d.Origin, plane.Normal))).Z;
            return pline;
        }
    }
}
