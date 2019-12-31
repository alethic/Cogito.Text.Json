using System;
using System.Collections;
using System.Collections.Generic;

namespace Cogito.Text.Json
{

    /// <summary>
    /// Describes a JSON Pointer as specified in RFC6901.
    /// </summary>
    public readonly struct JsonPointer : IEnumerable<JsonPointerSegment>
    {

        /// <summary>
        /// Provides enumeration across the elements of a <see cref="JsonPointer"/>.
        /// </summary>
        public struct JsonPointerSegmentEnumerator : IEnumerator<JsonPointerSegment>
        {

            readonly JsonPointer parent;
            int offset;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="parent"></param>
            /// <param name="offset"></param>
            public JsonPointerSegmentEnumerator(JsonPointer parent, int offset)
            {
                this.parent = parent;
                this.offset = offset;
            }

            /// <summary>
            /// Gets the current segment of the enumerator.
            /// </summary>
            public JsonPointerSegment Current => offset > -1 ? new JsonPointerSegment(parent, offset) : throw new InvalidOperationException();

            /// <summary>
            /// Gets the current segment of the enumerator.
            /// </summary>
            object IEnumerator.Current => Current;

            /// <summary>
            /// Advances to the next segment of the enumerator.
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                return parent.TryGetNextOffset(offset, out offset);
            }

            /// <summary>
            /// Resets the enumerator.
            /// </summary>
            public void Reset()
            {
                offset = -1;
            }

            /// <summary>
            /// Disposes of the instance.
            /// </summary>
            public void Dispose()
            {

            }

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="memory"></param>
        public JsonPointer(ReadOnlyMemory<char> memory)
        {
            Memory = memory;
        }

        /// <summary>
        /// Initalizes a new instance.
        /// </summary>
        /// <param name="text"></param>
        public JsonPointer(string text) :
            this(text.AsMemory())
        {

        }

        /// <summary>
        /// Gets the memory representing the pointer string.
        /// </summary>
        internal ReadOnlyMemory<char> Memory { get; }

        /// <summary>
        /// Gets the first segment of the pointer.
        /// </summary>
        public JsonPointerSegment? First => GetFirst();

        /// <summary>
        /// Implements the getter for <see cref="First"/>.
        /// </summary>
        /// <returns></returns>
        JsonPointerSegment? GetFirst()
        {
            return TryGetNextOffset(-1, out var o) ? new JsonPointerSegment(this, o) : (JsonPointerSegment?)null;
        }

        /// <summary>
        /// Gets the number of segments within the pointer.
        /// </summary>
        public int Count => GetCount();

        /// <summary>
        /// Implements the getter for <see cref="Count"/>.
        /// </summary>
        /// <returns></returns>
        int GetCount()
        {
            // essentially just a count of '/' characters

            var i = 0;

            foreach (var c in Memory.Span)
                if (c == '/')
                    i++;

            return i;
        }

        /// <summary>
        /// Tries to advance the offset to the next segment.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        internal bool TryGetNextOffset(int current, out int offset)
        {
            offset = current;
            offset += offset == -1 ? 1 : GetSegmentLength(offset);
            return Memory.Length > offset && Memory.Span[offset] == '/';
        }

        /// <summary>
        /// Gets the length of the segment beginning at the specified offset.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        internal int GetSegmentLength(int offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (offset >= Memory.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (Memory.Span[offset] != '/')
                throw new ArgumentException("Segment must begin at forward slash.", nameof(offset));

            // start one character ahead, find '/' or end of string
            var s = Memory.Slice(offset + 1);
            var p = s.Span.IndexOf('/');
            return p == -1 ? s.Length + 1 : p + 1;
        }

        /// <summary>
        /// Gets an enumerator over the segments of the JSON pointer.
        /// </summary>
        /// <returns></returns>
        public JsonPointerSegmentEnumerator GetEnumerator()
        {
            return new JsonPointerSegmentEnumerator(this, -1);
        }

        IEnumerator<JsonPointerSegment> IEnumerable<JsonPointerSegment>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
