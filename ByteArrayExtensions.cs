using System;

namespace SeratoCrateWrapper
{
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Searches for the specified pattern and returns the index of its first occurrence in a one-dimensional array or in a range of elements in the array.
        /// </summary>
        /// <param name="bytes">The one-dimensional array to search.</param>
        /// <param name="pattern">The pattern to locate in "bytes".</param>
        /// <param name="start">The starting index of the search.</param>
        /// <returns>The index of the first occurrence of value, if it's found, within the range of elements in array that extends from startIndex to the last element; otherwise -1.</returns>
        public static int IndexOf(this byte[] bytes, byte[] pattern, int start = 0)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), $"{nameof(start)} has to be positive.");

            if (start > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(start), $"{nameof(start)} has to be smaller than the length of bytes.");

            if (pattern.Length > bytes.Length - start)
                return -1;

            var count = 0;
            for (int i = start; i < bytes.Length - pattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (bytes[i + j] != pattern[j])
                    {
                        found = false;
                        i += j;
                        break;
                    }
                    count++;
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns a portion of the array from start to end index.
        /// </summary>
        /// <param name="bytes">The array.</param>
        /// <param name="start">The start index<./param>
        /// <param name="end">The end index.</param>
        /// <returns>The poriton of the provided array.</returns>
        public static byte[] Slice(this byte[] bytes, int start, int end)
        {
            var length = end - start;

            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start), $"{nameof(start)} has to be positive.");

            if (end < start)
                throw new ArgumentOutOfRangeException(nameof(end), $"{nameof(end)} has to be greater than {nameof(start)}.");

            if (start > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(start), $"{nameof(start)} has to be smaller than the length of bytes.");

            if (end > bytes.Length)
                throw new ArgumentOutOfRangeException(nameof(end), $"{nameof(end)} has to be smaller than the length of bytes.");

            var returnArray = new byte[length];

            for (int i = 0; i < length; i++)
            {
                returnArray[i] = bytes[i + start];
            }

            return returnArray;
        }
    }
}
