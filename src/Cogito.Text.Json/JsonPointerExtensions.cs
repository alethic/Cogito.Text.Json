using System;
using System.Buffers;
using System.Text.Json;

namespace Cogito.Text.Json
{

    public static class JsonPointerExtensions
    {

        /// <summary>
        /// Selects a <see cref="JsonElement"/> using JavaScript Object Notation Pointer syntax from this element as a root.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static JsonElement? SelectPointer(this JsonElement element, string text)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            return SelectPointer(element, new JsonPointer(text));
        }

        /// <summary>
        /// Selects a <see cref="JsonElement"/> using JavaScript Object Notation Pointer syntax from this element as a root.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static JsonElement? SelectPointer(JsonElement element, ReadOnlySpan<char> text)
        {
            return SelectPointer(element, new JsonPointer(text));
        }

        /// <summary>
        /// Selects a <see cref="JsonElement"/> using JavaScript Object Notation Pointer syntax from this element as a root.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static JsonElement? SelectPointer(this JsonElement element, ReadOnlyMemory<char> buffer)
        {
            return SelectPointer(element, new JsonPointer(buffer));
        }

        /// <summary>
        /// Selects a <see cref="JsonElement"/> using JavaScript Object Notation Pointer syntax from this element as a root.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public static JsonElement? SelectPointer(this JsonElement element, JsonPointer pointer)
        {
            foreach (var segment in pointer)
            {
                if (SelectSegment(element, segment) is JsonElement e)
                    element = e;
                else
                    return null;
            }

            return element;
        }

        /// <summary>
        /// Returns the element pointed to by the specified segment.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
        static JsonElement? SelectSegment(JsonElement element, JsonPointerSegment segment)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    return SelectObjectFormat(element, segment);
                case JsonValueKind.Array:
                    return SelectArrayFormat(element, segment);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Evaluates a segment against an object.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
        static JsonElement? SelectObjectFormat(JsonElement element, JsonPointerSegment segment)
        {
            return element.TryGetProperty(segment.Span.Slice(1), out var i) == false ? null : (JsonElement?)i;
        }

        /// <summary>
        /// Evaluates a segment against an array.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
        static JsonElement? SelectArrayFormat(JsonElement element, JsonPointerSegment segment)
        {
            return ParseInt32(segment.Span.Slice(1), out var i) && i < element.GetArrayLength() ? (JsonElement?)element[i] : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="span"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        static bool ParseInt32(ReadOnlySpan<char> span, out int result)
        {
#if NETSTANDARD2_1
            return int.TryParse(span, out result);
#else
            return int.TryParse(span.ToString(), out result);
#endif
        }

    }

}
