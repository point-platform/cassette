using System.IO;
using System.IO.Compression;

namespace Cassette
{
    /// <summary>
    /// Encodes content using GZip compression.
    /// </summary>
    public sealed class GZipContentEncoding : IContentEncoding
    {
        public string Name { get { return "gzip"; } }

        /// <summary>
        /// Get and set the compression level to use when writing content.
        /// </summary>
        public CompressionLevel CompressionLevel { get; set; }

        public GZipContentEncoding()
        {
            CompressionLevel = CompressionLevel.Optimal;
        }

        public Stream Encode(Stream stream)
        {
            return new GZipStream(stream, CompressionLevel, leaveOpen: true);
        }

        public Stream Decode(Stream stream)
        {
            return new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
        }
    }
}