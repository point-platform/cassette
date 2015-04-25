package org.sturrock;

import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.util.List;

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

/**
 * Defines a content-addressable store
 */
public interface ContentAddressableStore {
	/**
	 * Write content to the store, returning its hash. If the store already
	 * contains this content, the write is discarded but the hash is returned as
	 * normal.
	 * 
	 * @param stream
	 *            Content to be written
	 * @return hash of content
	 * @throws IOException
	 */
	byte[] write(InputStream stream) throws IOException;

	/**
	 * Check whether content exists in the store with the specified hash
	 * 
	 * @param hash
	 *            Hash of content to check
	 * @return <code>true</code> if content exists in the store with specified
	 *         hash
	 */
	boolean contains(byte[] hash);

	/**
	 * Read content from the store.
	 * 
	 * @param hash
	 *            The hash of the content to read.
	 * @return <code>InputStream</code> of content .
	 * @throws FileNotFoundException
	 */
	InputStream read(byte[] hash) throws FileNotFoundException;

	/**
	 * Get the length of the content with the specified hash
	 * 
	 * @param hash
	 *            The hash of the content
	 * @return The length of the content, or -1 if no content with the specified
	 *         hash exists.
	 * @throws IOException
	 */
	long getContentLength(byte[] hash) throws IOException;

	/**
	 * Get a list of all hashes in the store. The list is generated lazily by
	 * querying the filesystem, and thus will not behave deterministically if
	 * more content is added while this function is called.
	 * 
	 * @return List of hashes in the store.
	 * @throws IOException
	 */
	List<byte[]> getHashes() throws IOException;

	/**
	 * Delete content from the store.
	 * 
	 * @param hash
	 *            Hash of content to delete
	 * @return <code>true</code> if the content existed and was deleted;
	 *         otherwise false.
	 * @throws IOException
	 */
	boolean delete(byte[] hash) throws IOException;
}
