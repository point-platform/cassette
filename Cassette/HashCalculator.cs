/*
 * Copyright 2015-2017 Drew Noakes
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
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Cassette
{
    /// <summary>
    /// Computes hashes from data.
    /// </summary>
    public static class HashCalculator
    {
        private const int BufferSize = 4096;

        /// <summary>
        /// Compute the hash over the contents of <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path of a file to process.</param>
        /// <returns>The hash of <paramref name="path"/>'s contents.</returns>
        public static Hash Compute(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.SequentialScan))
                return Compute(fileStream);
        }
        /// <summary>
        /// Compute the hash over the contents of <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path of a file to process.</param>
        /// <param name="bufferSize">Optional size of the buffer to use when reading chunks from the stream. Defaults to 4096.</param>
        /// <returns>A task that yields the hash of <paramref name="path"/>'s contents.</returns>
        public static Task<Hash> ComputeAsync(string path, int bufferSize = 4096)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan))
                return ComputeAsync(fileStream, bufferSize);
        }

        /// <summary>
        /// Compute the hash over the contents of <paramref name="stream" />.
        /// </summary>
        /// <param name="stream">The stream to process.</param>
        /// <returns>The hash of <paramref name="stream"/>'s contents.</returns>
        public static Hash Compute(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var hashFunction = SHA1.Create())
                return Hash.FromBytes(hashFunction.ComputeHash(stream));
        }

        /// <summary>
        /// Compute the hash over the contents of <paramref name="stream" />, reading asynchronously from the stream.
        /// </summary>
        /// <param name="stream">The stream to process.</param>
        /// <returns>A task that yields the hash of <paramref name="stream"/>'s contents.</returns>
        public static async Task<Hash> ComputeAsync(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var hashFunction = IncrementalHash.CreateHash(HashAlgorithmName.SHA1))
                return Hash.FromBytes(await hashFunction.ComputeHashAsync(stream));
        }
    }

    internal static class IncrementalHashExtensions
    {
        public static async Task<byte[]> ComputeHashAsync(this IncrementalHash algorithm, Stream inputStream)
        {
            const int bufferSize = 4096;

            var buffer = new byte[bufferSize];
            
            while (true)
            {
                var read = await inputStream.ReadAsync(buffer, 0, bufferSize);

                if (read == 0)
                    break;

                algorithm.AppendData(buffer, 0, read);
            }
            
            return algorithm.GetHashAndReset();
        }
    }
}