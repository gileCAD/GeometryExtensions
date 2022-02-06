using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Gile.AutoCAD.Geometry
{
    /// <summary>
    /// Provides extension methods for the Viewport type.
    /// </summary>
    public static class ViewportExtension
    {
        /// <summary>
        /// Gets the transformation matrix of the display coordinate system (DCS)
        /// of the specified window to the world coordinate system (WCS).
        /// </summary>
        /// <param name="vp">The instance to which this method applies.</param>
        /// <returns>The transformation matrix from DCS to WCS.</returns>
        public static Matrix3d DCS2WCS(this Viewport vp)
        {
            return
                Matrix3d.Rotation(-vp.TwistAngle, vp.ViewDirection, vp.ViewTarget) *
                Matrix3d.Displacement(vp.ViewTarget.GetAsVector()) *
                Matrix3d.PlaneToWorld(vp.ViewDirection);
        }

        /// <summary>
        /// Gets the transformation matrix of the world coordinate system (WCS)
        /// to the display coordinate system (DCS) of the specified window.
        /// </summary>
        /// <param name="vp">The instance to which this method applies.</param>
        /// <returns>The transformation matrix from WCS to DCS.</returns>
        public static Matrix3d WCS2DCS(this Viewport vp)
        {
            return
                Matrix3d.WorldToPlane(vp.ViewDirection) *
                Matrix3d.Displacement(vp.ViewTarget.GetAsVector().Negate()) *
                Matrix3d.Rotation(vp.TwistAngle, vp.ViewDirection, vp.ViewTarget);
        }

        /// <summary>
        /// Gets the transformation matrix of the display coordinate system of the specified paper space window (DCS)
        /// to the paper space display coordinate system (PSDCS).
        /// </summary>
        /// <param name="vp">The instance to which this method applies.</param>
        /// <returns>The transformation matrix from DCS to PSDCS.</returns>
        public static Matrix3d DCS2PSDCS(this Viewport vp)
        {
            return
                Matrix3d.Scaling(vp.CustomScale, vp.CenterPoint) *
                Matrix3d.Displacement(vp.ViewCenter.Convert3d().GetVectorTo(vp.CenterPoint));
        }

        /// <summary>
        /// Gets the transformation matrix of the paper space display coordinate system (PSDCS)
        /// to the display coordinate system of the specified paper space window (DCS). 
        /// </summary>
        /// <param name="vp">The instance to which this method applies.</param>
        /// <returns>The transformation matrix from PSDCS to DCS.</returns>
        public static Matrix3d PSDCS2DCS(this Viewport vp)
        {
            return
                Matrix3d.Displacement(vp.CenterPoint.GetVectorTo(vp.ViewCenter.Convert3d())) *
                Matrix3d.Scaling(1.0 / vp.CustomScale, vp.CenterPoint);
        }
    }
}
