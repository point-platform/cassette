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

using System.IO;

namespace Cassette
{
    /// <summary>
    /// Defines an encoding for use with <see cref="IContentAddressableStore"/>.
    /// </summary>
    /// <remarks>
    /// Such encodings allow data to be stored in a different format and later retrieved in that
    /// format directly.
    /// </remarks>
    public interface IContentEncoding
    {
        /// <summary>Gets the name given to the type of encoding.</summary>
        string Name { get; }

        /// <summary>
        /// Encodes <paramref name="stream"/>, returning a new stream adhering to the type of encoding.
        /// </summary>
        /// <param name="stream">The stream to encode.</param>
        /// <returns>An encoded stream.</returns>
        Stream Encode(Stream stream);

        /// <summary>
        /// Decodes <paramref name="stream"/>, returning a new stream adhering to the type of encoding.
        /// </summary>
        /// <param name="stream">The stream to decode.</param>
        /// <returns>A decoded stream.</returns>
        Stream Decode(Stream stream);
    }
}