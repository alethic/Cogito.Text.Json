using System.Text.Json;

namespace Cogito.Text.Json
{

    /// <summary>
    /// Provides extensions to <see cref="JsonElement"/>s.
    /// </summary>
    public static class JsonElementExtensions
    {

        /// <summary>
        /// Gets the count of properties present on the element.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int GetPropertyCount(this JsonElement self)
        {
            if (self.ValueKind != JsonValueKind.Object)
                throw new JsonException("Cannot get property count for non-object.");

            var l = 0;
            foreach (var i in self.EnumerateObject())
                l++;

            return l;
        }

    }

}
