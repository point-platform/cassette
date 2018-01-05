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
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Xunit;

namespace Cassette.Tests
{
    public sealed class HashCalculatorTests
    {
        [Fact]
        public void ComputeFromStream()
        {
            var (bytes, expectedHash) = GenerateTestData();

            var stream = new MemoryStream(bytes);
            
            Assert.Equal(expectedHash, HashCalculator.Compute(stream));
        }
        
        [Fact]
        public void ComputeFromStreamThreadSafety()
        {
            var (bytes, expectedHash) = GenerateTestData();

            const int threadCount = 4;

            var allCorrect = true;
            
            var semaphore = new SemaphoreSlim(initialCount: threadCount, maxCount: threadCount);
            
            var threads = Enumerable.Range(0, threadCount).Select(_ => new Thread(ThreadMethod)).ToList();

            foreach (var thread in threads)
                thread.Start();

            Thread.Sleep(100);

            semaphore.Release(threadCount);

            foreach (var thread in threads)
                thread.Join();

            Assert.True(allCorrect);

            void ThreadMethod()
            {
                var stream = new MemoryStream(bytes);

                semaphore.Wait();

                var actualHash = HashCalculator.Compute(stream);

                if (expectedHash != actualHash)
                    allCorrect = false;
            }
        }

        private static (byte[] bytes, Hash expectedHash) GenerateTestData()
        {
            var bytes = Enumerable.Range(0, 10 * 1024 * 1024).Select(i => (byte) i).ToArray();

            var expectedHash = SHA1.Create().ComputeHash(bytes);
            
            return (bytes, Hash.FromBytes(expectedHash));
        }
    }
}