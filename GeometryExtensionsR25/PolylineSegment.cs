

namespace Gile.AutoCAD.Geometry
{
    /// <summary>
    /// Descibes a Polyline segment
    /// </summary>
    public class PolylineSegment
    {
        #region Properties

        /// <summary>
        /// Gets or sets the start point of the segment.
        /// </summary>
        public Point2d StartPoint { get; set; }

        /// <summary>
        /// Gets or sets the end point of the segment.
        /// </summary>
        public Point2d EndPoint { get; set; }

        /// <summary>
        /// Gets or sets the bulge of the segment.
        /// </summary>
        public double Bulge { get; set; }

        /// <summary>
        /// Gets or sets the start width of the segment.
        /// </summary>
        public double StartWidth { get; set;}

        /// <summary>
        /// Gets or sets the end width of the segment.
        /// </summary>
        public double EndWidth { get; set; }

        /// <summary>
        /// Gets a value indicating if the segment is linear.
        /// </summary>
        public bool IsLinear => Bulge == 0.0;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of PolylineSegment.
        /// </summary>
        /// <param name="startPoint">Start point of the segment.</param>
        /// <param name="endPoint">End point of the segment.</param>
        /// <param name="bulge">Bulge of the segment (default 0.0).</param>
        /// <param name="constantWidth">Constant width of the segment (default 0.0).</param>
        public PolylineSegment(Point2d startPoint, Point2d endPoint, double bulge = 0.0, double constantWidth = 0.0) 
            : this(startPoint, endPoint, bulge, constantWidth, constantWidth)
        { }

        /// <summary>
        /// Creates a new instance of PolylineSegment.
        /// Créé une nouvelle instance de PolylineSegment.
        /// </summary>
        /// <param name="startPoint">Start point of the segment.</param>
        /// <param name="endPoint">End point of the segment.</param>
        /// <param name="bulge">Bulge of the segment (default 0.0).</param>
        /// <param name="startWidth">Start width of the segment.</param>
        /// <param name="endWidth">End width of the segment.</param>
        public PolylineSegment(Point2d startPoint, Point2d endPoint, double bulge, double startWidth, double endWidth)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Bulge = bulge;
            StartWidth = startWidth;
            EndWidth = endWidth;
        }

        /// <summary>
        /// Creates a new instance of PolylineSegment.
        /// </summary>
        /// <param name="line">An instance of LineSegment2d.</param>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="line"/> is null.</exception>
        public PolylineSegment(LineSegment2d line)
        {
            Assert.IsNotNull(line, nameof(line));
            StartPoint = line.StartPoint;
            EndPoint = line.EndPoint;
            Bulge = 0.0;
            StartWidth = 0.0;
            EndWidth = 0.0;
        }

        /// <summary>
        /// Creates a new instance of PolylineSegment.
        /// </summary>
        /// <param name="arc">An instance of CircularArc2d.</param>
        /// <exception cref="ArgumentNullException">ArgumentException is thrown if <paramref name="arc"/> is null.</exception>
        public PolylineSegment(CircularArc2d arc)
        {
            Assert.IsNotNull(arc, nameof(arc));
            StartPoint = arc.StartPoint;
            EndPoint = arc.EndPoint;
            Bulge = Math.Tan((arc.EndAngle - arc.StartAngle) / 4.0);
            if (arc.IsClockWise) Bulge = -Bulge;
            StartWidth = 0.0;
            EndWidth = 0.0;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the parameter at point value.
        /// </summary>
        /// <param name="pt">The Point2d on the segment where we want to know the parameter.</param>
        /// <returns>A real number beween 0.0 and 1.0, or -1.0 if the point is not on the segment.</returns>
        public double GetParameterOf(Point2d pt)
        {
            if (IsLinear)
            {
                LineSegment2d? line = ToLineSegment();
                return line!.IsOn(pt) ? StartPoint.GetDistanceTo(pt) / line.Length : -1.0;
            }
            else
            {
                CircularArc2d? arc = ToCircularArc();
                return arc!.IsOn(pt) ?
                    arc.GetLength(arc.GetParameterOf(StartPoint), arc.GetParameterOf(pt)) /
                    arc.GetLength(arc.GetParameterOf(StartPoint), arc.GetParameterOf(EndPoint)) :
                    -1.0;
            }
        }

        /// <summary>
        /// Reverses the segment.
        /// </summary>
        public void Inverse()
        {
            Point2d tmpPoint = StartPoint;
            double tmpWidth = StartWidth;
            StartPoint = EndPoint;
            EndPoint = tmpPoint;
            Bulge = -Bulge;
            StartWidth = EndWidth;
            EndWidth = tmpWidth;
        }

        /// <summary>
        /// Converts the linear segment into an instance of Line2dSegment.
        /// </summary>
        /// <returns>A new instance of LineSegment2d or null if the segment is not linear.</returns>
        public LineSegment2d? ToLineSegment() => 
            IsLinear ? new LineSegment2d(StartPoint, EndPoint) : null;

        /// <summary>
        /// Converts the circular arc segment into an instance of CircularArc2d.
        /// </summary>
        /// <returns>A new instance of CircularArc2d or null if the segment is linear.</returns>
        public CircularArc2d? ToCircularArc() => 
            IsLinear ? null : new CircularArc2d(StartPoint, EndPoint, Bulge, false);

        /// <summary>
        /// Convertit le segment en instance de Curve2d.
        /// </summary>
        /// <returns>Une nouvelle instance de Curve2d.</returns>
        public Curve2d ToCurve2d()
        {
            return IsLinear ?
                (Curve2d)new LineSegment2d(StartPoint, EndPoint) :
                (Curve2d)new CircularArc2d(StartPoint, EndPoint, Bulge, false);
        }

        /// <summary>
        /// Evaluates is the specified PolylineSegment is equal to the current one. 
        /// </summary>
        /// <param name="obj">Object to compre.</param>
        /// <returns>true si les objets sont considérés égaux ; null sinon.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not PolylineSegment seg) return false;
            if (seg.GetHashCode() != GetHashCode()) return false;
            if (!StartPoint.IsEqualTo(seg.StartPoint)) return false;
            if (!EndPoint.IsEqualTo(seg.EndPoint)) return false;
            if (Bulge != seg.Bulge) return false;
            if (StartWidth != seg.StartWidth) return false;
            if (EndWidth != seg.EndWidth) return false;
            return true;
        }

        /// <summary>
        /// Serves as the PolylineSegment hash function.
        /// </summary>
        /// <returns>A hash code for the current PolylineSegment.</returns>
        public override int GetHashCode() =>
            StartPoint.GetHashCode() ^
            EndPoint.GetHashCode() ^
            Bulge.GetHashCode() ^
            StartWidth.GetHashCode() ^
            EndWidth.GetHashCode();

        /// <summary>
        /// Overrides the ToString method for the PolylineSegment type.
        /// </summary>
        /// <returns>A string containing the properties of the current instance.</returns>
        public override string ToString() =>
            $"{StartPoint}, {EndPoint}, {Bulge}, {StartWidth}, {EndWidth}";

        #endregion
    }
}
