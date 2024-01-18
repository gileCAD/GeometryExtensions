using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

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
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="pline"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="plane"/> is null.</exception>
        public static Polyline GetProjectedPolyline(this Polyline3d pline, Plane plane, Vector3d direction)
        {
            Assert.IsNotNull(pline, nameof(pline));
            Assert.IsNotNull(pline, nameof(plane));
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
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="pline"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="plane"/> is null.</exception>
        public static Polyline GetOrthoProjectedPolyline(this Polyline3d pline, Plane plane)
        {
            Assert.IsNotNull(pline, nameof(pline));
            Assert.IsNotNull(pline, nameof(plane));
            return pline.GetProjectedPolyline(plane, plane.Normal);
        }
    }
}
