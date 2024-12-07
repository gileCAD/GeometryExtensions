using Autodesk.AutoCAD.Geometry;

namespace Gile.AutoCAD.R25.Geometry
{
    /// <summary>
    /// Provides extension methods for the Curve2d type.
    /// </summary>
    public static class Curve2dExtension
    {
        /// <summary>
        /// Gets the reversed curve without modifying the original one.
        /// </summary>
        /// <param name="curve">Instance this method applies to.</param>
        /// <returns>A new Curve3d with reversed parameters.</returns>
        public static Curve2d GetReversedCurve(this Curve2d curve) =>
            ((Curve2d)curve.Clone()).GetReverseParameterCurve();
    }
}
