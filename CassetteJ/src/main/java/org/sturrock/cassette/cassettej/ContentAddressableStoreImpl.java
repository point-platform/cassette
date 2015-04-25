package org.sturrock.cassette.cassettej;

/*
 * Copyright 2015 Andy Sturrock
 * Derived from https://github.com/drewnoakes/cassette
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

import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.nio.file.DirectoryStream;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.attribute.BasicFileAttributes;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.LinkedList;
import java.util.List;

/**
 * A content-addressable store backed by the file system. Default implementation
 * of ContentAddressableStore.
 * 
 */
public final class ContentAddressableStoreImpl implements
		ContentAddressableStore {
	/**
	 * The root path for all content within this store
	 */
	private final Path rootPath;

	/**
	 * The size of the byte array buffer used for read/write operations.
	 */
	private final int bufferSize = 4096;

	/**
	 * The number of characters from the hash to use for the name of the top
	 * level subdirectories.
	 */
	private final int hashPrefixLength = 4;

	/**
	 * Initialises the store, using rootPath as the root for all content.
	 * 
	 * @param rootPath
	 *            Root path for all content in this store.
	 * @throws IOException
	 */
	public ContentAddressableStoreImpl(String rootPath) throws IOException {
		if (rootPath == null)
			throw new IllegalArgumentException("rootPath");

		this.rootPath = Paths.get(rootPath);

		if (!Files.isDirectory(this.rootPath))
			Files.createDirectory(this.rootPath);
	}

	public byte[] write(InputStream stream) throws IOException {
		if (stream == null)
			throw new IllegalArgumentException("stream");

		MessageDigest messageDigest;
		try {
			messageDigest = MessageDigest.getInstance("SHA1");
		} catch (NoSuchAlgorithmException e) {
			throw new IllegalArgumentException(e);
		}
		final byte[] buffer = new byte[bufferSize];
		for (int read = 0; (read = stream.read(buffer)) != -1;) {
			messageDigest.update(buffer, 0, read);
		}
		final byte[] hash = messageDigest.digest();
		stream.reset();

		String hashString = Hash.getString(hash);

		// Determine the location for the content file
		Path contentPath = getContentPath(hashString);
		Path subPath = getSubPath(hashString);

		// Test whether a file already exists for this hash
		if (!Files.exists(contentPath)) {
			// Ensure the sub-path exists
			if (!Files.isDirectory(subPath))
				Files.createDirectories(subPath);

			Files.copy(stream, contentPath);
		}

		// The caller receives the hash, regardless of whether the
		// file previously existed in the store
		return hash;
	}

	public boolean contains(byte[] hash) {
		if (hash == null)
			throw new IllegalArgumentException("hash");

		String hashString = Hash.getString(hash);
		Path contentPath = getContentPath(hashString);

		return Files.exists(contentPath);
	}

	public InputStream read(byte[] hash) throws FileNotFoundException {
		if (hash == null)
			throw new IllegalArgumentException("hash");

		String hashString = Hash.getString(hash);
		Path contentPath = getContentPath(hashString);

		if (!Files.exists(contentPath)) {
			return null;
		}

		return new FileInputStream(contentPath.toFile());
	}

	public long getContentLength(byte[] hash) throws IOException {
		if (hash == null)
			throw new IllegalArgumentException("hash");

		String hashString = Hash.getString(hash);
		Path contentPath = getContentPath(hashString);

		if (!Files.exists(contentPath)) {
			return -1;
		}

		BasicFileAttributes attrs;
		attrs = Files.readAttributes(contentPath, BasicFileAttributes.class);
		return attrs.size();
	}

	public List<byte[]> getHashes() throws IOException {
		List<byte[]> hashes = new LinkedList<byte[]>();

		DirectoryStream<Path> directories = Files.newDirectoryStream(rootPath,
				"[0-9A-F]*");

		for (Path directory : directories) {
			DirectoryStream<Path> files = Files.newDirectoryStream(directory,
					"[0-9A-F]*");
			for (Path file : files) {
				byte[] bytes = Hash.getBytes(directory.getFileName().toString()
						+ file.getFileName().toString());
				hashes.add(bytes);
			}
		}
		return hashes;
	}

	public boolean delete(byte[] hash) throws IOException {
		String hashString = Hash.getString(hash);
		Path contentPath = getContentPath(hashString);

		if (!Files.exists(contentPath))
			return false;

		Files.delete(contentPath);
		return true;
	}

	private Path getContentPath(String hashString) {
		Path subPath = getSubPath(hashString);
		Path contentPath = Paths.get(subPath.toString(),
				hashString.substring(hashPrefixLength));
		return contentPath;
	}

	private Path getSubPath(String hashString) {
		Path subPath = Paths.get(rootPath.toString(),
				hashString.substring(0, hashPrefixLength));
		return subPath;
	}
}
