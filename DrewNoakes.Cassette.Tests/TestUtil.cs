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

using System.IO;
using System.Security.Cryptography;
using Xunit;

namespace Cassette.Tests
{
    internal static class TestUtil
    {
        private static readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();

        public static void AssertStreamsEqual(Stream expectedStream, Stream actualStream)
        {
            while (true)
            {
                var expected = expectedStream.ReadByte();
                var actual = actualStream.ReadByte();

                Assert.Equal(expected, actual);

                if (expected == -1 && actual == -1)
                    break;
            }
        }

        public static MemoryStream GetRandomData(int count, int keepBits = 8)
        {
            var stream = new MemoryStream();
            var bytes = new byte[count];

            _random.GetBytes(bytes);

            if (keepBits < 8)
            {
                var mask = (byte)((1 << keepBits) - 1);
                for (var i = 0; i < bytes.Length; i++)
                    bytes[i] &= mask;
            }

            stream.Write(bytes, 0, bytes.Length);
            stream.Position = 0;
            return stream;
        }

        public static Hash CalculateHash(MemoryStream stream)
        {
            // NOTE we need a new instance for each calculation as it's not threadsafe
            using (var hash = new SHA1CryptoServiceProvider())
                return Hash.FromBytes(hash.ComputeHash(stream));
        }
    }
}