package org.sturrock.cassette.cassettej;

import java.util.Formatter;

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
 * Utility class for working with hashes.
 *
 */
public final class Hash {
	
	public static final int byteCount = 20;
	
	public static String getString(byte[] hash) {
		Formatter formatter = new Formatter();
		for (final byte b : hash) {
			formatter.format("%02X", b);
		}
		String hashString = formatter.toString();
		formatter.close();
		return hashString;
	}

	public static byte[] getBytes(String string) {
		int len = string.length();
		byte[] data = new byte[len / 2];
		for (int i = 0; i < len; i += 2) {
			data[i / 2] = (byte) ((Character.digit(string.charAt(i), 16) << 4) + Character
					.digit(string.charAt(i + 1), 16));
		}
		return data;
	}
	
	public static boolean equals(byte[] hash1, byte[] hash2)
    {
        if (hash1 == null)
            throw new IllegalArgumentException("hash1");
        if (hash2 == null)
            throw new IllegalArgumentException("hash2");
        if (hash1.length != byteCount)
            throw new IllegalArgumentException("hash1 has invalid length.");
        if (hash2.length != byteCount)
            throw new IllegalArgumentException("hash2 hHas invalid length.");

        for (int i = 0; i < byteCount; ++i)
        {
            if (hash1[i] != hash2[i])
                return false;
        }

        return true;
    }


}
