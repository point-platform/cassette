# Cassette

[![Cassette NuGet version](https://img.shields.io/nuget/v/DrewNoakes.Cassette.svg)](https://www.nuget.org/packages/DrewNoakes.Cassette/)

_Cassette_ is a simple content-addressable storage system for .NET 4.5.

Content-addressable storage (CAS) is a fast and efficient mechanism for storing and retrieving fixed data on disk.

Information is uniquely and unambiguously identified by the SHA-1 hash of its contents.

```csharp
// Create a store, backed by the specified file system location
var cassette = new ContentAddressableStore(@"c:\cassette-data\");

// Store some content, obtaining its hash (content address)
byte[] hash = await cassette.WriteAsync(writeStream);

// Later, use the hash to look up the content
Stream readStream;
if (cassette.TryRead(hash, out readStream))
{
    using (readStream)
    {
        // Read the stored content via the returned read-only stream
    }
}
```

A significant advantage of CAS is its efficient use of storage media for data backups where a majority of files are identical, and so separate storage would be redundant.

For more information, read [Wikipedia's CAS article](http://en.wikipedia.org/wiki/Content-addressable_storage).
