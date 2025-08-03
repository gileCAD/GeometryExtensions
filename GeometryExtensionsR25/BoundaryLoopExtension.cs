using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.Geometry;

using System;
using System.Collections.Generic;

namespace Gile.AutoCAD.R25.Geometry
{
    /// <summary>
    /// Provides extension methods for the BoundaryLoop type.
    /// </summary>
    public static class BoundaryLoopExtension
    {
        /// <summary>
        /// Gets the native curves constituting the loop (implicitly disposes of the ExternalCurve3d instances).
        /// </summary>
        /// <param name="loop">Boundary Represention loop.</param>
        /// <returns></returns>
        public static IEnumerable<Curve3d> GetNativeCurves(this BoundaryLoop loop)
        {
            ArgumentNullException.ThrowIfNull(loop);

            foreach (var edge in loop.Edges)
            {
                using var externalCurve = (ExternalCurve3d)edge.Curve;
                yield return externalCurve.NativeCurve;
            }
        }
    }
}
