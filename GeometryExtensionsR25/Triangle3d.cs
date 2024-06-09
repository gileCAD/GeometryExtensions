using System.Globalization;

namespace Gile.AutoCAD.Geometry
{
    /// <summary>
    /// Describes a triangle within the 3D space. It can be seen as a structure of three Point3d.
    /// </summary>
    public readonly struct Triangle3d : IFormattable
    {
        #region Fields

        readonly Point3d point0;
        readonly Point3d point1;
        readonly Point3d point2;
        readonly Point3d[] points;

        #endregion

        #region Constructors


        /// <summary>
        /// Creates a new instance of Triangle3d.
        /// </summary>
        /// <param name="points">Array of three Point3d.</param>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="points"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">ArgumentOutOfRangeException is thrown if <paramref name="points"/> length is different from 3.</exception>
        public Triangle3d(Point3d[] points)
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
        /// Creates a new instance of Triangle3d.
        /// </summary>
        /// <param name="a">First point.</param>
        /// <param name="b">Second point.</param>
        /// <param name="c">Third point.</param>
        public Triangle3d(Point3d a, Point3d b, Point3d c)
        {
            point0 = a;
            point1 = b;
            point2 = c;
            points = [a, b, c];
        }

        /// <summary>
        /// Creates a new instance of Triangle3d.
        /// </summary>
        /// <param name="org">Origin of the Triangle2d (first point).</param>
        /// <param name="v1">Vector from origin to second point.</param>
        /// <param name="v2">Vector from origin to third point.</param>
        public Triangle3d(Point3d org, Vector3d v1, Vector3d v2)

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
        public Point3d this[int i] => points[i];

        #region Properties

        /// <summary>
        /// Gets the area of the triangle.
        /// </summary>
        public double Area =>
            Math.Abs(((point1.X - point0.X) * (point2.Y - point0.Y) -
                (point2.X - point0.X) * (point1.Y - point0.Y)) / 2.0);

        /// <summary>
        /// Gets the centroid.
        /// </summary>
        public Point3d Centroid => (point0 + point1.GetAsVector() + point2.GetAsVector()) / 3.0;

        /// <summary>
        /// Gets the circumscribed circle.
        /// </summary>
        public CircularArc3d? CircumscribedCircle
        {
            get
            {
                CircularArc2d? ca2d = Convert2d().CircumscribedCircle;
                if (ca2d == null)
                    return null;
                return new CircularArc3d(ca2d.Center.Convert3d(GetPlane()), Normal, ca2d.Radius);
            }
        }

        /// <summary>
        /// Gets the elevation of the plane the triangle lies on.
        /// </summary>
        public double Elevation => point0.TransformBy(Matrix3d.WorldToPlane(Normal)).Z;

        /// <summary>
        /// Gets the unit vector of the greatest slope of the plane the triangle lies on.
        /// </summary>
        public Vector3d GreatestSlope =>
            Normal.IsParallelTo(Vector3d.ZAxis) ?
                new Vector3d(0.0, 0.0, 0.0) :
                Normal.Z == 0.0 ?
                    Vector3d.ZAxis.Negate() :
                    new Vector3d(-Normal.Y, Normal.X, 0.0).CrossProduct(Normal).GetNormal();

        /// <summary>
        /// Gets the unit vector of the horizontal of the plane the triangle lies on.
        /// </summary>
        public Vector3d Horizontal =>
            Normal.IsParallelTo(Vector3d.ZAxis) ?
                Vector3d.XAxis :
                new Vector3d(-Normal.Y, Normal.X, 0.0).GetNormal();

        /// <summary>
        /// Gets the inscribed circle.
        /// </summary>
        public CircularArc3d? InscribedCircle
        {
            get
            {
                CircularArc2d? ca2d = Convert2d().InscribedCircle;
                if (ca2d == null)
                    return null;
                return new CircularArc3d(ca2d.Center.Convert3d(GetPlane()), Normal, ca2d.Radius);
            }
        }

        /// <summary>
        /// Get a value indicating if the plane the triangle lies on is horizontal.
        /// </summary>
        public bool IsHorizontal => point0.Z == point1.Z && point0.Z == point2.Z;

        /// <summary>
        /// Gets the Normal of the plane the triangle lies on.
        /// </summary>
        public Vector3d Normal => (point1 - point0).CrossProduct(point2 - point0).GetNormal();

        /// <summary>
        /// Gets the slope of the triangle expressed in percent.
        /// </summary>
        public double SlopePerCent =>
            Normal.Z == 0.0 ?
                double.PositiveInfinity :
                Math.Abs(100.0 * Math.Sqrt(Normal.X * Normal.X + Normal.Y * Normal.Y) / Normal.Z);

        /// <summary>
        /// Gets the coordinate system of the triangle (origin = Centroid, X axis = Horizontal, Z axis = Normal).
        /// </summary>
        public Matrix3d SlopeUCS
        {
            get
            {
                Point3d origin = Centroid;
                Vector3d zaxis = Normal;
                Vector3d xaxis = Horizontal;
                Vector3d yaxis = zaxis.CrossProduct(xaxis).GetNormal();
                return new Matrix3d([
                    xaxis.X, yaxis.X, zaxis.X, origin.X,
                    xaxis.Y, yaxis.Y, zaxis.Y, origin.Y,
                    xaxis.Z, yaxis.Z, zaxis.Z, origin.Z,
                    0.0, 0.0, 0.0, 1.0]);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the current instance into a Triangle2d.
        /// </summary>
        /// <returns>The new instance of Triangle2d.</returns>
        public Triangle2d Convert2d()
        {
            var plane = GetPlane();
            return new Triangle2d(Array.ConvertAll(points, x => x.Convert2d(plane)));
        }

        /// <summary>
        /// Projects the current instance onto the XY plane.
        /// </summary>
        /// <returns>The resulting Triangle2d.</returns>
        public Triangle2d Flatten() =>
            new(
                new Point2d(this[0].X, this[0].Y),
                new Point2d(this[1].X, this[1].Y),
                new Point2d(this[2].X, this[2].Y));

        /// <summary>
        /// Gets the angle between two sides at the specified index.
        /// </summary>.
        /// <param name="index">Index of the vertex.</param>
        /// <returns>The angle in radians.</returns>
        public double GetAngleAt(int index) =>
            this[index].GetVectorTo(this[(index + 1) % 3]).GetAngleTo(
                this[index].GetVectorTo(this[(index + 2) % 3]));

        /// <summary>
        /// Gets the bounded plane defined by the current triangle.
        /// </summary>
        /// <returns>The bounded plane.</returns>
        public BoundedPlane GetBoundedPlane() => new(point0, point1, point2);

        /// <summary>
        /// Gets the unbounded plane defined by the current triangle.
        /// </summary>
        /// <returns>The plane.</returns>
        public Plane GetPlane()
        {
            Point3d origin =
                new Point3d(0.0, 0.0, Elevation).TransformBy(Matrix3d.PlaneToWorld(Normal));
            return new Plane(origin, Normal);
        }

        /// <summary>
        /// Gets the LineSegement3d at specified index.
        /// </summary>
        /// <param name="index">Index of the segment.</param>
        /// <returns>The LineSegement2d at specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// IndexOutOfRangeException is thrown if indes is lower than 0 or greater than 2.</exception>
        public LineSegment3d GetSegmentAt(int index)
        {
            if (index > 2)
                throw new IndexOutOfRangeException("Index out of range");
            return new LineSegment3d(this[index], this[(index + 1) % 3]);
        }

        /// <summary>
        /// Reverse the order of points without changing the origin.
        /// </summary>
        public Triangle3d Inverse()
        {
            return new Triangle3d(point0, point2, point1);
        }

        /// <summary>
        /// Evaluates if the current instance is equal to another Triangle2d using Tolerance.Global.
        /// </summary>
        /// <param name="other">Triangle to be compared to.</param>
        /// <returns>true, if vertices are equal; false, otherwise.</returns>
        public bool IsEqualTo(Triangle3d other)
        {
            return IsEqualTo(other, Tolerance.Global);
        }

        /// <summary>
        /// Evaluates if the current instance is equal to another Triangle3d using the specified Tolerance.
        /// </summary>
        /// <param name="other">Triangle to be compared to.</param>
        /// <param name="tol">Tolerance to be used for comparisons.</param>
        /// <returns>true, if vertices are equal; false, otherwise.</returns>
        public bool IsEqualTo(Triangle3d other, Tolerance tol)
        {
            return other[0].IsEqualTo(point0, tol) && other[1].IsEqualTo(point1, tol) && other[2].IsEqualTo(point2, tol);
        }

        /// <summary>
        /// Gets a value indicating if the the Point2d is strictly inside the current triangle.
        /// </summary>
        /// <param name="pt">Point to be evaluated.</param>
        /// <returns>true, if the point is inside; false, otherwise.</returns>
        public bool IsPointInside(Point3d pt)
        {
            Tolerance tol = new(1e-9, 1e-9);
            Vector3d v1 = pt.GetVectorTo(point0).CrossProduct(pt.GetVectorTo(point1)).GetNormal();
            Vector3d v2 = pt.GetVectorTo(point1).CrossProduct(pt.GetVectorTo(point2)).GetNormal();
            Vector3d v3 = pt.GetVectorTo(point2).CrossProduct(pt.GetVectorTo(point0)).GetNormal();
            return v1.IsEqualTo(v2, tol) && v2.IsEqualTo(v3, tol);
        }

        /// <summary>
        /// Gets a value indicating if the the Point3d is on an segment of the current triangle.
        /// </summary>
        /// <param name="pt">Point to be evaluated.</param>
        /// <returns>true, if the point is on a segment; false, otherwise.</returns>
        public bool IsPointOn(Point3d pt)
        {
            Tolerance tol = new(1e-9, 1e-9);
            Vector3d v0 = new(0.0, 0.0, 0.0);
            Vector3d v1 = pt.GetVectorTo(point0).CrossProduct(pt.GetVectorTo(point1));
            Vector3d v2 = pt.GetVectorTo(point1).CrossProduct(pt.GetVectorTo(point2));
            Vector3d v3 = pt.GetVectorTo(point2).CrossProduct(pt.GetVectorTo(point0));
            return v1.IsEqualTo(v0, tol) || v2.IsEqualTo(v0, tol) || v3.IsEqualTo(v0, tol);
        }

        /// <summary>
        /// Converts the triangle into a Point2d array.
        /// </summary>
        /// <returns>A Point2d array containing the 3 points.</returns>
        public Point3d[] ToArray() => points;

        /// <summary>
        /// Transforms the triangle using transformation matrix.
        /// </summary>
        /// <param name="mat">3D transformation matrix.</param>
        /// <returns>The new instance of Triangle3d.</returns>
        public Triangle3d Transformby(Matrix3d mat) =>
            new(Array.ConvertAll(points, new Converter<Point3d, Point3d>(p => p.TransformBy(mat))));

        #endregion

        #region overrides

        /// <summary>
        /// Evaluates if the object is equal to the current instance of Triangle3d.
        /// </summary>
        /// <param name="obj">Object to be compared.</param>
        /// <returns>true, if vertices are equal; false, otherwise.</returns>
        public override bool Equals(object? obj) =>
            obj is Triangle3d tri && tri.IsEqualTo(this);

        /// <summary>
        /// Serves as the Triangle3d hash function.
        /// </summary>
        /// <returns>A hash code for the current Triangle3d instance..</returns>
        public override int GetHashCode()
        {
            return point0.GetHashCode() ^ point1.GetHashCode() ^ point2.GetHashCode();
        }

        /// <summary>
        /// Returns a string representing the current instance of Triangle3d.
        /// </summary>
        /// <returns>A string containing the 3 points separated by commas.</returns>
        public override string ToString() => $"({point0},{point1},{point2})";

        /// <summary>
        /// Returns a string representing the current instance of Triangle3d.
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
        /// Returns a string representing the current instance of Triangle3d.
        /// </summary>
        /// <param name="format">String format to be used for the points.</param>
        /// <param name="provider">Format provider to be used to format the points.</param>
        /// <returns>A string containing the 3 points in the specified format, separated by commas.</returns>
        public string ToString(string? format, IFormatProvider? provider) =>
            $"({point0.ToString(format, provider)}," +
            $"{point1.ToString(format, provider)}," +
            $"{point2.ToString(format, provider)})";

        /// <summary>
        /// Evaluates if the instance of Triangle3d are equal.
        /// </summary>
        /// <param name="left">Instance of Triangle3d.</param>
        /// <param name="right">Instance of Triangle3d.</param>
        /// <returns>true, if the triangles are equal; false, otherwise.</returns>
        public static bool operator ==(Triangle3d left, Triangle3d right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Evaluates if the instance of Triangle3d are not equal.
        /// </summary>
        /// <param name="left">Instance of Triangle3d.</param>
        /// <param name="right">Instance of Triangle3d.</param>
        /// <returns>true, if the triangles are not equal; false, otherwise.</returns>
        public static bool operator !=(Triangle3d left, Triangle3d right)
        {
            return !(left == right);
        }

        #endregion
    }
}
