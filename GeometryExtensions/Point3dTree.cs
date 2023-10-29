using Autodesk.AutoCAD.Geometry;

using System.Collections.Generic;

namespace Gile.AutoCAD.Geometry
{
    /// <summary>
    /// Provides methods to organize 3d points in a Kd tree structure to speed up the search of neighbours.
    /// A boolean constructor parameter (ignoreZ) indicates if the resulting Kd tree is a 3d tree or a 2d tree.
    /// Use ignoreZ = true if all points in the input collection lie on a plane parallel to XY 
    /// or if the points have to be considered as projected on the XY plane. 
    /// </summary>
    public class Point3dTree : KdTree<Point3d>
    {
        /// <summary>
        /// Creates an new instance of Point3dTree.
        /// </summary>
        /// <param name="points">The Point3d collection to fill the tree.</param>
        /// <param name="ignoreZ">A value indicating if the Z coordinate of points is ignored 
        /// (as if all points were projected to the XY plane).</param>
        public Point3dTree(IEnumerable<Point3d> points, bool ignoreZ = false)
            : base(points, (pt) => pt, ignoreZ ? 2 : 3)
        { }
    }
}
