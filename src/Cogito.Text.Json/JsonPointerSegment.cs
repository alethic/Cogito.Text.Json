using System;

namespace Cogito.Text.Json
{

    /// <summary>
    /// Represents a segment of a JSON pointer path.
    /// </summary>
    public readonly struct JsonPointerSegment
    {

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
        public int Offset => offset;

        /// <summary>
        /// Length of this segment.
        /// </summary>
        public int Length => parent.GetSegmentLength(offset);

        /// <summary>
        /// Gets a memory that covers the contents of this segment.
        /// </summary>
        public ReadOnlyMemory<char> Memory => parent.Memory.Slice(offset, Length);

        /// <summary>
        /// Gets a span that covers the contents of this segment.
        /// </summary>
        public ReadOnlySpan<char> Span => Memory.Span;

        /// <summary>
        /// Tries to get the next pointer segment from the given segmet.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public bool TryGetNext(out JsonPointerSegment next)
        {
            var o = offset;
            if (parent.TryGetNextOffset(ref o))
            {
                next = new JsonPointerSegment(parent, o);
                return true;
            }
            else
            {
                next = new JsonPointerSegment(parent, int.MaxValue);
                return false;
            }
        }

        /// <summary>
        /// Gets the string representation of this segment.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return new string(Memory.ToArray());
        }

    }

}
