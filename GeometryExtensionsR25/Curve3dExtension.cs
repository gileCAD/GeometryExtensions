using Autodesk.AutoCAD.Geometry;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gile.AutoCAD.R25.Geometry
{
    /// <summary>
    /// Provides extension methods for the Curve3d type.
    /// </summary>
    public static class Curve3dExtension
    {
        /// <summary>
        /// Gets the reversed curve without modifying the original one.
        /// </summary>
        /// <param name="curve">Instance this method applies to.</param>
        /// <returns>A new Curve3d with reversed parameters.</returns>
        public static Curve3d GetReversedCurve(this Curve3d curve) =>
            ((Curve3d)curve.Clone()).GetReverseParameterCurve();

        /// <summary>
        /// Order the collection by contiguous curves ([n].EndPoint equals to [n+1].StartPoint)
        /// </summary>
        /// <param name="source">Collection this method applies to.</param>
        /// <param name="tolerance">Tolerance used to compare end points.</param>
        /// <returns>Ordered array of Curve3d.</returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException is thrown if <paramref name="source"/> is null.</exception>
        /// <exception cref="InvalidOperationException">InvalidOperationException is thrown if non-contiguous segments are found.</exception>
        public static Curve3d[] ToOrderedArray(this IEnumerable<Curve3d> source, Tolerance tolerance = default)
        {
            ArgumentNullException.ThrowIfNull(source);

            var input = source as Curve3d[] ?? source.ToArray();
            int length = input.Length;
            if (length < 2)
                return input;

            if (tolerance.Equals(default(Tolerance)))
                tolerance = Tolerance.Global;

            var output = new Curve3d[length];
            Span<Curve3d> span = output;
            var done = new bool[length];
            var current = input[0];
            span[0] = current;
            done[0] = true;
            int count = 1;
            var startPoint = current.StartPoint;
            var endPoint = current.EndPoint;

            while (count < length)
            {
                bool found = false;

                for (int i = 1; i < length; i++)
                {
                    if (done[i])
                        continue;

                    current = input[i];

                    if (endPoint.IsEqualTo(current.StartPoint, tolerance))
                    {
                        endPoint = current.EndPoint;
                        span[count] = current;
                        found = done[i] = true;
                        break;
                    }
                    else if (endPoint.IsEqualTo(current.EndPoint, tolerance))
                    {
                        endPoint = current.StartPoint;
                        span[count] = current.GetReversedCurve();
                        found = done[i] = true;
                        break;
                    }
                    else if (startPoint.IsEqualTo(current.EndPoint, tolerance))
                    {
                        startPoint = current.StartPoint;
                        span[..count].CopyTo(span[1..]);
                        span[0] = current;
                        found = done[i] = true;
                        break;
                    }
                    else if (startPoint.IsEqualTo(current.StartPoint, tolerance))
                    {
                        startPoint = current.EndPoint;
                        span[..count].CopyTo(span[1..]);
                        span[0] = current.GetReversedCurve();
                        found = done[i] = true;
                        break;
                    }
                }

                if (!found)
                    throw new InvalidOperationException("Non-contiguous curves");

                count++;
            }
            return output;
        }

        /// <summary>
        /// Tries to convert the Curve3d sequence into a CompositeCurve3d.
        /// </summary>
        /// <param name="source">Collection this method applies to.</param>
        /// <param name="compositeCurve">Output composite curve.</param>
        /// <param name="tolerance">Tolerance used to compare end points.</param>
        /// <param name="predicate">Predicate used to filter input curves 3d.</param>
        /// <returns>true, if the composite curve could be created; false otherwise.</returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException is thrown if <paramref name="source"/> is null.</exception>
        public static bool TryConvertToCompositeCurve(
            this IEnumerable<Curve3d> source,
            out CompositeCurve3d? compositeCurve,
            Tolerance tolerance = default,
            Predicate<Curve3d>? predicate = null)
        {
            ArgumentNullException.ThrowIfNull(source);

            compositeCurve = default;

            if (!source.Any())
                return false;

            var isValid = predicate ?? (_ => true);

            if (tolerance.Equals(default(Tolerance)))
                tolerance = Tolerance.Global;

            var input = source as Curve3d[] ?? source.ToArray();

            if (!isValid(input[0]))
                return false;

            int length = input.Length;
            var output = new Curve3d[length];
            Span<Curve3d> span = output;
            var done = new bool[length];
            var current = input[0];
            span[0] = current;
            done[0] = true;
            int count = 1;
            var startPoint = current.StartPoint;
            var endPoint = current.EndPoint;

            while (count < length)
            {
                bool found = false;

                for (int i = 1; i < length; i++)
                {
                    if (done[i])
                        continue;

                    current = input[i];
                    if (!isValid(current))
                        return false;

                    if (endPoint.IsEqualTo(current.StartPoint, tolerance))
                    {
                        endPoint = current.EndPoint;
                        span[count] = current;
                        found = done[i] = true;
                        break;
                    }
                    else if (endPoint.IsEqualTo(current.EndPoint, tolerance))
                    {
                        endPoint = current.StartPoint;
                        span[count] = current.GetReversedCurve();
                        found = done[i] = true;
                        break;
                    }
                    else if (startPoint.IsEqualTo(current.EndPoint, tolerance))
                    {
                        startPoint = current.StartPoint;
                        span[..count].CopyTo(span[1..]);
                        span[0] = current;
                        found = done[i] = true;
                        break;
                    }
                    else if (startPoint.IsEqualTo(current.StartPoint, tolerance))
                    {
                        startPoint = current.EndPoint;
                        span[..count].CopyTo(span[1..]);
                        span[0] = current.GetReversedCurve();
                        found = done[i] = true;
                        break;
                    }
                }

                if (!found)
                    return false;

                count++;
            }
            compositeCurve = new CompositeCurve3d(output);
            return true;
        }
    }
}
