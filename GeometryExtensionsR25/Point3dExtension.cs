using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using AcRx = Autodesk.AutoCAD.Runtime;

namespace Gile.AutoCAD.R25.Geometry
{
    /// <summary>
    /// Provides extension methods for the Point2d type.
    /// </summary>
    public static class Point3dExtension
    {
        /// <summary>
        /// Converts a Point3d into a Point2d (projection on XY plane).
        /// </summary>
        /// <param name="pt">The instance to which this method applies.</param>
        /// <returns>The corresponding Point3d.</returns>
        public static Point2d Convert2d(this Point3d pt)
        {
            return new Point2d(pt.X, pt.Y);
        }

        /// <summary>
        /// Projects the point on the WCS XY plane.
        /// </summary>
        /// <param name="pt">The point to be projected.</param>
        /// <returns>The projected point.</returns>
        public static Point3d Flatten(this Point3d pt)
        {
            return new Point3d(pt.X, pt.Y, 0.0);
        }

        /// <summary>
        /// Gets a value indicating if <c>pt</c> lies on the segment <c>p1</c> <c>p2</c> using Tolerance.Global.
        /// </summary>
        /// <param name="pt">The instance to which this method applies.</param>
        /// <param name="p1">The start point of the segment.</param>
        /// <param name="p2">The end point of the segment.</param>
        /// <returns>true, if the point lies on the segment ; false, otherwise.</returns>
        public static bool IsBetween(this Point3d pt, Point3d p1, Point3d p2)
        {
            return p1.GetVectorTo(pt).GetNormal().Equals(pt.GetVectorTo(p2).GetNormal());
        }

        /// <summary>
        /// Gets a value indicating if <c>pt</c> lies on the segment <c>p1</c> <c>p2</c> using the specified Tolerance.
        /// </summary>
        /// <param name="pt">The instance to which this method applies.</param>
        /// <param name="p1">The start point of the segment.</param>
        /// <param name="p2">The end point of the segment.</param>
        /// <param name="tol">The tolerance used for comparisons.</param>
        /// <returns>true, if the point lies on the segment ; false, otherwise.</returns>
        public static bool IsBetween(this Point3d pt, Point3d p1, Point3d p2, Tolerance tol)
        {
            return p1.GetVectorTo(pt).GetNormal(tol).Equals(pt.GetVectorTo(p2).GetNormal(tol));
        }

        /// <summary>
        /// Get a value indicating if the specified point is inside the extents.
        /// </summary>
        /// <param name="pt">The instance to which this method applies.</param>
        /// <param name="extents">The extents 2d to test against.</param>
        /// <returns>true, if the point us inside the extents ; false, otherwise.</returns>
        public static bool IsInside(this Point3d pt, Extents3d extents)
        {
            return
                pt.X >= extents.MinPoint.X &&
                pt.Y >= extents.MinPoint.Y &&
                pt.Z >= extents.MinPoint.Z &&
                pt.X <= extents.MaxPoint.X &&
                pt.Y <= extents.MaxPoint.Y &&
                pt.Z <= extents.MaxPoint.Z;
        }

        /// <summary>
        /// Defines a point with polar coordiantes relative to a base point.
        /// </summary>
        /// <param name="org">The instance to which this method applies.</param>
        /// <param name="angle">The angle in radians from the X axis.</param>
        /// <param name="distance">The distance from the base point.</param>
        /// <returns>The new point3d.</returns>
        public static Point3d Polar(this Point3d org, double angle, double distance)
        {
            return new Point3d(
                org.X + distance * Math.Cos(angle),
                org.Y + distance * Math.Sin(angle),
                org.Z);
        }

        /// <summary>
        /// Converts a point from a coordinate system to another one.
        /// </summary>
        /// <param name="pt">The instance to which this method applies.</param>
        /// <param name="from">The origin coordinate system flag.</param>
        /// <param name="to">The destination coordinate system flag.</param>
        /// <returns>The corresponding Point3d.</returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        /// eInvalidInput thrown of 3 (CoordSystem.PSDCS) is used with another flag than 2 (CoordSystem.DCS).</exception>
        public static Point3d Trans(this Point3d pt, int from, int to)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            return pt.Trans(ed, (CoordSystem)from, (CoordSystem)to);
        }

        /// <summary>
        /// Converts a point from a coordinate system to another one.
        /// </summary>
        /// <param name="pt">The instance to which this method applies.</param>
        /// <param name="ed">Current instance of Editor.</param>
        /// <param name="from">The origin coordinate system flag.</param>
        /// <param name="to">The destination coordinate system flag.</param>
        /// <returns>The corresponding Point3d.</returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        /// eInvalidInput thrown of 3 (CoordSystem.PSDCS) is used with another flag than 2 (CoordSystem.DCS).</exception>
        public static Point3d Trans(this Point3d pt, Editor ed, int from, int to)
        {
            return pt.Trans(ed, (CoordSystem)from, (CoordSystem)to);
        }

        /// <summary>
        /// Converts a point from a coordinate system to another one.
        /// </summary>
        /// <param name="pt">The instance to which this method applies.</param>
        /// <param name="from">The origin coordinate system.</param>
        /// <param name="to">The destination coordinate system.</param>
        /// <returns>The corresponding Point3d.</returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        /// eInvalidInput thrown of 3 (CoordSystem.PSDCS) is used with another flag than 2 (CoordSystem.DCS).</exception>
        public static Point3d Trans(this Point3d pt, CoordSystem from, CoordSystem to)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            return pt.Trans(ed, from, to);
        }

        /// <summary>
        /// Converts a point from a coordinate system to another one.
        /// </summary>
        /// <param name="pt">The instance to which this method applies.</param>
        /// <param name="ed">Current instance of Editor.</param>
        /// <param name="from">The origin coordinate system.</param>
        /// <param name="to">The destination coordinate system.</param>
        /// <returns>The corresponding Point3d.</returns>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        /// eInvalidInput thrown of 3 (CoordSystem.PSDCS) is used with another flag than 2 (CoordSystem.DCS).</exception>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">
        /// eInvalidInput est lancée si CoordSystem.PSDCS est utilisé avec un autre drapeau que CoordSystem.DCS.</exception>
        public static Point3d Trans(this Point3d pt, Editor ed, CoordSystem from, CoordSystem to)
        {
            Matrix3d mat = new();
            switch (from)
            {
                case CoordSystem.WCS:
                    switch (to)
                    {
                        case CoordSystem.UCS:
                            mat = ed.WCS2UCS();
                            break;
                        case CoordSystem.DCS:
                            mat = ed.WCS2DCS();
                            break;
                        case CoordSystem.PSDCS:
                            throw new AcRx.Exception(
                                AcRx.ErrorStatus.InvalidInput,
                                "To be used only with DCS");
                        default:
                            mat = Matrix3d.Identity;
                            break;
                    }
                    break;
                case CoordSystem.UCS:
                    switch (to)
                    {
                        case CoordSystem.WCS:
                            mat = ed.UCS2WCS();
                            break;
                        case CoordSystem.DCS:
                            mat = ed.UCS2WCS() * ed.WCS2DCS();
                            break;
                        case CoordSystem.PSDCS:
                            throw new AcRx.Exception(
                                AcRx.ErrorStatus.InvalidInput,
                                "To be used only with DCS");
                        default:
                            mat = Matrix3d.Identity;
                            break;
                    }
                    break;
                case CoordSystem.DCS:
                    switch (to)
                    {
                        case CoordSystem.WCS:
                            mat = ed.DCS2WCS();
                            break;
                        case CoordSystem.UCS:
                            mat = ed.DCS2WCS() * ed.WCS2UCS();
                            break;
                        case CoordSystem.PSDCS:
                            mat = ed.DCS2PSDCS();
                            break;
                        default:
                            mat = Matrix3d.Identity;
                            break;
                    }
                    break;
                case CoordSystem.PSDCS:
                    switch (to)
                    {
                        case CoordSystem.WCS:
                            throw new AcRx.Exception(
                                AcRx.ErrorStatus.InvalidInput,
                                "To be used only with DCS");
                        case CoordSystem.UCS:
                            throw new AcRx.Exception(
                                AcRx.ErrorStatus.InvalidInput,
                                "To be used only with DCS");
                        case CoordSystem.DCS:
                            mat = ed.PSDCS2DCS();
                            break;
                        default:
                            mat = Matrix3d.Identity;
                            break;
                    }
                    break;
            }
            return pt.TransformBy(mat);
        }
    }
}
