using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Gile.AutoCAD.R25.Geometry
{
    /// <summary>
    /// Provides extension methods for Point3dCollection and IEnumerable&lt;Point3d&gt; types.
    /// </summary>
    public static class Point3dCollectionExtension
    {
        /// <summary>
        /// Removes duplicated points in the collection using Tolerance.Global.EqualPoint.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <returns>A sequence of distinct points.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="source"/> is null.</exception>
        public static IEnumerable<Point3d> RemoveDuplicates(this Point3dCollection source)
        {
            ArgumentNullException.ThrowIfNull(source);
            return source.RemoveDuplicates(Tolerance.Global);
        }

        /// <summary>
        /// Removes duplicated points in the collection using the specified Tolerance.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <param name="tolerance">The tolerance to be used in equality comparison.</param>
        /// <returns>A sequence of distinct points.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="source"/> is null.</exception>
        public static IEnumerable<Point3d> RemoveDuplicates(this Point3dCollection source, Tolerance tolerance)
        {
            ArgumentNullException.ThrowIfNull(source);
            return source.Cast<Point3d>().Distinct(new Point3dComparer(tolerance));
        }

        /// <summary>
        /// Removes duplicated points in the sequence using Tolerance.Global.EqualPoint.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <returns>A sequence of distinct points.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="source"/> is null.</exception>
        public static IEnumerable<Point3d> RemoveDuplicates(this IEnumerable<Point3d> source) =>
            source.RemoveDuplicates(Tolerance.Global);

        /// <summary>
        /// Removes duplicated points in the sequence using the specified Tolerance.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <param name="tolerance">The tolerance to be used in equality comparison.</param>
        /// <returns>A sequence of distinct points.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="source"/> is null.</exception>
        public static IEnumerable<Point3d> RemoveDuplicates(this IEnumerable<Point3d> source, Tolerance tolerance)
        {
            ArgumentNullException.ThrowIfNull(source);

            return source.Distinct(new Point3dComparer(tolerance));
        }

        /// <summary>
        /// Gets a value indicating if the the collection contains the point using Tolerance.Global.EqualPoint.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <param name="pt">The point to search.</param>
        /// <returns>true, if the point is found ; false, otherwise.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="source"/> is null.</exception>
        public static bool Contains(this Point3dCollection source, Point3d pt) =>
            source.Contains(pt, Tolerance.Global);

        /// <summary>
        /// Gets a value indicating if the the collection contains the point using the specified Tolerance.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <param name="pt">The point to search.</param>
        /// <param name="tol">The Tolerance to be use in comparisons.</param>
        /// <returns>true, if the point is found ; false, otherwise.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="source"/> is null.</exception>
        public static bool Contains(this Point3dCollection source, Point3d pt, Tolerance tol)
        {
            ArgumentNullException.ThrowIfNull(source);

            for (int i = 0; i < source.Count; i++)
            {
                if (pt.IsEqualTo(source[i], tol))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the extents of the collection of points.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <returns>An Extents3d instance.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="source"/> is null.</exception>
        /// <exception cref="ArgumentException">ArgumentException is thrown if <paramref name="source"/> is empty.</exception>
        public static Extents3d ToExtents3d(this Point3dCollection source)
        {
            ArgumentNullException.ThrowIfNull(source);
            Assert.IsNotEmpty(source, nameof(source));
            return source.Cast<Point3d>().ToExtents3d();
        }

        /// <summary>
        /// Gets the extents of the sequence of points.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <returns>An Extents3d instance.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="source"/> is null.</exception>
        /// <exception cref="ArgumentException">ArgumentException is thrown if <paramref name="source"/> is empty.</exception>
        public static Extents3d ToExtents3d(this IEnumerable<Point3d> source)
        {
            ArgumentNullException.ThrowIfNull(source);
            Assert.IsNotEmpty(source, nameof(source));
            return source.Aggregate(new Extents3d(), (e, p) => { e.AddPoint(p); return e; });
        }

        /// <summary>
        /// Provides equality comparison methods for the Point3d type.
        /// </summary>
        /// <remarks>
        /// Creates a new instance ot Point3dComparer
        /// </remarks>
        /// <param name="tolerance">The Tolerance to be used in equality comparison.</param>
        class Point3dComparer(Tolerance tolerance) : IEqualityComparer<Point3d>
        {
            private Tolerance tolerance = tolerance;
            private readonly double prec = tolerance.EqualPoint * 10.0;

            /// <summary>
            /// Evaluates if two points are equal.
            /// </summary>
            /// <param name="a">First point.</param>
            /// <param name="b">Second point.</param>
            /// <returns>true, if the two points are equal; false, otherwise.</returns>
            public bool Equals(Point3d a, Point3d b) => a.IsEqualTo(b, tolerance);

            /// <summary>
            /// Serves as hashing function for the Point2d type.
            /// </summary>
            /// <param name="pt">Point.</param>
            /// <returns>The hash code.</returns>
            public int GetHashCode(Point3d pt) =>
                new Point3d(Round(pt.X), Round(pt.Y), Round(pt.Z)).GetHashCode();

            private double Round(double num) =>
                prec == 0.0 ? num : Math.Floor(num / prec + 0.5) * prec;
        }
    }
}
