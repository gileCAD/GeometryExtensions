using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Gile.AutoCAD.R25.Geometry
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
        /// <exception cref="System.ArgumentNullException">ArgumentNullException is thrown if <paramref name="curve"/> is null.</exception>
        public static bool IsPointOnCurve(this Curve curve, Point3d point, Tolerance tolerance)
        {
            System.ArgumentNullException.ThrowIfNull(curve);
            return point.IsEqualTo(curve.GetClosestPointTo(point, false), tolerance);
        }

        /// <summary>
        /// Calls curve.IsPointOnCurve(Point3d point, Tolerance tolerance) with tolerance set to Global.
        /// </summary>
        /// <param name="curve">The instance of Curve to which this method applies.</param>
        /// <param name="point">Point to check against.</param>
        /// <returns>true, if the condition is met; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentNullException is thrown if <paramref name="curve"/> is null.</exception>
        public static bool IsPointOnCurve(this Curve curve, Point3d point)
        {
            System.ArgumentNullException.ThrowIfNull(curve);
            return curve.IsPointOnCurve(point, Tolerance.Global);
        }
    }
}
