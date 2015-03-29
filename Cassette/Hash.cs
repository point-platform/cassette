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
        // TODO add TryParse

        /// <summary>
        /// Convert <paramref name="hash"/> into a hexadecimal string.
        /// </summary>
        /// <remarks>
        /// An example of this string is <c>40613A45BC715AE4A34895CBDD6122E982FE3DF5</c>.
        /// </remarks>
        [Pure]
        public static string Format(byte[] hash)
        {
            var s = new StringBuilder();
            foreach (var b in hash)
                s.Append(b.ToString("X2"));
            return s.ToString();
        }

        /// <summary>
        /// Parse the hexadecimal string <paramref name="hex"/> into a byte array.
        /// </summary>
        [Pure]
        public static byte[] Parse(string hex)
        {
            if (hex.Length%2 == 1)
                throw new ArgumentException("Must have an even number of characters", "hex");

            return Enumerable.Range(0, hex.Length)
                .Where(x => x%2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
    }
}