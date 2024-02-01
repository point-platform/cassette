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

using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Cassette.Tests;

public class ContentAddressableStoreTests : IDisposable
{
    private readonly ContentAddressableStore _store;
    private readonly string _contentPath;

    public ContentAddressableStoreTests()
    {
        // Create a new temporary content path
        var tempPath = Path.GetTempPath();
        var random = new Random();
        do
        {
            _contentPath = Path.Combine(tempPath, "Cassette-Test-Data-" + random.Next().ToString("X"));
        }
        while (Directory.Exists(_contentPath));

        // Create a store
        _store = new ContentAddressableStore(_contentPath);
    }

    public void Dispose()
    {
        if (_contentPath == null)
            return;

        // Recursively delete all generated content
        foreach (var subpath in Directory.GetDirectories(_contentPath))
        {
            foreach (var contentFile in Directory.GetFiles(subpath))
            {
                File.SetAttributes(contentFile, FileAttributes.Normal);
                File.Delete(contentFile);
            }
            Directory.Delete(subpath);
        }
        Directory.Delete(_contentPath);
    }

    [Fact]
    public async void Contains()
    {
        Assert.False(_store.Contains(Hash.Parse("40613A45BC715AE4A34895CBDD6122E982FE3DF5")));
        Assert.False(_store.Contains(Hash.Parse("07FA0B2F00BA82A440BFEACAFD8B0B8D1B3E4EE7")));
        Assert.False(_store.Contains(Hash.Parse("0E913CE8D693110A1FD2F894F35F0A77C97B9A74")));

        var stream = TestUtil.GetRandomData(1024);

        var hash = await _store.WriteAsync(stream);

        Assert.True(_store.Contains(hash));
    }

    [Fact]
    public async void WriteProducesCorrectHash()
    {
        var stream = TestUtil.GetRandomData(1024);

        var expectedHash = TestUtil.CalculateHash(stream);

        stream.Position = 0;

        Assert.Equal(expectedHash, await _store.WriteAsync(stream));
    }

    [Fact]
    public async void WriteExistingContent()
    {
        var stream = TestUtil.GetRandomData(1024);

        var hash1 = await _store.WriteAsync(stream);

        stream.Position = 0;

        var hash2 = await _store.WriteAsync(stream);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public async void WriteCreatesFile()
    {
        var stream = TestUtil.GetRandomData(1024);

        var hash = await _store.WriteAsync(stream);

        var subpath = Path.Combine(_contentPath, hash.ToString().Substring(0, 4));
        var contentPath = Path.Combine(subpath, hash.ToString().Substring(4));
        Assert.True(Directory.Exists(subpath));
        Assert.True(File.Exists(contentPath));

        var fileInfo = new FileInfo(contentPath);
        Assert.Equal(1024, fileInfo.Length);
        Assert.Equal(FileAttributes.ReadOnly, fileInfo.Attributes);
        Assert.True(fileInfo.IsReadOnly);

        stream.Position = 0;
        using (var fileStream = File.OpenRead(contentPath))
            TestUtil.AssertStreamsEqual(stream, fileStream);
    }

    [Fact]
    public void TryReadAbsentContent()
    {
        var hash = Hash.Parse("40613A45BC715AE4A34895CBDD6122E982FE3DF5");

        Assert.False(_store.TryRead(hash, out _));
    }

    [Fact]
    public async void TryReadExistingContent()
    {
        Stream stream = TestUtil.GetRandomData(1024);

        var hash = await _store.WriteAsync(stream);

        stream.Position = 0;

        Assert.True(_store.TryRead(hash, out Stream? storedStream));

        using (storedStream)
            TestUtil.AssertStreamsEqual(stream, storedStream);
    }

    [Fact]
    public async void StoringWithEncoding()
    {
        var stream = TestUtil.GetRandomData(4096, keepBits: 4);
        var hash = await _store.WriteAsync(stream, encodings: new[] { new GZipContentEncoding() });

        var subpath = Path.Combine(_contentPath, hash.ToString().Substring(0, 4));
        var contentPath = Path.Combine(subpath, hash.ToString().Substring(4));

        Assert.True(File.Exists(contentPath));
        Assert.True(File.Exists(contentPath + ".gzip"));

        Assert.True(_store.Contains(hash));
        Assert.True(_store.Contains(hash, "gzip"));
        Assert.False(_store.Contains(hash, "deflate"));


        Assert.True(_store.TryGetContentLength(hash, out long length));
        Assert.Equal(4096L, length);

        Assert.True(_store.TryGetContentLength(hash, out length, "gzip"));
        Assert.InRange(length, 1L, 4096L - 1);

        Assert.Equal(hash, _store.GetHashes().SingleOrDefault());

        Assert.True(_store.TryRead(hash, out Stream? readStream));
        readStream.Dispose();

        Assert.True(_store.TryRead(hash, out readStream, encodingName: "gzip"));
        readStream.Dispose();

        Assert.False(_store.TryRead(hash, out _, encodingName: "deflate"));

        Assert.True(_store.Delete(hash));

        Assert.False(File.Exists(contentPath));
        Assert.False(File.Exists(contentPath + ".gzip"));
    }
}