namespace Gile.AutoCAD.R19.Geometry
{
    /// <summary>
    /// Describes a generic tree node.
    /// </summary>
    /// <typeparam name="T">Type of the node Value.</typeparam>
    public class TreeNode<T>
    {
        /// <summary>
        /// Creates a new instance of TreeNode.
        /// </summary>
        /// <param name="value">Value of the node.</param>
        /// <param name="depth">Depth of the node.</param>
        public TreeNode(T value, int depth)
        {
            Value = value;
            Depth = depth;
        }

        /// <summary>
        /// Gets the depth of the node in tree.
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Gets the left child node.
        /// </summary>
        public TreeNode<T> LeftChild { get; set; }

        /// <summary>
        /// Gets the right child node.
        /// </summary>
        public TreeNode<T> RightChild { get; set; }

        /// <summary>
        /// Gets the value of the node.
        /// </summary>
        public T Value { get; }
    }
}
