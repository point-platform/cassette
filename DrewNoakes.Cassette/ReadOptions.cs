/*
 * Copyright 2015-2016 Drew Noakes
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

namespace Cassette
{
    /// <summary>
    /// Represents advanced options for reading from a <see cref="IContentAddressableStore"/>.
    /// </summary>
    [Flags]
    public enum ReadOptions
    {
        /// <summary>
        /// Indicates no additional options should be used when reading content from the store.
        /// </summary>
        None = FileOptions.None,

        /// <summary>
        /// Indicates that data will be read sequentially from beginning to end. The system may
        /// use this information to optimise cachine.
        /// </summary>
        SequentialScan = FileOptions.SequentialScan,

        /// <summary>
        /// Indicates that data contents will be accessed randomly. The system may use this
        /// information to optimise cachine.
        /// </summary>
        RandomAccess = FileOptions.RandomAccess,

        /// <summary>
        /// Indicates that content will be read asynchronously via <see cref="Stream.ReadAsync(byte[],int,int)"/>.
        /// </summary>
        Asynchronous = FileOptions.Asynchronous
    }
}