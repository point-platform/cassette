/*
 * Copyright 2015 Drew Noakes
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cassette
{
    /// <summary>
    /// Defines a content-addressable store.
    /// </summary>
    public interface IContentAddressableStore
    {
        /// <summary>
        /// Write data to the store.
        /// </summary>
        /// <param name="stream">A stream from which the data to be written can be read.</param>
        /// <param name="cancellationToken">An optional cancellation token which may be used to cancel the asynchronous write operation.</param>
        /// <returns>An async task, the result of which is the written content's hash.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
        Task<byte[]> WriteAsync(Stream stream, CancellationToken cancellationToken = new CancellationToken());

        /// <summary>
        /// Gets a value indicating whether an object exists in the store with the specified <paramref name="hash"/>.
        /// </summary>
        /// <param name="hash">The hash of the content to search for.</param>
        /// <returns><c>true</c> if the content exists in the store, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="hash"/> is <c>null</c>.</exception>
        bool Contains(byte[] hash);

        /// <summary>
        /// Read data from the store.
        /// </summary>
        /// <remarks>
        /// When <c>true</c> is returned, <paramref name="stream"/> will be non-null and
        /// must be disposed when finished with.
        /// </remarks>
        /// <param name="hash">The hash of the content to retried.</param>
        /// <param name="stream">A stream from which the stored content may be read.</param>
        /// <param name="options">Optional parameters to control how data be read from disk.
        /// See the <see cref="ReadOptions"/> enum for further details.</param>
        /// <returns><c>true</c> if the content was found, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="hash"/> is <c>null</c>.</exception>
        bool TryRead(byte[] hash, out Stream stream, ReadOptions options = ReadOptions.None);
    }
}