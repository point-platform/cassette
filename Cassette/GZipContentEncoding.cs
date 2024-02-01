/*
 * Copyright 2015-2024 Drew Noakes
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

namespace Cassette;

/// <summary>
/// Encodes content using GZip compression.
/// </summary>
public sealed class GZipContentEncoding : IContentEncoding
{
    /// <inheritdoc />
    public string Name => "gzip";

    /// <summary>
    /// Get and set the compression level to use when writing content. Default is <see cref="System.IO.Compression.CompressionLevel.Optimal"/>.
    /// </summary>
    public CompressionLevel CompressionLevel { get; set; }

    /// <summary>
    /// Initialises a new GZip-based content encoding.
    /// </summary>
    public GZipContentEncoding()
    {
        CompressionLevel = CompressionLevel.Optimal;
    }

    /// <inheritdoc />
    public Stream Encode(Stream stream)
    {
        return new GZipStream(stream, CompressionLevel, leaveOpen: true);
    }

    /// <inheritdoc />
    public Stream Decode(Stream stream)
    {
        return new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
    }
}