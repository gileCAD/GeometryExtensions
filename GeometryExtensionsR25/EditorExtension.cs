using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

using AcRx = Autodesk.AutoCAD.Runtime;

namespace Gile.AutoCAD.R25.Geometry
{
    /// <summary>
    /// Provides extension methods for the Editor type. 
    /// </summary>
    public static class EditorExtension
    {
        /// <summary>
        /// Gets the transformation matrix from the current User Coordinate System (UCS) to the World Coordinate System (WCS). 
        /// </summary>
        /// <param name="ed">The instance to which this method applies.</param>
        /// <returns>The UCS to WCS transformation matrix.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentNullException is thrown if <paramref name="ed"/> is null.</exception>
        public static Matrix3d UCS2WCS(this Editor ed)
        {
            System.ArgumentNullException.ThrowIfNull(ed);
            return ed.CurrentUserCoordinateSystem;
        }

        /// <summary>
        /// Gets the transformation matrix from the World Coordinate System (WCS) to the current User Coordinate System (UCS). 
        /// </summary>
        /// <param name="ed">The instance to which this method applies.</param>
        /// <returns>The WCS to UCS transformation matrix.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentNullException is thrown if <paramref name="ed"/> is null.</exception>
        public static Matrix3d WCS2UCS(this Editor ed)
        {
            System.ArgumentNullException.ThrowIfNull(ed);
            return ed.CurrentUserCoordinateSystem.Inverse();
        }

        /// <summary>
        /// Gets the transformation matrix from the current viewport Display Coordinate System (DCS) to the World Coordinate System (WCS). 
        /// </summary>
        /// <param name="ed">The instance to which this method applies.</param>
        /// <returns>The DCS to WCS transformation matrix.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentNullException is thrown if <paramref name="ed"/> is null.</exception>
        public static Matrix3d DCS2WCS(this Editor ed)
        {
            System.ArgumentNullException.ThrowIfNull(ed);
            Matrix3d retVal = new();
            bool tilemode = ed.Document.Database.TileMode;
            if (!tilemode)
                ed.SwitchToModelSpace();
            using (ViewTableRecord vtr = ed.GetCurrentView())
            {
                retVal =
                    Matrix3d.Rotation(-vtr.ViewTwist, vtr.ViewDirection, vtr.Target) *
                    Matrix3d.Displacement(vtr.Target - Point3d.Origin) *
                    Matrix3d.PlaneToWorld(vtr.ViewDirection);
            }
            if (!tilemode)
                ed.SwitchToPaperSpace();
            return retVal;
        }

        /// <summary>
        /// Gets the transformation matrix from the World Coordinate System (WCS) to the current viewport Display Coordinate System (DCS). 
        /// </summary>
        /// <param name="ed">The instance to which this method applies.</param>
        /// <returns>The WCS to DCS transformation matrix.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentNullException is thrown if <paramref name="ed"/> is null.</exception>
        public static Matrix3d WCS2DCS(this Editor ed)
        {
            System.ArgumentNullException.ThrowIfNull(ed);
            return ed.DCS2WCS().Inverse();
        }

        /// <summary>
        ///  Gets the transformation matrix from the paper space active viewport Display Coordinate System (DCS) to the Paper space Display Coordinate System (PSDCS). 
        /// </summary>
        /// <param name="ed">The instance to which this method applies.</param>
        /// <returns>The DCS to PSDCS transformation matrix.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentNullException is thrown if <paramref name="ed"/> is null.</exception>
        /// <exception cref=" Autodesk.AutoCAD.Runtime.Exception">
        /// eNotInPaperSpace is thrown if this method is called form Model Space.</exception>
        /// <exception cref=" Autodesk.AutoCAD.Runtime.Exception">
        /// eCannotChangeActiveViewport is thrown if there is none floating viewport in the current layout.</exception>
        public static Matrix3d DCS2PSDCS(this Editor ed)
        {
            System.ArgumentNullException.ThrowIfNull(ed);
            Database db = ed.Document.Database;
            if (db.TileMode)
                throw new AcRx.Exception(AcRx.ErrorStatus.NotInPaperspace);
            using Transaction tr = db.TransactionManager.StartTransaction();
            Viewport vp =
                (Viewport)tr.GetObject(ed.CurrentViewportObjectId, OpenMode.ForRead);
            if (vp.Number == 1)
            {
                try
                {
                    ed.SwitchToModelSpace();
                    vp = (Viewport)tr.GetObject(ed.CurrentViewportObjectId, OpenMode.ForRead);
                    ed.SwitchToPaperSpace();
                }
                catch
                {
                    throw new AcRx.Exception(AcRx.ErrorStatus.CannotChangeActiveViewport);
                }
            }
            return vp.DCS2PSDCS();
        }

        /// <summary>
        ///  Gets the transformation matrix from the Paper space Display Coordinate System (PSDCS) to the paper space active viewport Display Coordinate System (DCS). 
        /// </summary>
        /// <param name="ed">The instance to which this method applies.</param>
        /// <returns>The PSDCS to DCS transformation matrix.</returns>
        /// <exception cref="System.ArgumentNullException">ArgumentNullException is thrown if <paramref name="ed"/> is null.</exception>
        /// <exception cref=" Autodesk.AutoCAD.Runtime.Exception">
        /// eNotInPaperSpace is thrown if this method is called form Model Space.</exception>
        /// <exception cref=" Autodesk.AutoCAD.Runtime.Exception">
        /// eCannotChangeActiveViewport is thrown if there is none floating viewport in the current layout.</exception>
        public static Matrix3d PSDCS2DCS(this Editor ed)
        {
            System.ArgumentNullException.ThrowIfNull(ed);
            return ed.DCS2PSDCS().Inverse();
        }
    }
}
