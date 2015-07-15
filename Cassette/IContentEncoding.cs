using System.IO;

namespace Cassette
{
    /// <summary>
    /// Defines an encoding for use with <see cref="IContentAddressableStore"/>.
    /// </summary>
    /// <remarks>
    /// Such encodings allow data to be stored in a different format and later retrieved in that
    /// format directly.
    /// </remarks>
    public interface IContentEncoding
    {
        /// <summary>Gets the name given to the type of encoding.</summary>
        string Name { get; }

        /// <summary>
        /// Encodes <paramref name="stream"/>, returning a new stream adhering to the type of encoding.
        /// </summary>
        /// <param name="stream">The stream to encode.</param>
        /// <returns>An encoded stream.</returns>
        Stream Encode(Stream stream);

        /// <summary>
        /// Decodes <paramref name="stream"/>, returning a new stream adhering to the type of encoding.
        /// </summary>
        /// <param name="stream">The stream to decode.</param>
        /// <returns>A decoded stream.</returns>
        Stream Decode(Stream stream);
    }
}