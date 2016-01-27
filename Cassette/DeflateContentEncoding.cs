/*
 * Copyright 2015-2016 Drew Noakes
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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