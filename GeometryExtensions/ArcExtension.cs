using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gile.AutoCAD.Geometry
{
    public static class ArcExtension
    {
        /// <summary>
        /// Converts an Arc into a CircularArc2d given a plane.
        /// </summary>
        /// <param name="arc"></param>
        /// <param name="plane"></param>
        /// <returns></returns>
        public static CircularArc2d ToCircularArc2d(this Arc arc, Plane plane = null)
        {
            if (plane == null)
            {
                plane = new Plane(new Point3d(), Vector3d.ZAxis);
            }            

            Point2d startPoint = arc.StartPoint.Convert2d(plane);

            Point2d intermediatePoint  = arc.GetPointAtDist(arc.Length / 2.0)
                                            .Convert2d(plane);            

            Point2d endPoint = arc.EndPoint.Convert2d(plane);

            CircularArc2d circularArc2D = new CircularArc2d(startPoint, intermediatePoint, endPoint);
            return circularArc2D;
        }
    }
}
