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

        public async Task<byte[]> WriteAsync(Stream stream, CancellationToken cancellationToken = new CancellationToken(), IEnumerable<IContentEncoding> encodings = null)
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
                    // We use parallel read/write for increased throughput which requires two buffers
                    var buffers = new[] { new byte[BufferSize], new byte[BufferSize] };

                    var bufferIndex = 0;
                    var writeTask = (Task)null;

                    // Loop until the source stream is exhausted
                    while (true)
                    {
                        // Swap buffers
                        bufferIndex ^= 1;

                        // Start read a chunk of data into the buffer asynchronously
                        var readTask = stream.ReadAsync(buffers[bufferIndex], 0, BufferSize, cancellationToken);

                        if (writeTask != null)
                            await Task.WhenAll(readTask, writeTask);

                        var readCount = readTask.Result;

                        // If the stream has ended, break
                        if (readCount == 0)
                            break;

                        // Integrate the source data chunk into the hash
                        hashBuilder.TransformBlock(buffers[bufferIndex], 0, readCount, null, 0);

                        // Write the source data chunk to the output file
                        writeTask = fileStream.WriteAsync(buffers[bufferIndex], 0, readCount, cancellationToken);
                    }

                    // Finalise the hash computation
                    hashBuilder.TransformFinalBlock(buffers[bufferIndex], 0, 0);
                }

                // Retrieve the computed hash
                var hash = hashBuilder.Hash;

                // Determine the location for the content file
                string subPath;
                string contentPath;
                GetPaths(hash, null, out subPath, out contentPath);

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

                // Write any encoded forms of the content too
                if (encodings != null)
                {
                    foreach (var encoding in encodings)
                    {
                        var encodedContentPath = contentPath + "." + encoding.Name;

                        if (File.Exists(encodedContentPath))
                            continue;

                        // Create a new temporary file for the encoded content
                        tempFile = Path.GetTempFileName();

                        using (var inputStream = new FileStream(contentPath,
                            FileMode.Open, FileAccess.Read, FileShare.Read,
                            BufferSize, FileOptions.SequentialScan | FileOptions.Asynchronous))
                        using (var outputStream = new FileStream(tempFile,
                            FileMode.Open, FileAccess.Write, FileShare.None,
                            BufferSize, FileOptions.SequentialScan | FileOptions.Asynchronous))
                        using (var encodedOutputStream = encoding.Encode(outputStream))
                        {
                            await inputStream.CopyToAsync(encodedOutputStream, BufferSize, cancellationToken);
                        }

                        // Move the temporary file into its correct location
                        File.Move(tempFile, encodedContentPath);

                        // Set the read-only flag on the file
                        File.SetAttributes(encodedContentPath, FileAttributes.ReadOnly);
                    }
                }

                // The caller receives the hash, regardless of whether the
                // file previously existed in the store
                return hash;
            }
        }

        public bool Contains(byte[] hash, string encodingName = null)
        {
            if (hash == null)
                throw new ArgumentNullException("hash");

            return File.Exists(GetContentPath(hash, encodingName));
        }

        public bool TryRead(byte[] hash, out Stream stream, ReadOptions options = ReadOptions.None, string encodingName = null)
        {
            if (hash == null)
                throw new ArgumentNullException("hash");

            var contentPath = GetContentPath(hash, encodingName);

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

        public bool TryGetContentLength(byte[] hash, out long length, string encodingName = null)
        {
            if (hash == null)
                throw new ArgumentNullException("hash");

            var contentPath = GetContentPath(hash, encodingName);

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
            var topLevelRegex = new Regex("^[0-9a-f]{" + HashPrefixLength + "}$", RegexOptions.IgnoreCase);
            var subLevelRegex = new Regex("^[0-9a-f]{" + (Hash.StringLength - HashPrefixLength) + "}$", RegexOptions.IgnoreCase);

            var directories = Directory.GetDirectories(_contentPath).Select(Path.GetFileName).Where(d => topLevelRegex.IsMatch(d));

            return from directory in directories
                   let subPath = Path.Combine(_contentPath, directory)
                   let files = Directory.GetFiles(subPath).Select(Path.GetFileName).Where(f => subLevelRegex.IsMatch(f))
                   from file in files
                   select Hash.Parse(directory + file);
        }

        public bool Delete(byte[] hash)
        {
            var hashString = Hash.Format(hash);
            var subPath = GetSubPath(hashString);

            var files = Directory.GetFiles(subPath, hashString.Substring(HashPrefixLength) + ".*", SearchOption.TopDirectoryOnly);

            if (files.Length == 0)
                return false;

            foreach (var file in files)
            {
                // Remove the read-only flag from the file
                var attributes = File.GetAttributes(file);
                File.SetAttributes(file, attributes & ~FileAttributes.ReadOnly);

                // Delete the file
                File.Delete(file);
            }

            return true;
        }

        #region Computing paths

        private void GetPaths(byte[] hashBytes, string encodingName, out string subPath, out string contentPath)
        {
            var hashString = Hash.Format(hashBytes);
            subPath = GetSubPath(hashString);
            contentPath = Path.Combine(subPath, hashString.Substring(HashPrefixLength));
            if (encodingName != null)
                contentPath += "." + encodingName;
        }

        private string GetContentPath(byte[] hash, string encodingName = null)
        {
            var hashString = Hash.Format(hash);
            var subPath = GetSubPath(hashString);
            var contentPath = Path.Combine(subPath, hashString.Substring(HashPrefixLength));
            return encodingName != null
                ? contentPath + "." + encodingName
                : contentPath;
        }

        private string GetSubPath(string hashString)
        {
            var subPath = Path.Combine(_contentPath, hashString.Substring(0, HashPrefixLength));
            return subPath;
        }

        #endregion
    }
}