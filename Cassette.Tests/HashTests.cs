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

using Xunit;

namespace Cassette.Tests
{
    public sealed class HashTests
    {
        [Fact]
        public void RoundTripString()
        {
            const string hashString = "40613A45BC715AE4A34895CBDD6122E982FE3DF5";

            Assert.Equal(hashString, Hash.Parse(hashString).ToString());
        }

        [Fact]
        public void Constants()
        {
            Assert.Equal(20, Hash.ByteCount);
            Assert.Equal(40, Hash.StringLength);
        }

        [Fact]
        public void IsValid()
        {
            Assert.True(Hash.IsValid("40613A45BC715AE4A34895CBDD6122E982FE3DF5"));
            Assert.True(Hash.IsValid("0000000000000000000000000000000000000000"));
            Assert.True(Hash.IsValid("ABCDEFABCDEFABCDEFABCDEFABCDEFABCDEFABCD"));
            Assert.True(Hash.IsValid("abcdefabcdefabcdefabcdefabcdefabcdefabcd"));

            Assert.False(Hash.IsValid("0000000000000000000000000000000000000000  "));
            Assert.False(Hash.IsValid("   0000000000000000000000000000000000000000"));
            Assert.False(Hash.IsValid("0000000000000000000000000000000000000000111"));
            Assert.False(Hash.IsValid("0000000000000000000000000000000000000"));
            Assert.False(Hash.IsValid("xyzxyzxyzxyzxyzxyzxyzxyzxyzxyzxyzxyzxyzx"));
            Assert.False(Hash.IsValid((string)null));

            Assert.True(Hash.IsValid(new byte[Hash.ByteCount]));

            Assert.False(Hash.IsValid(new byte[Hash.ByteCount - 1]));
            Assert.False(Hash.IsValid(new byte[Hash.ByteCount + 1]));
            Assert.False(Hash.IsValid(new byte[0]));
            Assert.False(Hash.IsValid((byte[])null));
        }
    }
}