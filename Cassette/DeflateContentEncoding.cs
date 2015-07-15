using System.IO;
using System.IO.Compression;

namespace Cassette
{
    /// <summary>
    /// Encodes content using deflate compression.
    /// </summary>
    public sealed class DeflateContentEncoding : IContentEncoding
    {
        public string Name { get { return "deflate"; } }

        /// <summary>
        /// Get and set the compression level to use when writing content.
        /// </summary>
        public CompressionLevel CompressionLevel { get; set; }

        public DeflateContentEncoding()
        {
            CompressionLevel = CompressionLevel.Optimal;
        }

        public Stream Encode(Stream stream)
        {
            return new DeflateStream(stream, CompressionLevel, leaveOpen: true);
        }

        public Stream Decode(Stream stream)
        {
            return new DeflateStream(stream, CompressionMode.Decompress, leaveOpen: true);
        }
    }
}