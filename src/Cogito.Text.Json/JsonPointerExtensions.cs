using System;
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
        /// <param name="span"></param>
        /// <returns></returns>
        public static JsonElement? SelectPointer(JsonElement element, ReadOnlySpan<char> span)
        {
            return SelectPointer(element, new JsonPointer(span));
        }

        /// <summary>
        /// Selects a <see cref="JsonElement"/> using JavaScript Object Notation Pointer syntax from this element as a root.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="memory"></param>
        /// <returns></returns>
        public static JsonElement? SelectPointer(this JsonElement element, ReadOnlyMemory<char> memory)
        {
            return SelectPointer(element, new JsonPointer(memory));
        }

        /// <summary>
        /// Selects a <see cref="JsonElement"/> using JavaScript Object Notation Pointer syntax from this element as a root.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public static JsonElement? SelectPointer(this JsonElement element, JsonPointer pointer)
        {
            return JsonElementPointerNavigator.Select(element, pointer);
        }

    }

}
