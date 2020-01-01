using System;
using System.Text.Json;

namespace Cogito.Text.Json
{

    /// <summary>
    /// Provides JSON pointer functionality against a <see cref="JsonElement"/>.
    /// </summary>
    public class JsonElementPointerNavigator : IJsonPointerNavigator
    {

        readonly JsonElement element;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="element"></param>
        public JsonElementPointerNavigator(JsonElement element)
        {
            this.element = element;
        }

        /// <summary>
        /// Gets the element referenced by this navigator.
        /// </summary>
        public JsonElement Element => element;

        /// <summary>
        /// Selects into the current element by pointer.
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public IJsonPointerNavigator Select(JsonPointer pointer)
        {
            return SelectPointer(element, pointer) is JsonElement e ? new JsonElementPointerNavigator(e) : null;
        }

        /// <summary>
        /// Selects into the specified element by pointer. Does not require object allocation.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public static JsonElement? Select(JsonElement element, JsonPointer pointer)
        {
            return SelectPointer(element, pointer);
        }

        /// <summary>
        /// Selects a <see cref="JsonElement"/> using JavaScript Object Notation Pointer syntax from this element as a root.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="pointer"></param>
        /// <returns></returns>
        static JsonElement? SelectPointer(JsonElement element, JsonPointer pointer)
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
        /// <param name="segment"></param>
        /// <returns></returns>
        static JsonElement? SelectObjectFormat(JsonElement element, JsonPointerSegment segment)
        {
            return element.TryGetProperty(segment.Span.Slice(1), out var i) == false ? null : (JsonElement?)i;
        }

        /// <summary>
        /// Evaluates a segment against an array.
        /// </summary>
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
