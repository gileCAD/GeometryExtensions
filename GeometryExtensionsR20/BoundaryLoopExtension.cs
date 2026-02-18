using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using System.Collections.Generic;
using System.Linq;

namespace Gile.AutoCAD.R20.Geometry
{
    /// <summary>
    /// Provides extension methods for the BoundaryLoop type.
    /// </summary>
    public static class BoundaryLoopExtension
    {
        /// <summary>
        /// Gets the native curves constituting the loop (implicitly disposes of the ExternalCurve3d instances).
        /// </summary>
        /// <param name="loop">Brep loop.</param>
        /// <returns>The native curves constituting the loop.</returns>
        public static IEnumerable<Curve3d> GetNativeCurves(this BoundaryLoop loop)
        {
            Assert.IsNotNull(loop, nameof(loop));

            foreach (var edge in loop.Edges)
            {
                using (var externalCurve = (ExternalCurve3d)edge.Curve)
                {
                    yield return externalCurve.NativeCurve;
                }
            }
        }

        /// <summary>
        /// Gets the curves constituting the boundary loop.
        /// </summary>
        /// <param name="loop">The instance to which this method applies.</param>
        /// <param name="tolerance">Tolerance used in curve end points comparison.</param>
        /// <returns>Curve collection.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="loop"/> is null.</exception>
        public static IEnumerable<Curve> GetCurves(this BoundaryLoop loop, Tolerance tolerance = default)
        {
            Assert.IsNotNull(loop, nameof(loop));

            if (tolerance.Equals(default(Tolerance)))
                tolerance = Tolerance.Global;

            var curves3d = loop.GetNativeCurves().ToArray();
            if (curves3d.Length == 1)
            {
                yield return Curve.CreateFromGeCurve(curves3d[0]);
            }
            else if (curves3d.TryConvertToCompositeCurve(out CompositeCurve3d compositeCurve, tolerance, c => c is LineSegment3d || c is CircularArc3d))
            {
                yield return (Polyline)Curve.CreateFromGeCurve(compositeCurve);
            }
            else
            {
                foreach (Curve3d curve3d in curves3d)
                {
                    yield return Curve.CreateFromGeCurve(curve3d);
                }
            }
        }
    }
}
