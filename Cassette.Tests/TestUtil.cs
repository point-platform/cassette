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

        public static MemoryStream GetRandomData(int count)
        {
            var stream = new MemoryStream();
            var bytes = new byte[count];
            _random.GetBytes(bytes);
            stream.Write(bytes, 0, bytes.Length);
            stream.Position = 0;
            return stream;
        }

        public static byte[] CalculateHash(MemoryStream stream)
        {
            byte[] expectedHash;
            using (var hash = new SHA1CryptoServiceProvider())
                expectedHash = hash.ComputeHash(stream);
            stream.Position = 0;
            return expectedHash;
        }
    }
}