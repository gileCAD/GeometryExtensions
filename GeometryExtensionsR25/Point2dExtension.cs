using System;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Gile.AutoCAD.R25.Geometry
{
    /// <summary>
    /// Provides extension methods for the Point2d type.
    /// </summary>
    public static class Point2dExtension
    {
        /// <summary>
        /// Converts a Point2d into a Point3d with a Z coordinate equal to 0.
        /// </summary>
        /// <param name="pt">The instance to which this method applies.</param>
        /// <returns>The corresponding Point3d.</returns>
        public static Point3d Convert3d(this Point2d pt) =>
            new(pt.X, pt.Y, 0.0);

        /// <summary>
        /// Converts a Point2d into a Point3d according to the specified plane.
        /// </summary>
        /// <param name="pt">The instance to which this method applies.</param>
        /// <param name="plane">The plane the Point2d lies on.</param>
        /// <returns>The corresponding Point3d.</returns>
        public static Point3d Convert3d(this Point2d pt, Plane plane) =>
            pt.Convert3d().TransformBy(Matrix3d.PlaneToWorld(plane));

        /// <summary>
        /// Converts a Point2d into a Point3d according to the plane defined by its Normal an Elevation.
        /// </summary>
        /// <param name="pt">The instance to which this method applies.</param>
        /// <param name="normal">The Normal of the plane the Point2d lies on.</param>
        /// <param name="elevation">The Elevation of the plane the Point2d lies on.</param>
        /// <returns>The corresponding Point3d.</returns>
        public static Point3d Convert3d(this Point2d pt, Vector3d normal, double elevation) =>
            new Point3d(pt.X, pt.Y, elevation).TransformBy(Matrix3d.PlaneToWorld(normal));

        /// <summary>
        /// Projects the point on the XY plane of WCS.
        /// </summary>
        /// <param name="pt">The point to be projected.</param>
        /// <param name="normal">The Normal of the plane the point lies on.</param>
        /// <returns>The projected Point2d.</returns>
        public static Point2d Flatten(this Point2d pt, Vector3d normal) =>
            new Point3d(pt.X, pt.Y, 0.0)
                .TransformBy(Matrix3d.PlaneToWorld(normal))
                .Convert2d(new Plane());

        /// <summary>
        /// Gets a value indicating if <c>pt</c> lies on the segment <c>p1</c> <c>p2</c> using Tolerance.Global.
        /// </summary>
        /// <param name="pt">The instance to which this method applies.</param>
        /// <param name="p1">The start point of the segment.</param>
        /// <param name="p2">The end point of the segment.</param>
        /// <returns>true, if the point lies on the segment ; false, otherwise.</returns>
        public static bool IsBetween(this Point2d pt, Point2d p1, Point2d p2) =>
            p1.GetVectorTo(pt).GetNormal().Equals(pt.GetVectorTo(p2).GetNormal());

        /// <summary>
        /// Gets a value indicating if <c>pt</c> lies on the segment <c>p1</c> <c>p2</c> using the specified Tolerance.
        /// </summary>
        /// <param name="pt">The instance to which this method applies.</param>
        /// <param name="p1">The start point of the segment.</param>
        /// <param name="p2">The end point of the segment.</param>
        /// <param name="tol">The tolerance used for comparisons.</param>
        /// <returns>true, if the point lies on the segment ; false, otherwise.</returns>
        public static bool IsBetween(this Point2d pt, Point2d p1, Point2d p2, Tolerance tol) =>
            p1.GetVectorTo(pt).GetNormal(tol).Equals(pt.GetVectorTo(p2).GetNormal(tol));

        /// <summary>
        /// Get a value indicating if the specified point is inside the extents.
        /// </summary>
        /// <param name="pt">The instance to which this method applies.</param>
        /// <param name="extents">The extents 2d to test against.</param>
        /// <returns>true, if the point us inside the extents ; false, otherwise.</returns>
        public static bool IsInside(this Point2d pt, Extents2d extents) =>
                pt.X >= extents.MinPoint.X &&
                pt.Y >= extents.MinPoint.Y &&
                pt.X <= extents.MaxPoint.X &&
                pt.Y <= extents.MaxPoint.Y;

        /// <summary>
        /// Defines a point with polar coordiantes relative to a base point.
        /// </summary>
        /// <param name="org">The instance to which this method applies.</param>
        /// <param name="angle">The angle in radians from the X axis.</param>
        /// <param name="distance">The distance from the base point.</param>
        /// <returns>The new point2d.</returns>
        public static Point2d Polar(this Point2d org, double angle, double distance) =>
            new(
                org.X + distance * Math.Cos(angle),
                org.Y + distance * Math.Sin(angle));
    }
}
