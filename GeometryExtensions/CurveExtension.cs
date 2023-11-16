using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace Gile.AutoCAD.Geometry
{
    /// <summary>
    /// Provides extension methods for the Curve type.
    /// </summary>
    public static class CurveExtension
    {
        /// <summary>
        /// Checks if <paramref name="point"/> is within the distance Tolerance.EqualPoint from this curve.
        /// </summary>
        /// <param name="curve">The instance of Curve to which this method applies.</param>
        /// <param name="point">Point to check against.</param>
        /// <param name="tolerance">Tolerance value.</param>
        /// <returns>true, if the condition is met; otherwise, false.</returns>
        public static bool IsPointOnCurve(this Curve curve, Point3d point, Tolerance tolerance) =>
            point.IsEqualTo(curve.GetClosestPointTo(point, false), tolerance);

        /// <summary>
        /// Calls curve.IsPointOnCurve(Point3d point, Tolerance tolerance) with tolerance set to Global.
        /// </summary>
        /// <param name="curve">The instance of Curve to which this method applies.</param>
        /// <param name="point">Point to check against.</param>
        /// <returns>true, if the condition is met; otherwise, false.</returns>
        
        public static bool IsPointOnCurve(this Curve curve, Point3d point) =>
            curve.IsPointOnCurve(point, Tolerance.Global);
        
        public static bool IsArc(this Curve curve) =>
            curve.GetRXClass() == RXObject.GetClass(typeof(Arc));
        public static bool IsCircle(this Curve curve) =>
            curve.GetRXClass() == RXObject.GetClass(typeof(Circle));
        public static bool IsEllipse(this Curve curve) =>
            curve.GetRXClass() == RXObject.GetClass(typeof(Ellipse));
        
        public static bool IsLeader(this Curve curve) =>
            curve.GetRXClass() == RXObject.GetClass(typeof(Leader));
        public static bool IsLine(this Curve curve) =>
            curve.GetRXClass() == RXObject.GetClass(typeof(Line));

        public static bool IsPolyline(this Curve curve) =>
            curve.GetRXClass() == RXObject.GetClass(typeof(Polyline));
        public static bool IsPolyline2d(this Curve curve) =>
            curve.GetRXClass() == RXObject.GetClass(typeof(Polyline2d));
        public static bool IsPolyline3d(this Curve curve) =>
            curve.GetRXClass() == RXObject.GetClass(typeof(Polyline3d));
        public static bool IsRay(this Curve curve) =>
            curve.GetRXClass() == RXObject.GetClass(typeof(Ray));
        public static bool IsSpline(this Curve curve) =>
            curve.GetRXClass() == RXObject.GetClass(typeof(Spline));
        public static bool IsXline(this Curve curve) =>
            curve.GetRXClass() == RXObject.GetClass(typeof(Xline));

    }
}
