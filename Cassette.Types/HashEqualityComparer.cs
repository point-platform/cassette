/*
 * Copyright 2015-2018 Drew Noakes
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

using System.Collections.Generic;

namespace Cassette
{
    /// <summary>
    /// Implementation of <see cref="IEqualityComparer{T}"/> for hash byte arrays.
    /// </summary>
    public sealed class HashBytesEqualityComparer : IEqualityComparer<byte[]>
    {
        /// <inheritdoc />
        public bool Equals(byte[] x, byte[] y)
        {
            if (x == null ^ y == null)
                return false;
            return ReferenceEquals(x, y) || Hash.Equals(x, y);
        }

        /// <inheritdoc />
        public int GetHashCode(byte[] hash)
        {
            // Implementation from http://stackoverflow.com/a/468084/24874

            unchecked
		    {
			    const int p = 0x1000193;

                var code = (int)0x811c9dc5;

		        // ReSharper disable once LoopCanBeConvertedToQuery
		        // ReSharper disable once ForCanBeConvertedToForeach
			    for (var i = 0; i < hash.Length; i++)
				    code = (code ^ hash[i]) * p;

			    code += code << 13;
			    code ^= code >> 7;
			    code += code << 3;
			    code ^= code >> 17;
			    code += code << 5;

                return code;
		    }
        }
    }
}