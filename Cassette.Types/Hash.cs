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
using System.Linq;
using System.Text;

namespace Cassette
{
    /// <summary>
    /// Models a SHA1 hash.
    /// </summary>
    /// <remarks>
    /// This type holds the hash of content stored by Cassette. Data stored in Cassette
    /// is addressed by the hash of its content, so this type is used when addressing
    /// content in a <c>IContentAddressableStore</c>.
    /// <para />
    /// Internally, this type holds a single <c>byte[]</c> with 20 elements.
    /// </remarks>
    public struct Hash : IEquatable<Hash>
    {
        /// <summary>
        /// The length of a hash in string form used by <see cref="Format"/> and <see cref="Parse"/>.
        /// </summary>
        public const int StringLength = 40;

        /// <summary>
        /// The length of a hash in byte array form.
        /// </summary>
        public const int ByteCount = 20;

        private readonly byte[] _bytes;

        private Hash(byte[] bytes)
        {
            // NOTE we don't validate here as we require all callers to have done so already
            _bytes = bytes;
        }

        /// <summary>
        /// Gets the 40 character hexadecimal string representation of this hash.
        /// </summary>
        /// <example>
        /// 40613A45BC715AE4A34895CBDD6122E982FE3DF5
        /// </example>
        public override string ToString()
        {
            if (_bytes == null)
                return "0000000000000000000000000000000000000000";

            var s = new StringBuilder();
            foreach (var b in _bytes)
                s.Append(b.ToString("X2"));
            return s.ToString();
        }

        #region Static helpers

        /// <summary>
        /// Convert <paramref name="hash"/> into a 40 character hexadecimal string.
        /// </summary>
        /// <remarks>
        /// An example of this string is <c>40613A45BC715AE4A34895CBDD6122E982FE3DF5</c>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="hash"/> is <c>null</c>.</exception>
        public static string Format(byte[] hash)
        {
            if (hash == null)
                throw new ArgumentNullException(nameof(hash));
            if (hash.Length != ByteCount)
                throw new ArgumentException("Incorrect number of bytes", nameof(hash));

            var s = new StringBuilder();
            foreach (var b in hash)
                s.Append(b.ToString("X2"));
            return s.ToString();
        }

        /// <summary>
        /// Parse the hexadecimal string <paramref name="hex"/> into a <see cref="Hash"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="hex"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="hex"/> has incorrect length.</exception>
        /// <exception cref="FormatException"><paramref name="hex"/> has invalid format.</exception>
        public static Hash Parse(string hex) => new Hash(ParseToBytes(hex));

        /// <summary>
        /// Creates a <see cref="Hash"/> from a byte array.
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="bytes"/> is an invalid hash.</exception>
        public static Hash FromBytes(byte[] bytes)
        {
            if (!IsValid(bytes))
                throw new ArgumentException("Invalid byte array.", nameof(bytes));

            return new Hash(bytes);
        }

        /// <summary>
        /// Parse the hexadecimal string <paramref name="hex"/> into a 20 element byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="hex"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="hex"/> has incorrect length.</exception>
        /// <exception cref="FormatException"><paramref name="hex"/> has invalid format.</exception>
        private static byte[] ParseToBytes(string hex)
        {
            if (hex == null)
                throw new ArgumentNullException(nameof(hex));

            if (hex.Length != StringLength)
                throw new ArgumentException("Incorrect number of characters", nameof(hex));

            // TODO do this with only a single allocation (currently there are at least 23 wasted allocations)
            return Enumerable.Range(0, hex.Length)
                .Where(x => x%2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        /// <summary>
        /// Attempt to parse the hexadecimal string <paramref name="hex"/> into a <see cref="Hash"/>.
        /// </summary>
        /// <returns><c>true</c> if the parse was successful, otherwise <c>false</c>.</returns>
        public static bool TryParse(string hex, out Hash hash)
        {
            if (!TryParse(hex, out byte[] bytes))
            {
                hash = default;
                return false;
            }

            hash = new Hash(bytes);
            return true;
        }

        /// <summary>
        /// Attempt to parse the hexadecimal string <paramref name="hex"/> into a 20 element byte array.
        /// </summary>
        /// <returns><c>true</c> if the parse was successful, otherwise <c>false</c>.</returns>
        public static bool TryParse(string hex, out byte[] hash)
        {
            if (hex == null || hex.Length != StringLength)
            {
                hash = null;
                return false;
            }

            try
            {
                hash = ParseToBytes(hex);
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
        /// <remarks>
        /// This method verifies that <paramref name="hash"/> is not <c>null</c> and has the correct length.
        /// <para />
        /// Note that you never have to test validity of a <see cref="Hash"/> instance, as they are always
        /// in a valid state.
        /// </remarks>
        public static bool IsValid(string hash)
        {
            if (hash?.Length != StringLength)
                return false;

            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < hash.Length; i++)
            {
                var c = hash[i];
                if (c >= '0' && c <= '9')
                    continue;
                if (c >= 'a' && c <= 'f')
                    continue;
                if (c >= 'A' && c <= 'F')
                    continue;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get a value indicating whether <paramref name="hash"/> is a valid hash array.
        /// </summary>
        /// <remarks>
        /// This method verifies <paramref name="hash"/> is not <c>null</c> and has the correct length.
        /// <para />
        /// Note that you never have to test validity of a <see cref="Hash"/> instance, as they are always
        /// in a valid state.
        /// </remarks>
        public static bool IsValid(byte[] hash) => hash != null && hash.Length == ByteCount;

        #endregion

        #region Equality and hashing

        /// <inheritdoc />
        public bool Equals(Hash other)
        {
            if (_bytes == null)
                return other._bytes == null;
            if (other._bytes == null)
                return _bytes == null;

            for (var i = 0; i < ByteCount; i++)
            {
                if (_bytes[i] != other._bytes[i])
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is Hash hash && Equals(hash);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            if (_bytes == null)
                return 0;

            unchecked
            {
                var result = 0;
                // ReSharper disable once ForCanBeConvertedToForeach
                // ReSharper disable once LoopCanBeConvertedToQuery
                for (var i = 0; i < _bytes.Length; i++)
                    result = (result*31) ^ _bytes[i];
                return result;
            }
        }

        /// <inheritdoc />
        public static bool operator ==(Hash left, Hash right) => left.Equals(right);

        /// <inheritdoc />
        public static bool operator !=(Hash left, Hash right) => !left.Equals(right);

        #endregion
    }
}
