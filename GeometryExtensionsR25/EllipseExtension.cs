using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Gile.AutoCAD.R25.Geometry
{
    /// <summary>
    /// Provides extension methods for the Ellipse type. 
    /// </summary>
    public static class EllipseExtension
    {
        /// <summary>
        /// Generates a polyline to approximate an ellipse. 
        /// </summary>
        /// <param name="ellipse">The instance to which this method applies.</param>
        /// <returns>A new Polyline instance.</returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException is thrown if <paramref name="ellipse"/> is null.</exception>
        public static Polyline ToPolyline(this Ellipse ellipse)
        {
            ArgumentNullException.ThrowIfNull(ellipse);
            Polyline pline = new PolylineSegmentCollection(ellipse).ToPolyline();
            pline.Closed = ellipse.Closed;
            pline.Normal = ellipse.Normal;
            pline.Elevation = ellipse.Center.TransformBy(Matrix3d.WorldToPlane(new Plane(Point3d.Origin, ellipse.Normal))).Z;
            return pline;
        }

        /// <summary>
        /// Gets the ellipse parameter corresponding to the specified angle.
        /// </summary>
        /// <param name="ellipse">The instance to which this method applies.</param>
        /// <param name="angle">Angle.</param>
        /// <returns>The parameter corresponding to the angle.</returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException is thrown if <paramref name="ellipse"/> is null.</exception>
        public static double GetParamAtAngle(this Ellipse ellipse, double angle)
        {
            ArgumentNullException.ThrowIfNull(ellipse);
            return Math.Atan2(ellipse.MajorRadius * Math.Sin(angle), ellipse.MinorRadius * Math.Cos(angle));
        }

        /// <summary>
        /// Gets the ellipse angle corresponding to the specified parameter.
        /// </summary>
        /// <param name="ellipse">The instance to which this method applies.</param>
        /// <param name="param">Parameter.</param>
        /// <returns>The angle corresponding to the parameter.</returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException is thrown if <paramref name="ellipse"/> is null.</exception>
        public static double GetAngleAtParam(this Ellipse ellipse, double param)
        {
            ArgumentNullException.ThrowIfNull(ellipse);
            return Math.Atan2(ellipse.MinorRadius * Math.Sin(param), ellipse.MajorRadius * Math.Cos(param));
        }
    }
}
