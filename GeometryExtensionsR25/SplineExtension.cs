namespace Gile.AutoCAD.Geometry
{
    /// <summary>
    /// Provides extension methods for the Spline type.
    /// </summary>
    public static class SplineExtension
    {
        /// <summary>
        /// Gets the centroid of a closed planar spline.
        /// </summary>
        /// <param name="spline">The instance to which this method applies.</param>
        /// <returns>The centroid of the spline (WCS coordinates).</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentException is thrown if <paramref name="spline"/> is null.</exception>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">eNonPlanarEntity is thrown if the spline is not planar.</exception>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">eNotApplicable is thrown if the spline is not closed.</exception>
        public static Point3d Centroid(this Spline spline)
        {
            Assert.IsNotNull(spline, nameof(spline));
            if (!spline.IsPlanar)
                throw new AcRx.Exception(AcRx.ErrorStatus.NonPlanarEntity);
            if (spline.Closed != true)
                throw new AcRx.Exception(AcRx.ErrorStatus.NotApplicable);
            using DBObjectCollection curves = new();
            curves.Add(spline);
            using DBObjectCollection dboc = Region.CreateFromCurves(curves);
            return ((Region)dboc[0]).Centroid();
        }
    }
}
