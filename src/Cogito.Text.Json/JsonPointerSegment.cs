using System;

namespace Cogito.Text.Json
{

    /// <summary>
    /// Represents a segment of a JSON pointer path.
    /// </summary>
    public readonly ref struct JsonPointerSegment
    {

        /// <summary>
        /// Gets the segment representing null.
        /// </summary>
        public static JsonPointerSegment Null => new JsonPointerSegment(new JsonPointer(ReadOnlySpan<char>.Empty), -1);

        /// <summary>
        /// Returns <c>true</c> if the two objects are equal.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(JsonPointerSegment a, JsonPointerSegment b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Returns <c>true</c> if the two objects are not equal.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(JsonPointerSegment a, JsonPointerSegment b)
        {
            return !(a == b);
        }

        readonly JsonPointer parent;
        readonly int offset;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="offset"></param>
        internal JsonPointerSegment(JsonPointer parent, int offset)
        {
            this.parent = parent;
            this.offset = offset;
        }

        /// <summary>
        /// Number of characters this segment is offset from the beginning of the pointer.
        /// </summary>
        public readonly int Offset => offset > -1 ? offset : throw new JsonPointerException("Null segment cannot be accessed.");

        /// <summary>
        /// Length of this segment.
        /// </summary>
        public readonly int Length => offset > -1 ? parent.GetSegmentLength(offset) : throw new JsonPointerException("Null segment cannot be accessed.");

        /// <summary>
        /// Gets a span that covers the contents of this segment.
        /// </summary>
        public readonly ReadOnlySpan<char> Span => DecodeSpan(parent.Span.Slice(offset, Length));

        /// <summary>
        /// Potentially decodes the data within the span.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        ReadOnlySpan<char> DecodeSpan(ReadOnlySpan<char> source)
        {
            if (source.Contains("~".AsSpan(), StringComparison.Ordinal) == false)
                return source;
            else
                return source.ToString().Replace("~1", "/").Replace("~0", "~").AsSpan();
        }

        /// <summary>
        /// Gets the next segment.
        /// </summary>
        public readonly JsonPointerSegment Next => parent.TryGetNextOffset(offset, out var o) ? new JsonPointerSegment(parent, o) : JsonPointerSegment.Null;

        /// <summary>
        /// Gets the string representation of this segment.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // null segment returns null string
            if (offset == -1)
                return null;

#if NETSTANDARD2_1
            return new string(Span);
#else
            return new string(Span.ToArray());
#endif
        }

        /// <summary>
        /// Returns <c>true</c> if the two segments are equal.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(JsonPointerSegment other)
        {
            return parent.Equals(other.parent) && offset.Equals(other.offset);
        }

        public override bool Equals(object obj) => throw new NotImplementedException();

        public override int GetHashCode() => throw new NotImplementedException();

    }

}
