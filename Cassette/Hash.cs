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
using System.Text.RegularExpressions;

namespace Cassette
{
    /// <summary>
    /// Utility functions for working with hashes.
    /// </summary>
    public static class Hash
    {
        private const int Sha1StringLength = 40;
        private const int Sha1ByteCount = 20;

        private static readonly Regex _hashRegex = new Regex("[0-9a-fA-F]{" + Sha1StringLength + "}", RegexOptions.Compiled);

        /// <summary>
        /// Convert <paramref name="hash"/> into a 40 character hexadecimal string.
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
        /// Parse the hexadecimal string <paramref name="hex"/> into a 20 element byte array.
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

        /// <summary>
        /// Attempt to parse the hexadecimal string <paramref name="hex"/> into a 20 element byte array.
        /// </summary>
        /// <returns><c>true</c> if the parse was successful, otherwise <c>false</c>.</returns>
        [Pure]
        public static bool TryParse(string hex, out byte[] hash)
        {
            if (hex == null || hex.Length != Sha1StringLength)
            {
                hash = null;
                return false;
            }

            try
            {
                hash = Parse(hex);
                return true;
            }
            catch
            {
                hash = null;
                return false;
            }
        }

        /// <summary>
        /// Get a value indicating whether <paramref name="hash"/> is a valid hexadecimal hash string.
        /// </summary>
        [Pure]
        public static bool IsValid(string hash)
        {
            return hash != null && _hashRegex.IsMatch(hash);
        }
    }
}