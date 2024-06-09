namespace Gile.AutoCAD.Geometry
{
    /// <summary>
    /// Provides extension methods for the CircularArc2d type.
    /// </summary>
    public static class CircularArc2dExtension
    {
        /// <summary>-
        /// Gets the signed area of the circular arc (negative if points are clockwise).
        /// </summary>
        /// <param name="arc">The instance to which the method applies.</param>
        /// <returns>The signed area.</returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException is thrown if <paramref name="arc"/> is null.</exception>
        public static double SignedArea(this CircularArc2d arc)
        {
            Assert.IsNotNull(arc, nameof(arc));
            double rad = arc.Radius;
            double ang = arc.IsClockWise ?
                arc.StartAngle - arc.EndAngle :
                arc.EndAngle - arc.StartAngle;
            return rad * rad * (ang - Math.Sin(ang)) / 2.0;
        }

        /// <summary>
        /// Gets the centroid of the arc.
        /// </summary>
        /// <param name="arc">The instance to which the method applies.</param>
        /// <returns>The centroid of the arc.</returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException is thrown if <paramref name="arc"/> is null.</exception>
        public static Point2d Centroid(this CircularArc2d arc)
        {
            Assert.IsNotNull(arc, nameof(arc));
            Point2d start = arc.StartPoint;
            Point2d end = arc.EndPoint;
            double area = arc.SignedArea();
            double chord = start.GetDistanceTo(end);
            double angle = (end - start).Angle;
            return arc.Center.Polar(angle - Math.PI / 2.0, chord * chord * chord / (12.0 * area));
        }

        /// <summary>
        /// Gets the tangents between the active CircularArc2d instance complete circle and the point.
        /// </summary>
        /// <remarks>
        /// Tangents start points are on the object to which this method applies, end points on the point passed as argument. 
        /// Tangents are always returned in the same order: the tangent on the left side of the line from the circular arc center to the point before the other one.  
        /// </remarks>
        /// <param name="arc">The instance to which the method applies.</param>
        /// <param name="pt">The Point2d to which tangents are searched.</param>
        /// <returns>An array of LineSegement2d representing the tangents (2) or <c>null</c> if there is none.</returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException is thrown if <paramref name="arc"/> is null.</exception>
        public static LineSegment2d[]? GetTangentsTo(this CircularArc2d arc, Point2d pt)
        {
            Assert.IsNotNull(arc, nameof(arc));
            // check if the point is inside the circle
            Point2d center = arc.Center;
            if (pt.GetDistanceTo(center) <= arc.Radius)
                return null;

            Vector2d vec = center.GetVectorTo(pt) / 2.0;
            CircularArc2d tmp = new(center + vec, vec.Length);
            Point2d[] inters = arc.IntersectWith(tmp);
            if (inters == null)
                return null;
            LineSegment2d[] result = new LineSegment2d[2];
            Vector2d v1 = center.GetVectorTo(inters[0]);
            int i = vec.X * v1.Y - vec.Y - v1.X > 0 ? 0 : 1;
            int j = i ^ 1;
            result[i] = new LineSegment2d(inters[0], pt);
            result[j] = new LineSegment2d(inters[1], pt);
            return result;
        }

        /// <summary>
        /// Gets the tangents between the active CircularArc2d instance complete circle and another one. 
        /// </summary>
        /// <remarks>
        /// Tangents start points are on the object to which this method applies, end points on the one passed as argument. 
        /// Tangents are always returned in the same order: outer tangents before inner tangents, and for both, 
        /// the tangent on the left side of the line from this circular arc center to the other one before the other one. 
        /// </remarks>
        /// <param name="arc">The instance to which the method applies.</param>
        /// <param name="other">The CircularArc2d to which searched for tangents.</param>
        /// <param name="flags">An enum value specifying which type of tangent is returned.</param>
        /// <returns>An array of LineSegment2d representing the tangents (maybe 2 or 4) or <c>null</c> if there is none.</returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException is thrown if <paramref name="arc"/> is null.</exception>
        /// <exception cref="ArgumentNullException">ArgumentNullException is thrown if <paramref name="other"/> is null.</exception>
        public static LineSegment2d[]? GetTangentsTo(this CircularArc2d arc, CircularArc2d other, TangentType flags)
        {
            Assert.IsNotNull(arc, nameof(arc));
            Assert.IsNotNull(other, nameof(other));
            // check if a circle is inside the other
            double dist = arc.Center.GetDistanceTo(other.Center);
            if (dist - Math.Abs(arc.Radius - other.Radius) <= Tolerance.Global.EqualPoint)
                return null;

            // check if circles overlap
            bool overlap = arc.Radius + other.Radius >= dist;
            if (overlap && flags == TangentType.Inner)
                return null;

            CircularArc2d tmp1, tmp2;
            Point2d[] inters;
            Vector2d vec1, vec2, vec = other.Center - arc.Center;
            int i, j;
            LineSegment2d[] result = new LineSegment2d[(int)flags == 3 && !overlap ? 4 : 2];

            // outer tangents
            if ((flags & TangentType.Outer) > 0)
            {
                if (arc.Radius == other.Radius)
                {
                    Line2d perp = new(arc.Center, vec.GetPerpendicularVector());
                    inters = arc.IntersectWith(perp);
                    if (inters == null)
                        return null;
                    vec1 = (inters[0] - arc.Center).GetNormal();
                    i = vec.X * vec1.Y - vec.Y - vec1.X > 0 ? 0 : 1;
                    j = i ^ 1;
                    result[i] = new LineSegment2d(inters[0], inters[0] + vec);
                    result[j] = new LineSegment2d(inters[1], inters[1] + vec);
                }
                else
                {
                    Point2d center = arc.Radius < other.Radius ? other.Center : arc.Center;
                    tmp1 = new CircularArc2d(center, Math.Abs(arc.Radius - other.Radius));
                    tmp2 = new CircularArc2d(arc.Center + vec / 2.0, dist / 2.0);
                    inters = tmp1.IntersectWith(tmp2);
                    if (inters == null)
                        return null;
                    vec1 = (inters[0] - center).GetNormal();
                    vec2 = (inters[1] - center).GetNormal();
                    i = vec.X * vec1.Y - vec.Y - vec1.X > 0 ? 0 : 1;
                    j = i ^ 1;
                    result[i] = new LineSegment2d(arc.Center + vec1 * arc.Radius, other.Center + vec1 * other.Radius);
                    result[j] = new LineSegment2d(arc.Center + vec2 * arc.Radius, other.Center + vec2 * other.Radius);
                }
            }

            // inner tangents
            if ((flags & TangentType.Inner) > 0 && !overlap)
            {
                double ratio = arc.Radius / (arc.Radius + other.Radius) / 2.0;
                tmp1 = new CircularArc2d(arc.Center + vec * ratio, dist * ratio);
                inters = arc.IntersectWith(tmp1);
                if (inters == null)
                    return null;
                vec1 = (inters[0] - arc.Center).GetNormal();
                vec2 = (inters[1] - arc.Center).GetNormal();
                i = vec.X * vec1.Y - vec.Y - vec1.X > 0 ? 2 : 3;
                j = i == 2 ? 3 : 2;
                result[i] = new LineSegment2d(arc.Center + vec1 * arc.Radius, other.Center + vec1.Negate() * other.Radius);
                result[j] = new LineSegment2d(arc.Center + vec2 * arc.Radius, other.Center + vec2.Negate() * other.Radius);
            }
            return result;
        }
    }
}
