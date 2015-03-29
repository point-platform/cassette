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