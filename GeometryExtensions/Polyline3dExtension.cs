using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;

using AcRx = Autodesk.AutoCAD.Runtime;

namespace Gile.AutoCAD.Geometry
{
    /// <summary>
    /// Provides extension methods for the Polyline3d type.
    /// </summary>
    public static class Polyline3dExtension
    {
        /// <summary>
        /// Creates a new Polyline which is the result of the projection of the Polyline3d on the specified plane in the specified direction. 
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <param name="plane">The projection plane.</param>
        /// <param name="direction">The projection direction (WCS coordinates).</param>
        /// <returns>The projected Polyline.</returns>
        public static Polyline GetProjectedPolyline(this Polyline3d pline, Plane plane, Vector3d direction)
        {
            if (plane.Normal.IsPerpendicularTo(direction, new Tolerance(1e-9, 1e-9)))
                return null;

            return GeometryExtension.ProjectPolyline(pline, plane, direction);
        }

        /// <summary>
        /// Creates a new Polyline which is the result of the orthogonal projection of the Polyline3d on the specified plane.
        /// </summary>
        /// <param name="pline">The instance to which this method applies.</param>
        /// <param name="plane">The projection plane.</param>
        /// <returns>The projected Polyline.</returns>
        public static Polyline GetOrthoProjectedPolyline(this Polyline3d pline, Plane plane) =>
            pline.GetProjectedPolyline(plane, plane.Normal);

        /// <summary>
        /// Gets all the Vertex3d elements for a given polyline3d
        /// </summary>
        /// <param name="pline"></param>
        /// <returns></returns>
        public static List<PolylineVertex3d> GetVertices(this Polyline3d pline)
        {
            Transaction tr = pline.Database.TransactionManager.TopTransaction;
            if (tr == null)
                throw new AcRx.Exception(AcRx.ErrorStatus.NoActiveTransactions);

            List<PolylineVertex3d> vertices = new List<PolylineVertex3d>();
            foreach (ObjectId id in pline)
            {
                PolylineVertex3d vx = (PolylineVertex3d)tr.GetObject(id, OpenMode.ForRead);
                vertices.Add(vx);
            }
            return vertices;
        }
    }
}
