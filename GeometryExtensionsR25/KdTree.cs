using Autodesk.AutoCAD.Geometry;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static System.Math;

namespace Gile.AutoCAD.R25.Geometry
{
    /// <summary>
    /// Provides methods to organize locatable objects in a Kd tree structure to speed up the search of neighbours.
    /// The 'dimension' parameter indicates if the resulting Kd tree is a 3d tree or a 2d tree.
    /// Use dimension = 2 if all objects in the input collection lie on a plane parallel to XY
    /// or if the objects have to be considered as projected on the XY plane.
    /// </summary>
    /// <typeparam name="T"> Type of the source items.</typeparam>

    public class KdTree<T>
    {
        #region Private fields

        private readonly int dimension;
        private readonly int parallelDepth;
        private readonly Func<Point3d, Point3d, double> sqrDist;
        private readonly Func<T, Point3d> getPosition;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of KdTree.
        /// </summary>
        /// <param name="source">The collection of objects to fill the tree.</param>
        /// <param name="getPosition">A function which returns the position of the object.</param>
        /// <param name="dimension">The dimension of the tree (2 or 3)</param>
        /// <exception cref="ArgumentNullException">Thrown if source is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if dimension is lower than2 or greater than 3.</exception>
        public KdTree(IEnumerable<T>? source, Func<T, Point3d>? getPosition, int dimension)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(getPosition);
            if (dimension < 2 || 3 < dimension)
                throw new ArgumentOutOfRangeException(nameof(dimension));
            this.getPosition = getPosition;
            this.dimension = dimension;
            if (dimension == 2)
                sqrDist = SqrDistance2d;
            else
                sqrDist = SqrDistance3d;
            int numProc = Environment.ProcessorCount;
            parallelDepth = -1;
            while (numProc >> ++parallelDepth > 1) ;
            Root = Create(source.ToArray(), 0) ?? throw new InvalidOperationException("Empty source");
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the root node of the tree.
        /// </summary>
        public TreeNode<T> Root { get; private set; }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets the nearest neighbour.
        /// </summary>
        /// <param name = "point" > The point from which search the nearest neighbour.</param>
        /// <returns> The nearest object in the collection from the specified point.</returns>
        public T GetNearestNeighbour(Point3d point)
        {
            return GetNeighbour(point, Root, Root.Value, double.MaxValue);
        }

        /// <summary>
        /// Gets the neighbours within the specified distance.
        /// </summary>
        /// <param name = "point" > The point from which search the nearest neighbours.</param>
        /// <param name = "radius" > The distance in which collect the neighbours.</param>
        /// <returns> The objects which distance from the specified point is less or equal to the specified distance.</returns>
        public List<T> GetNearestNeighbours(Point3d point, double radius)
        {
            var items = new List<T>();
            GetNeighboursAtDistance(point, radius * radius, Root, items);
            return items;
        }

        /// <summary>
        /// Gets the given number of nearest neighbours.
        /// </summary>
        /// <param name = "point" > The point from which search the nearest neighbours.</param>
        /// <param name = "number" > The number of objects to collect.</param>
        /// <returns>The n nearest neighbours of the specified point.</returns>
        public List<T> GetNearestNeighbours(Point3d point, int number)
        {
            var items = new List<(double, T Instance)>();
            GetKNeighbours(point, number, Root, items);
            return items.ConvertAll(x => x.Instance);
        }

        /// <summary>
        /// Gets the objects in a range.
        /// </summary>
        /// <param name = "pt1" > The first corner of range.</param>
        /// <param name = "pt2" > The opposite corner of the range.</param>
        /// <returns> All objects within the range.</returns>
        public List<T> GetBoxedRange(Point3d pt1, Point3d pt2)
        {
            var lowerLeft = new Point3d(
                Min(pt1.X, pt2.X), Min(pt1.Y, pt2.Y), Min(pt1.Z, pt2.Z));
            var upperRight = new Point3d(
                Max(pt1.X, pt2.X), Max(pt1.Y, pt2.Y), Max(pt1.Z, pt2.Z));
            var items = new List<T>();
            FindRange(lowerLeft, upperRight, Root, items);
            return items;
        }

        #endregion

        #region Private methods

        private TreeNode<T>? Create(T[] items, int depth)
        {
            int length = items.Length;
            if (length == 0) return null;
            int d = depth % dimension;
            var median = KdTree<T>.SelectMedian(items, (x, y) => getPosition(x)[d].CompareTo(getPosition(y)[d]));
            var node = new TreeNode<T>(median, depth);
            int mid = length / 2;
            int rlen = length - mid - 1;
            var left = new T[mid];
            var right = new T[rlen];
            Array.Copy(items, 0, left, 0, mid);
            Array.Copy(items, mid + 1, right, 0, rlen);
            if (depth < parallelDepth)
            {
                Parallel.Invoke(
                   () => node.LeftChild = Create(left, depth + 1),
                   () => node.RightChild = Create(right, depth + 1)
                );
            }
            else
            {
                node.LeftChild = Create(left, depth + 1);
                node.RightChild = Create(right, depth + 1);
            }
            return node;
        }

        private T GetNeighbour(Point3d center, TreeNode<T>? node, T currentBest, double bestDist)
        {
            if (node == null)
                return currentBest;
            var current = node.Value;
            var currentPosition = getPosition(current);
            int d = node.Depth % dimension;
            double coordCen = center[d];
            double coordCur = currentPosition[d];
            double dist = sqrDist(center, currentPosition);
            if (dist >= 0.0 && dist < bestDist)
            {
                currentBest = current;
                bestDist = dist;
            }
            dist = coordCen - coordCur;
            if (bestDist < dist * dist)
            {
                currentBest = GetNeighbour(
                    center, coordCen < coordCur ? node.LeftChild : node.RightChild, currentBest, bestDist);
            }
            else
            {
                currentBest = GetNeighbour(center, node.LeftChild, currentBest, bestDist);
                bestDist = sqrDist(center, getPosition(currentBest));
                currentBest = GetNeighbour(center, node.RightChild, currentBest, bestDist);
            }
            return currentBest;
        }

        private void GetNeighboursAtDistance(Point3d center, double radius, TreeNode<T>? node, List<T> items)
        {
            if (node == null) return;
            var current = node.Value;
            var currentPosition = getPosition(current);
            double dist = sqrDist(center, currentPosition);
            if (dist <= radius)
            {
                items.Add(current);
            }
            int d = node.Depth % dimension;
            double coordCen = center[d];
            double coordCur = currentPosition[d];
            dist = coordCen - coordCur;
            if (dist * dist > radius)
            {
                if (coordCen < coordCur)
                {
                    GetNeighboursAtDistance(center, radius, node.LeftChild, items);
                }
                else
                {
                    GetNeighboursAtDistance(center, radius, node.RightChild, items);
                }
            }
            else
            {
                GetNeighboursAtDistance(center, radius, node.LeftChild, items);
                GetNeighboursAtDistance(center, radius, node.RightChild, items);
            }
        }

        private void GetKNeighbours(Point3d center, int number, TreeNode<T>? node, List<(double Distance, T)> items)
        {
            if (node == null) return;
            T current = node.Value;
            Point3d currentPosition = getPosition(current);
            double dist = sqrDist(center, currentPosition);
            int cnt = items.Count;
            if (cnt == 0)
            {
                items.Add((dist, current));
            }
            else if (cnt < number)
            {
                if (dist > items[0].Distance)
                {
                    items.Insert(0, (dist, current));
                }
                else
                {
                    items.Add((dist, current));
                }
            }
            else if (dist < items[0].Distance)
            {
                items[0] = (dist, current);
                items.Sort((x, y) => y.Distance.CompareTo(x.Distance));
            }
            int d = node.Depth % dimension;
            double coordCen = center[d];
            double coordCur = currentPosition[d];
            dist = coordCen - coordCur;
            if (dist * dist > items[0].Distance)
            {
                if (coordCen < coordCur)
                {
                    GetKNeighbours(center, number, node.LeftChild, items);
                }
                else
                {
                    GetKNeighbours(center, number, node.RightChild, items);
                }
            }
            else
            {
                GetKNeighbours(center, number, node.LeftChild, items);
                GetKNeighbours(center, number, node.RightChild, items);
            }
        }

        private void FindRange(Point3d lowerLeft, Point3d upperRight, TreeNode<T>? node, List<T> items)
        {
            if (node == null)
                return;
            T current = node.Value;
            Point3d currentPosition = getPosition(current);
            if (dimension == 2)
            {
                if (lowerLeft.X <= currentPosition.X && currentPosition.X <= upperRight.X &&
                    lowerLeft.Y <= currentPosition.Y && currentPosition.Y <= upperRight.Y)
                    items.Add(current);
            }
            else
            {
                if (lowerLeft.X <= currentPosition.X && currentPosition.X <= upperRight.X &&
                    lowerLeft.Y <= currentPosition.Y && currentPosition.Y <= upperRight.Y &&
                    lowerLeft.Z <= currentPosition.Z && currentPosition.Z <= upperRight.Z)
                    items.Add(current);
            }
            int d = node.Depth % dimension;
            if (upperRight[d] < currentPosition[d])
                FindRange(lowerLeft, upperRight, node.LeftChild, items);
            else if (lowerLeft[d] > currentPosition[d])
                FindRange(lowerLeft, upperRight, node.RightChild, items);
            else
            {
                FindRange(lowerLeft, upperRight, node.LeftChild, items);
                FindRange(lowerLeft, upperRight, node.RightChild, items);
            }
        }

        private double SqrDistance2d(Point3d p1, Point3d p2)
        {
            return
                (p1.X - p2.X) * (p1.X - p2.X) +
                (p1.Y - p2.Y) * (p1.Y - p2.Y);
        }

        private double SqrDistance3d(Point3d p1, Point3d p2)
        {
            return
                (p1.X - p2.X) * (p1.X - p2.X) +
                (p1.Y - p2.Y) * (p1.Y - p2.Y) +
                (p1.Z - p2.Z) * (p1.Z - p2.Z);
        }

        //Credit: Tony Tanzillo
        // http://www.theswamp.org/index.php?topic=44312.msg495808#msg495808
        private static T SelectMedian(T[] items, Comparison<T> compare)
        {
            int l = items.Length;
            int k = l / 2;
            if (items == null || l == 0)
                throw new ArgumentException("array");
            int from = 0;
            int to = l - 1;
            while (from < to)
            {
                int r = from;
                int w = to;
                var current = items[(r + w) / 2];
                while (r < w)
                {
                    if (-1 < compare(items[r], current))
                    {
                        (items[r], items[w]) = (items[w], items[r]);
                        w--;
                    }
                    else
                    {
                        r++;
                    }
                }
                if (0 < compare(items[r], current))
                {
                    r--;
                }
                if (k <= r)
                {
                    to = r;
                }
                else
                {
                    from = r + 1;
                }
            }
            return items[k];
        }

        #endregion
    }
}