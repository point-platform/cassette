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

using Xunit;

namespace Cassette.Tests
{
    public sealed class HashTests
    {
        [Fact]
        public void RoundTrip()
        {
            const string hashString = "40613A45BC715AE4A34895CBDD6122E982FE3DF5";

            Assert.Equal(hashString, Hash.Format(Hash.Parse(hashString)));
        }
    }
}