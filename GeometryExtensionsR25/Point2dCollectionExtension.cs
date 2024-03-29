using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.Geometry;

namespace Gile.AutoCAD.R25.Geometry
{
    /// <summary>
    /// Provides extension methods for the Point2dCollection and IEnumerable&lt;Point2d&gt; types.
    /// </summary>
    public static class Point2dCollectionExtension
    {
        /// <summary>
        /// Removes duplicated points using Tolerance.Global.EqualPoint.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <returns>A sequence of distinct points.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="source"/> is null.</exception>
        public static IEnumerable<Point2d> RemoveDuplicates(this Point2dCollection source)
        {
            ArgumentNullException.ThrowIfNull(source);
            return source.RemoveDuplicates(Tolerance.Global);
        }

        /// <summary>
        /// Removes duplicated points using the specified Tolerance.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <param name="tolerance">The Tolerance to be used in equality comparison.</param>
        /// <returns>A sequence of distinct points.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="source"/> is null.</exception>
        public static IEnumerable<Point2d> RemoveDuplicates(this Point2dCollection source, Tolerance tolerance)
        {
            ArgumentNullException.ThrowIfNull(source);
            return source.Cast<Point2d>().Distinct(new Point2dComparer(tolerance));
        }

        /// <summary>
        /// Removes duplicated points using Tolerance.Global.EqualPoint.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <returns>A sequence of distinct points.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="source"/> is null.</exception>
        public static IEnumerable<Point2d> RemoveDuplicates(this IEnumerable<Point2d> source)
        {
            ArgumentNullException.ThrowIfNull(source);
            return source.RemoveDuplicates(Tolerance.Global);
        }

        /// <summary>
        /// Removes duplicated points using the specified Tolerance.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <param name="tolerance">The Tolerance to be used in equality comparison.</param>
        /// <returns>A sequence of distinct points.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="source"/> is null.</exception>
        public static IEnumerable<Point2d> RemoveDuplicates(this IEnumerable<Point2d> source, Tolerance tolerance)
        {
            ArgumentNullException.ThrowIfNull(source);
            return source.Distinct(new Point2dComparer(tolerance));
        }

        /// <summary>
        /// Evaluates if the collection contains <c>point</c> using Tolerance.Global.EqualPoint.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <param name="point">Point to search for.</param>
        /// <returns>true, if <c>point</c> is found; false, otherwise.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="source"/> is null.</exception>
        public static bool Contains(this Point2dCollection source, Point2d point)
        {
            ArgumentNullException.ThrowIfNull(source);
            return source.Contains(point, Tolerance.Global);
        }

        /// <summary>
        /// Evaluates if the collection contains <c>point</c> using the specified tolerance.
        /// </summary>
        /// <param name="source">The instance to which this method applies.</param>
        /// <param name="point">Point to search for.</param>
        /// <param name="tol">The Tolerance to be used in equality comparison..</param>
        /// <returns>true, if <c>point</c> is found; false, otherwise.</returns>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="source"/> is null.</exception>
        public static bool Contains(this Point2dCollection source, Point2d point, Tolerance tol)
        {
            ArgumentNullException.ThrowIfNull(source);
            for (int i = 0; i < source.Count; i++)
            {
                if (point.IsEqualTo(source[i], tol))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Provides equality comparison methods for the Point2d type.
        /// </summary>
        /// <remarks>
        /// Creates a new instance ot Point2dComparer
        /// </remarks>
        /// <param name="tolerance">The Tolerance to be used in equality comparison.</param>
        class Point2dComparer(Tolerance tolerance) : IEqualityComparer<Point2d>
        {
            private Tolerance tolerance = tolerance;
            private readonly double prec = tolerance.EqualPoint * 10.0;

            /// <summary>
            /// Evaluates if two points are equal.
            /// </summary>
            /// <param name="a">First point.</param>
            /// <param name="b">Second point.</param>
            /// <returns>true, if the two points are equal; false, otherwise.</returns>
            public bool Equals(Point2d a, Point2d b)
            {
                return a.IsEqualTo(b, tolerance);
            }

            /// <summary>
            /// Serves as hashing function for the Point2d type.
            /// </summary>
            /// <param name="pt">Point.</param>
            /// <returns>The hash code.</returns>
            public int GetHashCode(Point2d pt) =>
                new Point2d(Round(pt.X), Round(pt.Y)).GetHashCode();

            private double Round(double num)
            {
                return prec == 0.0 ? num : Math.Floor(num / prec + 0.5) * prec;
            }
        }
    }
}
