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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Cassette
{
    /// <summary>
    /// A content-addressable store backed by the file system.
    /// </summary>
    /// <remarks>
    /// This is cassette's default implementation of <see cref="IContentAddressableStore"/>.
    /// </remarks>
    public sealed class ContentAddressableStore : IContentAddressableStore
    {
        /// <summary>The root path for all content within this store.</summary>
        private readonly string _contentPath;

        /// <summary>The size of the byte array buffer used for read/write operations.</summary>
        private const int BufferSize = 4096;

        /// <summary>The number of characters from the hash to use for the name of the top level subdirectories.</summary>
        private const int HashPrefixLength = 4;

        /// <summary>
        /// Initialises the store to use <paramref name="contentPath"/> as the root for all content.
        /// </summary>
        /// <remarks>
        /// If <paramref name="contentPath"/> does not exist, it is created.
        /// </remarks>
        /// <param name="contentPath">The root for all content.</param>
        /// <exception cref="ArgumentNullException"><paramref name="contentPath"/> is <c>null</c>.</exception>
        public ContentAddressableStore(string contentPath)
        {
            if (contentPath == null)
                throw new ArgumentNullException("contentPath");

            _contentPath = contentPath;

            if (!Directory.Exists(_contentPath))
                Directory.CreateDirectory(_contentPath);
        }

        public async Task<byte[]> WriteAsync(Stream stream, CancellationToken cancellationToken = new CancellationToken())
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            // Create a new, empty temporary file
            // We will write the source content into this file, whilst computing the content's hash
            // Once we have the hash, we can move this temp file into the correct location
            var tempFile = Path.GetTempFileName();

            // Create a SHA-1 hash builder
            using (var hashBuilder = new SHA1CryptoServiceProvider())
            {
                // Open the temp file for write
                using (var fileStream = new FileStream(tempFile,
                    FileMode.Open, FileAccess.Write, FileShare.None,
                    BufferSize, FileOptions.SequentialScan | FileOptions.Asynchronous))
                {
                    // Allocate a buffer, used to process data in chunks
                    var buffer = new byte[BufferSize];

                    // TODO investigate parallel read/write in this loop, for increased throughput

                    // Loop until the source stream is exhausted
                    while (true)
                    {
                        // Read a chunk of data into the buffer
                        var readCount = await stream.ReadAsync(buffer, 0, BufferSize, cancellationToken);

                        // If the stream has ended, break
                        if (readCount == 0)
                            break;

                        // Integrate the source data chunk into the hash
                        hashBuilder.TransformBlock(buffer, 0, readCount, buffer, 0);

                        // Write the source data chunk to the output file
                        await fileStream.WriteAsync(buffer, 0, readCount, cancellationToken);
                    }

                    // Finalise the hash computation
                    hashBuilder.TransformFinalBlock(buffer, 0, 0);
                }

                // Retrieve the computed hash
                var hash = hashBuilder.Hash;

                // Determine the location for the content file
                string subPath;
                string contentPath;
                GetPaths(hash, out subPath, out contentPath);

                // Test whether a file already exists for this hash
                if (File.Exists(contentPath))
                {
                    // This content already exists in the store
                    // Delete the temporary file
                    File.Delete(tempFile);
                }
                else
                {
                    // Ensure the sub-path exists
                    if (!Directory.Exists(subPath))
                        Directory.CreateDirectory(subPath);

                    // Move the temporary file into its correct location
                    File.Move(tempFile, contentPath);

                    // Set the read-only flag on the file
                    File.SetAttributes(contentPath, FileAttributes.ReadOnly);
                }

                // The caller receives the hash, regardless of whether the
                // file previously existed in the store
                return hash;
            }
        }

        public bool Contains(byte[] hash)
        {
            if (hash == null)
                throw new ArgumentNullException("hash");

            string subPath;
            string contentPath;
            GetPaths(hash, out subPath, out contentPath);

            return File.Exists(contentPath);
        }

        public bool TryRead(byte[] hash, out Stream stream, ReadOptions options = ReadOptions.None)
        {
            if (hash == null)
                throw new ArgumentNullException("hash");

            string subPath;
            string contentPath;
            GetPaths(hash, out subPath, out contentPath);

            if (!File.Exists(contentPath))
            {
                stream = null;
                return false;
            }

            stream = new FileStream(contentPath,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                BufferSize, (FileOptions)options);

            return true;
        }

        public bool TryGetContentLength(byte[] hash, out long length)
        {
            if (hash == null)
                throw new ArgumentNullException("hash");

            string subPath;
            string contentPath;
            GetPaths(hash, out subPath, out contentPath);

            if (!File.Exists(contentPath))
            {
                length = 0;
                return false;
            }

            length = new FileInfo(contentPath).Length;
            return true;
        }

        public IEnumerable<byte[]> GetHashes()
        {
            var topLevelRegex = new Regex("[0-9a-f]{" + HashPrefixLength + "}", RegexOptions.IgnoreCase);
            var subLevelRegex = new Regex("[0-9a-f]{" + (Hash.StringLength - HashPrefixLength) + "}", RegexOptions.IgnoreCase);

            var directories = Directory.GetDirectories(_contentPath).Select(Path.GetFileName).Where(d => topLevelRegex.IsMatch(d));

            return from directory in directories
                   let subPath = Path.Combine(_contentPath, directory)
                   let files = Directory.GetFiles(subPath).Select(Path.GetFileName).Where(f => subLevelRegex.IsMatch(f))
                   from file in files
                   select Hash.Parse(directory + file);
        }

        public bool Delete(byte[] hash)
        {
            string subPath;
            string contentPath;
            GetPaths(hash, out subPath, out contentPath);

            if (!File.Exists(contentPath))
                return false;

            // Remove the read-only flag from the file
            var attributes = File.GetAttributes(contentPath);
            File.SetAttributes(contentPath, attributes & ~FileAttributes.ReadOnly);

            File.Delete(contentPath);
            return true;
        }

        private void GetPaths(byte[] hashBytes, out string subPath, out string contentPath)
        {
            var hashString = Hash.Format(hashBytes);
            subPath = Path.Combine(_contentPath, hashString.Substring(0, HashPrefixLength));
            contentPath = Path.Combine(subPath, hashString.Substring(HashPrefixLength));
        }
    }
}