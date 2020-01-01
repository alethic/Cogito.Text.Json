using System;
using System.Collections.Generic;
using System.Linq;

namespace Cogito.Text.Json
{

    /// <summary>
    /// Describes a JSON Pointer as specified in RFC6901.
    /// </summary>
    public readonly ref struct JsonPointer
    {

        /// <summary>
        /// Provides enumeration across the elements of a <see cref="JsonPointer"/>.
        /// </summary>
        public ref struct Enumerator
        {

            readonly JsonPointer parent;
            int offset;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="parent"></param>
            /// <param name="offset"></param>
            public Enumerator(in JsonPointer parent, int offset)
            {
                this.parent = parent;
                this.offset = offset;
            }

            /// <summary>
            /// Gets the current segment of the enumerator.
            /// </summary>
            public JsonPointerSegment Current => offset > -1 ? new JsonPointerSegment(parent, offset) : throw new InvalidOperationException();

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
        /// <param name="span"></param>
        public JsonPointer(ReadOnlySpan<char> span)
        {
            Span = span;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="memory"></param>
        public JsonPointer(ReadOnlyMemory<char> memory) :
            this(memory.Span)
        {

        }

        /// <summary>
        /// Initalizes a new instance.
        /// </summary>
        /// <param name="text"></param>
        public JsonPointer(string text) :
            this(text.AsSpan())
        {

        }

        /// <summary>
        /// Gets the segment at the specified index, or <see cref="JsonPointer.NullSegment"/> if not available.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public JsonPointerSegment this[int index]
        {
            get
            {
                var s = FirstSegment;
                for (int j = 0; j < index - 1; j++)
                    s = s.Next;

                return s;
            }
        }

        /// <summary>
        /// Gets the memory representing the pointer string.
        /// </summary>
        internal ReadOnlySpan<char> Span { get; }

        /// <summary>
        /// Gets the first segment of the pointer.
        /// </summary>
        public JsonPointerSegment FirstSegment => GetFirst();

        /// <summary>
        /// Implements the getter for <see cref="FirstSegment"/>.
        /// </summary>
        /// <returns></returns>
        JsonPointerSegment GetFirst()
        {
            return TryGetNextOffset(-1, out var o) ? new JsonPointerSegment(this, o) : JsonPointerSegment.Null;
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

            foreach (var c in Span)
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
            return Span.Length > offset && Span[offset] == '/';
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
            if (offset >= Span.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (Span[offset] != '/')
                throw new ArgumentException("Segment must begin at forward slash.", nameof(offset));

            // start one character ahead, find '/' or end of string
            var s = Span.Slice(offset + 1);
            var p = s.IndexOf('/');
            return p == -1 ? s.Length + 1 : p + 1;
        }

        /// <summary>
        /// Gets an enumerator over the segments of the JSON pointer.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this, -1);
        }

        /// <summary>
        /// Gets an array of strings formed from the path segments.
        /// </summary>
        /// <returns></returns>
        public string[] ToArray()
        {
            var l = Count;
            var o = new string[l];

            if (o.Length > 0)
            {
                var i = 0;
                var c = FirstSegment;
                while (c != JsonPointerSegment.Null)
                {
                    o[i] = c.ToString();
                    c = c.Next;
                    i++;
                }
            }

            return o;
        }

        /// <summary>
        /// Gets an array of strings formed from the path segments.
        /// </summary>
        /// <returns></returns>
        public IList<string> ToList()
        {
            return ToArray().ToList();
        }

        /// <summary>
        /// Returns <c>true</c> if this <see cref="JsonPointer"/> equals the other <see cref="JsonPointer"/>.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(JsonPointer other)
        {
            if (Span.IsEmpty && other.Span.IsEmpty)
                return true;
            else
                return Span.SequenceEqual(other.Span);
        }

    }

}
