﻿using Autodesk.AutoCAD.Geometry;

namespace Gile.AutoCAD.R20.Geometry
{
    /// <summary>
    /// Provides extension methods for the Vector2d type.
    /// </summary>
    public static class Vector3dExtension
    {
        /// <summary>
        /// Projects the point on the WCS XY plane.
        /// </summary>
        /// <param name="vector">The instance to which this method applies.</param>
        /// <returns>he projected vector.</returns>
        public static Vector3d Flatten(this Vector3d vector) => new Vector3d(vector.X, vector.Y, 0.0);
    }
}
