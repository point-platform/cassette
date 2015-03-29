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
        Asynchronous = FileOptions.Asynchronous,
    }
}