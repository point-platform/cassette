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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Cassette
{
    /// <summary>
    /// Utility functions for working with hashes.
    /// </summary>
    public static class Hash
    {
        private const int Sha1StringLength = 40;
        private const int Sha1ByteCount = 20;
        // TODO add TryParse

        /// <summary>
        /// Convert <paramref name="hash"/> into a hexadecimal string.
        /// </summary>
        /// <remarks>
        /// An example of this string is <c>40613A45BC715AE4A34895CBDD6122E982FE3DF5</c>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="hash"/> is <c>null</c>.</exception>
        [Pure]
        public static string Format(byte[] hash)
        {
            if (hash == null)
                throw new ArgumentNullException("hash");
            if (hash.Length != Sha1ByteCount)
                throw new ArgumentException("Incorrect number of bytes", "hash");

            var s = new StringBuilder();
            foreach (var b in hash)
                s.Append(b.ToString("X2"));
            return s.ToString();
        }

        /// <summary>
        /// Parse the hexadecimal string <paramref name="hex"/> into a byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="hex"/> is <c>null</c>.</exception>
        [Pure]
        public static byte[] Parse(string hex)
        {
            if (hex == null)
                throw new ArgumentNullException("hex");

            if (hex.Length != Sha1StringLength)
                throw new ArgumentException("Incorrect number of characters", "hex");

            return Enumerable.Range(0, hex.Length)
                .Where(x => x%2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
    }
}