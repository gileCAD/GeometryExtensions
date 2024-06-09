using System.Collections;
using System.Runtime.CompilerServices;

namespace Gile.AutoCAD.Geometry
{
    /// <summary>
    /// Provides methods to throw an exception if an assertion is wrong.
    /// </summary>
    internal static class Assert
    {
        /// <summary>
        /// Throws ArgumentNullException if the object is null.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="obj">The instance to which the assertion applies.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <exception cref="System.ArgumentNullException">Throws ArgumentNullException if <paramref name="obj"/> is null.</exception>
        /// <remarks>This method is not available for AutoCAD 2025+ (replaced by System.ArgumentNullException.ThrowIfNull).</remarks>
        public static void IsNotNull<T>(T obj, [CallerMemberName] string? paramName = null) where T : class
        {
            if (obj == null)
                throw new ArgumentNullException(paramName ?? nameof(obj));
        }

        /// <summary>
        /// Throws ArgumentException if the sequence is empty.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="obj">The instance to which the assertion applies.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <exception cref="System.ArgumentException">Throws ArgumentException if <paramref name="obj"/> is an empty sequence.</exception>
        public static void IsNotEmpty<T>(T obj, [CallerMemberName] string? paramName = null) where T : IEnumerable
        {
            if (!obj.GetEnumerator().MoveNext())
                throw new ArgumentException("Empty sequence", paramName ?? nameof(obj));
        }
    }
}
