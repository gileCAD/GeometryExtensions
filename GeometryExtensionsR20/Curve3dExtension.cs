using Autodesk.AutoCAD.Geometry;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gile.AutoCAD.R20.Geometry
{
    /// <summary>
    /// Provides extension methods for the Curve3d type.
    /// </summary>
    public static class Curve3dExtension
    {
        /// <summary>
        /// Order the collection by contiguous curves ([n].EndPoint equals to [n+1].StartPoint).
        /// </summary>
        /// <param name="source">Collection this method applies to.</param>
        /// <param name="tolerance">Tolerance used to compare end points.</param>
        /// <returns>Ordered array of Curve3d.</returns>
        /// <exception cref="ArgumentNullException">ArgumentNullException is thrown if <paramref name="source"/> is null.</exception>
        /// <exception cref="InvalidOperationException">InvalidOperationException is thrown if non-contiguous segments are found.</exception>
        public static Curve3d[] ToOrderedArray(this IEnumerable<Curve3d> source, Tolerance tolerance = default)
        {
            Assert.IsNotNull(source, nameof(source));

            var input = source as Curve3d[] ?? source.ToArray();
            int length = input.Length;
            if (length < 2)
                return input;

            if (tolerance.Equals(default(Tolerance)))
                tolerance = Tolerance.Global;

            var output = new Curve3d[length];
            var done = new bool[length];

            output[0] = input[0];
            done[0] = true;
            int count = 1;
            var startPoint = output[0].StartPoint;
            var endPoint = output[0].EndPoint;

            while (count < length)
            {
                bool found = false;

                for (int i = 0; i < length; i++)
                {
                    if (done[i])
                        continue;

                    var current = input[i];

                    if (endPoint.IsEqualTo(current.StartPoint, tolerance))
                    {
                        output[count] = current;
                        endPoint = current.EndPoint;
                        found = done[i] = true;
                        break;
                    }
                    else if (endPoint.IsEqualTo(current.EndPoint, tolerance))
                    {
                        output[count] = current.GetReverseParameterCurve();
                        endPoint = current.StartPoint;
                        found = done[i] = true;
                        break;
                    }
                    else if (startPoint.IsEqualTo(current.EndPoint, tolerance))
                    {
                        for (int j = count; j > 0; j--)
                            output[j] = output[j - 1];
                        output[0] = current;
                        startPoint = current.StartPoint;
                        found = done[i] = true;
                        break;
                    }
                    else if (startPoint.IsEqualTo(current.StartPoint, tolerance))
                    {
                        for (int j = count; j > 0; j--)
                            output[j] = output[j - 1];
                        output[0] = current.GetReverseParameterCurve();
                        startPoint = current.EndPoint;
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
            out CompositeCurve3d compositeCurve,
            Tolerance tolerance = default,
            Predicate<Curve3d> predicate = null)
        {
            Assert.IsNotNull(source, nameof(source));

            var isValid = predicate ?? (_ => true);

            if (tolerance.Equals(default(Tolerance)))
                tolerance = Tolerance.Global;

            compositeCurve = default;

            var input = source as Curve3d[] ?? source.ToArray();

            if (!isValid(input[0]))
                return false;

            int length = input.Length;
            if (length < 2)
            {
                compositeCurve = new CompositeCurve3d(new[] { input[0] });
                return true;
            }

            var output = new Curve3d[length];
            var done = new bool[length];

            output[0] = input[0];
            done[0] = true;
            int count = 1;
            var startPoint = output[0].StartPoint;
            var endPoint = output[0].EndPoint;

            while (count < length)
            {
                bool found = false;

                for (int i = 0; i < length; i++)
                {
                    if (done[i])
                        continue;

                    var current = input[i];
                    if (!isValid(current))
                        return false;

                    if (endPoint.IsEqualTo(current.StartPoint, tolerance))
                    {
                        output[count] = current;
                        endPoint = current.EndPoint;
                        found = done[i] = true;
                        break;
                    }
                    else if (endPoint.IsEqualTo(current.EndPoint, tolerance))
                    {
                        output[count] = current.GetReverseParameterCurve();
                        endPoint = current.StartPoint;
                        found = done[i] = true;
                        break;
                    }
                    else if (startPoint.IsEqualTo(current.EndPoint, tolerance))
                    {
                        for (int j = count; j > 0; j--)
                            output[j] = output[j - 1];
                        output[0] = current;
                        startPoint = current.StartPoint;
                        found = done[i] = true;
                        break;
                    }
                    else if (startPoint.IsEqualTo(current.StartPoint, tolerance))
                    {
                        for (int j = count; j > 0; j--)
                            output[j] = output[j - 1];
                        output[0] = current.GetReverseParameterCurve();
                        startPoint = current.EndPoint;
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
