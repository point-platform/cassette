package org.sturrock.cassette.cassettej;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.nio.charset.StandardCharsets;
import java.nio.file.FileVisitResult;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.SimpleFileVisitor;
import java.nio.file.attribute.BasicFileAttributes;
import java.util.List;

import junit.framework.Test;
import junit.framework.TestCase;
import junit.framework.TestSuite;

/**
 * Unit test for simple ContentAddressableStoreImpl.
 */
public class ContentAddressableStoreImplTest extends TestCase {
	private ContentAddressableStore cas;
	private Path tempDir;
	private String helloWorldString = "Hello World";
	// Precomputed sha1 hash of "Hello World"
	private byte[] helloWorldHash = Hash
					.getBytes("0A4D55A8D778E5022FAB701977C5D840BBC486D0");

	public ContentAddressableStoreImplTest(String testName) {
		super(testName);
	}

	public void setUp() {
		try {
			tempDir = Files
					.createTempDirectory("ContentAddressableStoreImplTest");
			cas = new ContentAddressableStoreImpl(tempDir.toString());
		} catch (IOException e) {
			super.fail();
		}
	}

	public void tearDown() {
		try {
			deleteTempDirectory();
		} catch (IOException e) {
			fail();
		}
	}

	private void deleteTempDirectory() throws IOException {
		Files.walkFileTree(tempDir, new SimpleFileVisitor<Path>() {
			@Override
			public FileVisitResult visitFile(Path file,
					BasicFileAttributes attrs) throws IOException {
				Files.delete(file);
				return FileVisitResult.CONTINUE;
			}

			@Override
			public FileVisitResult postVisitDirectory(Path dir, IOException e)
					throws IOException {
				if (e == null) {
					Files.delete(dir);
					return FileVisitResult.CONTINUE;
				} else {
					// directory iteration failed
					throw e;
				}
			}
		});
	}

	/**
	 * @return the suite of tests being tested
	 */
	public static Test suite() {
		return new TestSuite(ContentAddressableStoreImplTest.class);
	}

	private byte[] writeHelloWorld() throws IOException {
		
		byte[] bytes = helloWorldString.getBytes(StandardCharsets.UTF_8);

		InputStream stream = new ByteArrayInputStream(bytes);
		return cas.write(stream);
	}

	public void testWrite() {
		byte[] actual;
		try {
			actual = writeHelloWorld();
			assert (Hash.equals(actual, helloWorldHash));
		} catch (IOException e) {
			fail(e.getMessage());
		}
	}

	public void testContains() {
		try {
			writeHelloWorld();
		} catch (IOException e) {
			fail(e.getMessage());
		}
		// Now cas should contain some content with this hash
		assert (cas.contains(helloWorldHash));
	}

	public void testGetContentLength() {
		try {
			writeHelloWorld();
		} catch (IOException e) {
			fail(e.getMessage());
		}
		// The content length should be 11 (Hello World)
		try {
			assertEquals(cas.getContentLength(helloWorldHash),
					helloWorldString.length());
		} catch (IOException e) {
			fail(e.getMessage());
		}
	}
	
	public void testGetHashes() {
		try {
			writeHelloWorld();
		} catch (IOException e) {
			fail(e.getMessage());
		}
		
		List<byte[]> hashes;
		try {
			hashes = cas.getHashes();
			// Should only be one piece of content
			assertEquals(hashes.size(), 1);
			// The hash should be the same as above
			assert (Hash.equals(helloWorldHash, hashes.get(0)));
		} catch (IOException e) {
			fail(e.getMessage());
		}
	}
	
	public void testRead() {
		try {
			writeHelloWorld();
		} catch (IOException e) {
			fail(e.getMessage());
		}
		
		InputStream stream;
		try {
			stream = cas.read(helloWorldHash);
			if (stream == null)
				fail("Content not found");
			String content = new String(readFully(stream),
					StandardCharsets.UTF_8);
			// Content should be Hello World
			assertEquals(helloWorldString, content);
		} catch (FileNotFoundException e) {
			fail(e.getMessage());
		} catch (IOException e) {
			fail(e.getMessage());
		}
	}

private static byte[] readFully(InputStream inputStream) throws IOException {
	ByteArrayOutputStream baos = new ByteArrayOutputStream();
	byte[] buffer = new byte[1024];
	int length = 0;
	while ((length = inputStream.read(buffer)) != -1) {
		baos.write(buffer, 0, length);
	}
	return baos.toByteArray();
}
}
