using System.Collections;

namespace Gile.AutoCAD.R25.Geometry
{
    /// <summary>
    /// Provides methods to throw an exception if an assertion is wrong.
    /// </summary>
    internal static class Assert
    {
        /// <summary>
        /// Throws ArgumentException if the sequence is empty.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="obj">The instance to which the assertion applies.</param>
        /// <param name="paramName">Name of the parameter.</param>
        /// <exception cref="System.ArgumentException">Throws ArgumentException if <paramref name="obj"/> is an empty sequence.</exception>
        public static void IsNotEmpty<T>(T obj, string paramName) where T: IEnumerable
        {
            if (!((IEnumerable)obj).GetEnumerator().MoveNext())
                throw new System.ArgumentException("Empty sequence", paramName);
        }
    }
}
