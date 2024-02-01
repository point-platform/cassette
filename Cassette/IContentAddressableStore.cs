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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cassette;

/// <summary>
/// Defines a content-addressable store.
/// </summary>
public interface IContentAddressableStore
{
    /// <summary>
    /// Write content to the store, returning its hash.
    /// </summary>
    /// <remarks>
    /// If the store already contains this content, the write is discarded but the hash is
    /// returned, as normal.
    /// <para />
    /// When one or more encodings are provided, the store will compute and persist encoded
    /// versions of the content as well.
    /// <para />
    /// If the store already contains the content, but not in a specified encoding, then
    /// a correspondingly encoded version will be created and persisted.
    /// </remarks>
    /// <param name="stream">A stream from which the data to be written can be read.</param>
    /// <param name="cancellationToken">An optional cancellation token which may be used to cancel the asynchronous write operation.</param>
    /// <param name="encodings">A sequence of encodings to also store this content with. If <see langword="null"/> or empty, no additional encodings are used.</param>
    /// <returns>An async task, the result of which is the written content's hash.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <see langword="null"/>.</exception>
    Task<Hash> WriteAsync(Stream stream, CancellationToken cancellationToken = new CancellationToken(), IEnumerable<IContentEncoding>? encodings = null);

    /// <summary>
    /// Gets a value indicating whether content exists in the store with the specified <paramref name="hash"/>.
    /// </summary>
    /// <param name="hash">The hash of the content to search for.</param>
    /// <param name="encodingName">Conditions the check on whether the content exists with the specified encoding. If <see langword="null"/>, the check indicates whether the unencoded content exists in the store.</param>
    /// <returns><see langword="true"/> if the content exists in the store, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="hash"/> is <see langword="null"/>.</exception>
    bool Contains(Hash hash, string? encodingName = null);

    /// <summary>
    /// Read content from the store.
    /// </summary>
    /// <remarks>
    /// When <see langword="true"/> is returned, <paramref name="stream"/> will be non-<see langword="null"/> and
    /// must be disposed when finished with.
    /// <para />
    /// If <paramref name="encodingName"/> is non-<see langword="null"/> and the content was not written
    /// explicitly using the encoding, this method will return <see langword="false"/>.
    /// </remarks>
    /// <param name="hash">The hash of the content to read.</param>
    /// <param name="stream">A stream from which the stored content may be read.</param>
    /// <param name="options">Optional parameters to control how data be read from disk.
    /// See the <see cref="ReadOptions"/> enum for further details.</param>
    /// <param name="encodingName">The encoding used when storing the content, or <see langword="null"/> to access the unencoded content.</param>
    /// <returns><see langword="true"/> if the content was found, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="hash"/> is <see langword="null"/>.</exception>
    bool TryRead(Hash hash, [NotNullWhen(returnValue: true)] out Stream? stream, ReadOptions options = ReadOptions.None, string? encodingName = null);

    /// <summary>
    /// Get the length of stored content.
    /// </summary>
    /// <param name="hash">The hash of the content to measure.</param>
    /// <param name="length">The length of the content in bytes.</param>
    /// <param name="encodingName">The encoding used when storing the content, or <see langword="null"/> to access the unencoded content.</param>
    /// <returns><see langword="true"/> if the requested content exists, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="hash"/> is <see langword="null"/>.</exception>
    bool TryGetContentLength(Hash hash, out long length, string? encodingName = null);

    /// <summary>
    /// Get an enumeration over all hashes contained within the store.
    /// </summary>
    /// <remarks>
    /// This enumeration is computed lazily by querying the file system and therefore will
    /// not behave deterministically if modified while enumerating.
    /// </remarks>
    /// <returns>An enumeration over all hashes contained within the store.</returns>
    IEnumerable<Hash> GetHashes();

    /// <summary>
    /// Attempt to delete an item of content.
    /// </summary>
    /// <remarks>
    /// All encodings of the content will be deleted.
    /// </remarks>
    /// <param name="hash">The hash of the content to delete.</param>
    /// <returns><see langword="true"/> if the content existed and was deleted, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="hash"/> is <see langword="null"/>.</exception>
    bool Delete(Hash hash);
}