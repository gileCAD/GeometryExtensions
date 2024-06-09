using System.Globalization;



namespace Gile.AutoCAD.Geometry
{
    /// <summary>
    /// Describes a triangle within a plane. It can be seen as a structure of three Point2d.
    /// </summary>
    public readonly struct Triangle2d
    {
        #region Fields

        readonly Point2d point0;
        readonly Point2d point1;
        readonly Point2d point2;
        readonly Point2d[] points;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of Triangle2d.
        /// </summary>
        /// <param name="points">Array of three Point2d.</param>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="points"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">ArgumentOutOfRangeException is thrown if <paramref name="points"/> length is different from 3.</exception>
        public Triangle2d(Point2d[] points)
        {
            Assert.IsNotNull(points, nameof(points));
            if (points.Length != 3)
                throw new ArgumentOutOfRangeException(nameof(points), "Needs 3 points.");

            this.points = points;
            point0 = points[0];
            point1 = points[1];
            point2 = points[2];
        }

        /// <summary>
        /// Creates a new instance of Triangle2d.
        /// </summary>
        /// <param name="a">First point.</param>
        /// <param name="b">Second point.</param>
        /// <param name="c">Third point.</param>
        public Triangle2d(Point2d a, Point2d b, Point2d c)
        {
            point0 = a;
            point1 = b;
            point2 = c;
            points = [a, b, c];
        }

        /// <summary>
        /// Creates a new instance of Triangle2d.
        /// </summary>
        /// <param name="org">Origin of the Triangle2d (first point).</param>
        /// <param name="v1">Vector from origin to second point.</param>
        /// <param name="v2">Vector from origin to third point.</param>
        public Triangle2d(Point2d org, Vector2d v1, Vector2d v2)

        {
            point0 = org;
            point1 = org + v1;
            point2 = org + v2;
            points = [point0, point1, point2];
        }

        #endregion

        /// <summary>
        /// Gets the point at specified index.
        /// </summary>
        /// <param name="i">Index of the point.</param>
        /// <returns>The point at specified index..</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// IndexOutOfRangeException is thrown if index is lower than 0 or greater than 2.</exception>
        public Point2d this[int i] => points[i];

        #region Properties

        /// <summary>
        /// Gets the centroid.
        /// </summary>
        public Point2d Centroid =>
            (point0 + point1.GetAsVector() + point2.GetAsVector()) / 3.0;

        /// <summary>
        /// Gets the circumscribed circle.
        /// </summary>
        public CircularArc2d? CircumscribedCircle
        {
            get
            {
                Line2d l1 = GetSegmentAt(0).GetBisector();
                Line2d l2 = GetSegmentAt(1).GetBisector();
                Point2d[] inters = l1.IntersectWith(l2);
                if (inters == null)
                    return null;
                return new CircularArc2d(inters[0], inters[0].GetDistanceTo(point0));
            }
        }

        /// <summary>
        /// Gets the inscribed circle.
        /// </summary>
        public CircularArc2d? InscribedCircle
        {
            get
            {
                Vector2d v1 = point0.GetVectorTo(point1).GetNormal();
                Vector2d v2 = point0.GetVectorTo(point2).GetNormal();
                Vector2d v3 = point1.GetVectorTo(point2).GetNormal();
                if (v1.IsEqualTo(v2) || v2.IsEqualTo(v3))
                    return null;
                Line2d l1 = new(point0, v1 + v2);
                Line2d l2 = new(point1, v1.Negate() + v3);
                Point2d[] inters = l1.IntersectWith(l2);
                return new CircularArc2d(inters[0], GetSegmentAt(0).GetDistanceTo(inters[0]));
            }
        }

        /// <summary>
        /// Gets a value indicating if the points are clockwise.
        /// </summary>
        public bool IsClockwise => SignedArea < 0.0;

        /// <summary>
        /// Gets the signed area (negative if points are clockwise).
        /// Obtient l'aire algébrique.
        /// </summary>
        public double SignedArea =>
            ((point1.X - point0.X) * (point2.Y - point0.Y) -
            (point2.X - point0.X) * (point1.Y - point0.Y)) / 2.0;

        #endregion

        #region Méthodes publiques

        /// <summary>
        /// Converts the current instance into a Triangle3d according to the specified plane.
        /// </summary>
        /// <param name="plane">Plane of the Triangle3d.</param>
        /// <returns>The new instance of Triangle3d.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="plane"/> is null.</exception>
        public Triangle3d Convert3d(Plane plane)
        {
            Assert.IsNotNull(plane, nameof(plane));
            return new Triangle3d(Array.ConvertAll(points, x => x.Convert3d(plane)));
        }

        /// <summary>
        /// Converts the current instance into a Triangle3d according to the plane defined by its Normal and its Elevation.
        /// </summary>
        /// <param name="normal">Normal of the plane.</param>
        /// <param name="elevation">Elevation of the plane.</param>
        /// <returns>The new instance of Triangle3d.</returns>
        public Triangle3d Convert3d(Vector3d normal, double elevation) =>
            new(Array.ConvertAll(points, x => x.Convert3d(normal, elevation)));

        /// <summary>
        /// Gets the angle between two sides at the specified index.
        /// </summary>.
        /// <param name="index">Index of the vertex.</param>
        /// <returns>The angle in radians.</returns>
        public double GetAngleAt(int index)
        {
            double ang =
                this[index].GetVectorTo(this[(index + 1) % 3]).GetAngleTo(
                this[index].GetVectorTo(this[(index + 2) % 3]));
            if (ang > Math.PI * 2)
                return Math.PI * 2 - ang;
            else
                return ang;
        }

        /// <summary>
        /// Gets the LineSegement2d at specified index.
        /// </summary>
        /// <param name="index">Index of the segment.</param>
        /// <returns>The LineSegement2d at specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// IndexOutOfRangeException is thrown if indes is lower than 0 or greater than 2.</exception>
        public LineSegment2d GetSegmentAt(int index)
        {
            if (index > 2)
                throw new IndexOutOfRangeException("Index out of range");
            return new LineSegment2d(this[index], this[(index + 1) % 3]);
        }

        /// <summary>
        /// Gets the intersection points between the current instance and a line using Tolerance.Global.
        /// </summary>
        /// <param name="line2d">The line for which the intersections are searched.</param>
        /// <returns>The list of intersection points (an empty list if none was found).</returns>
        public List<Point2d> IntersectWith(LinearEntity2d line2d) => IntersectWith(line2d, Tolerance.Global);

        /// <summary>
        /// Gets the intersection points between the current instance and a line using the specified Tolerance.
        /// </summary>
        /// <param name="line2d">The line for which the intersections are searched.</param>
        /// <param name="tol">Tolerance to be used for comparisons.</param>
        /// <returns>The list of intersection points (an empty list if none was found).</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="line2d"/> is null.</exception>
        public List<Point2d> IntersectWith(LinearEntity2d line2d, Tolerance tol)
        {
            Assert.IsNotNull(line2d, nameof(line2d));
            List<Point2d> result = [];
            for (int i = 0; i < 3; i++)
            {
                Point2d[] inters = line2d.IntersectWith(GetSegmentAt(i), tol);
                if (inters != null && inters.Length != 0 && !result.Contains(inters[0]))
                    result.Add(inters[0]);
            }
            return result;
        }

        /// <summary>
        /// Reverse the order of points without changing the origin.
        /// </summary>
        public Triangle2d Inverse()
        {
            return new Triangle2d(point0, point2, point1);
        }

        /// <summary>
        /// Evaluates if the current instance is equal to another Triangle2d using Tolerance.Global.
        /// </summary>
        /// <param name="other">Triangle to be compared to.</param>
        /// <returns>true, if vertices are equal; false, otherwise.</returns>
        public bool IsEqualTo(Triangle2d other) => IsEqualTo(other, Tolerance.Global);

        /// <summary>
        /// Evaluates if the current instance is equal to another Triangle2d using the specified Tolerance.
        /// </summary>
        /// <param name="other">Triangle to be compared to.</param>
        /// <param name="tol">Tolerance to be used for comparisons.</param>
        /// <returns>true, if vertices are equal; false, otherwise.</returns>
        public bool IsEqualTo(Triangle2d other, Tolerance tol) =>
            other[0].IsEqualTo(point0, tol) && other[1].IsEqualTo(point1, tol) && other[2].IsEqualTo(point2, tol);

        /// <summary>
        /// Gets a value indicating if the the Point2d is strictly inside the current triangle.
        /// </summary>
        /// <param name="pt">Point to be evaluated.</param>
        /// <returns>true, if the point is inside; false, otherwise.</returns>
        public bool IsPointInside(Point2d pt)
        {
            if (IsPointOn(pt))
                return false;
            List<Point2d> inters = IntersectWith(new Ray2d(pt, Vector2d.XAxis));
            if (inters.Count != 1)
                return false;
            Point2d p = inters[0];
            return !p.IsEqualTo(this[0]) && !p.IsEqualTo(this[1]) && !p.IsEqualTo(this[2]);
        }

        /// <summary>
        /// Gets a value indicating if the the Point2d is on an segment of the current triangle.
        /// </summary>
        /// <param name="pt">Point to be evaluated.</param>
        /// <returns>true, if the point is on a segment; false, otherwise.</returns>
        public bool IsPointOn(Point2d pt) =>
                pt.IsEqualTo(this[0]) ||
                pt.IsEqualTo(this[1]) ||
                pt.IsEqualTo(this[2]) ||
                pt.IsBetween(this[0], this[1]) ||
                pt.IsBetween(this[1], this[2]) ||
                pt.IsBetween(this[2], this[0]);

        /// <summary>
        /// Converts the triangle into a Point2d array.
        /// </summary>
        /// <returns>A Point2d array containing the 3 points.</returns>
        public Point2d[] ToArray() => points;

        /// <summary>
        /// Transforms the triangle using transformation matrix.
        /// </summary>
        /// <param name="mat">2D transformation matrix.</param>
        /// <returns>The new instance of Triangle2d.</returns>
        public Triangle2d TransformBy(Matrix2d mat) =>
            new(Array.ConvertAll(points, new Converter<Point2d, Point2d>(p => p.TransformBy(mat))));

        #endregion

        #region Overrides

        /// <summary>
        /// Evaluates if the object is equal to the current instance of Triangle2d.
        /// </summary>
        /// <param name="obj">Object to be compared.</param>
        /// <returns>true, if vertices are equal; false, otherwise.</returns>
        public override bool Equals(object? obj) =>
            obj is Triangle2d tri && tri.IsEqualTo(this);

        /// <summary>
        /// Serves as the Triangle2d hash function.
        /// </summary>
        /// <returns>A hash code for the current Triangle2d instance..</returns>
        public override int GetHashCode()
        {
            return point0.GetHashCode() ^ point1.GetHashCode() ^ point2.GetHashCode();
        }

        /// <summary>
        /// Returns a string representing the current instance of Triangle2d.
        /// </summary>
        /// <returns>A string containing the 3 points separated with commas.</returns>
        public override string ToString() =>
            $"({point0},{point1},{point2})";

        /// <summary>
        /// Returns a string representing the current instance of Triangle2d.
        /// </summary>
        /// <param name="format">String format to be used for the points.</param>
        /// <returns>A string containing the 3 points in the specified format, separated by commas.</returns>
        public string ToString(string format) =>
            string.IsNullOrEmpty(format) ?
                $"({point0},{point1},{point2})" :
                $"({point0.ToString(format, CultureInfo.InvariantCulture)}," +
                $"{point1.ToString(format, CultureInfo.InvariantCulture)}," +
                $"{point2.ToString(format, CultureInfo.InvariantCulture)})";

        /// <summary>
        /// Returns a string representing the current instance of Triangle2d.
        /// </summary>
        /// <param name="format">String format to be used for the points.</param>
        /// <param name="provider">Format provider to be used to format the points.</param>
        /// <returns>A string containing the 3 points in the specified format, separated by commas.</returns>
        public string ToString(string format, IFormatProvider provider) =>
            $"({point0.ToString(format, provider)}," +
            $"{point1.ToString(format, provider)}," +
            $"{point2.ToString(format, provider)})";

        /// <summary>
        /// Evaluates if the instance of Triangle2d are equal.
        /// </summary>
        /// <param name="left">Instance of Triangle2d.</param>
        /// <param name="right">Instance of Triangle2d.</param>
        /// <returns>true, if the triangles are equal; false, otherwise.</returns>
        public static bool operator ==(Triangle2d left, Triangle2d right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Evaluates if the instance of Triangle2d are not equal.
        /// </summary>
        /// <param name="left">Instance of Triangle2d.</param>
        /// <param name="right">Instance of Triangle2d.</param>
        /// <returns>true, if the triangles are not equal; false, otherwise.</returns>
        public static bool operator !=(Triangle2d left, Triangle2d right)
        {
            return !(left == right);
        }

        #endregion
    }
}
