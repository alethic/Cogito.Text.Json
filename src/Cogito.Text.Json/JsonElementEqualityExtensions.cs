using System.Text.Json;

namespace Cogito.Text.Json
{

    public static class JsonElementEqualityExtensions
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

        /// <summary>
        /// Returns <c>true</c> if the two elements are equal, ignoring object property order.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool DeepEquals(this JsonElement a, JsonElement b)
        {
            switch (a.ValueKind)
            {
                case JsonValueKind.Undefined:
                    return b.ValueKind == JsonValueKind.Undefined;
                case JsonValueKind.Null:
                    return b.ValueKind == JsonValueKind.Null;
                case JsonValueKind.False:
                    return b.ValueKind == JsonValueKind.False;
                case JsonValueKind.True:
                    return b.ValueKind == JsonValueKind.True;
                case JsonValueKind.String:
                    return b.ValueKind == JsonValueKind.String && a.ValueEquals(b.GetString());
                case JsonValueKind.Number when a.TryGetInt64(out var l):
                    return b.ValueKind == JsonValueKind.Number && b.TryGetInt64(out var l2) && l == l2;
                case JsonValueKind.Number when a.TryGetDouble(out var d):
                    return b.ValueKind == JsonValueKind.Number && b.TryGetDouble(out var d2) && d == d2;
                case JsonValueKind.Array:
                    return b.ValueKind == JsonValueKind.Array && DeepEqualsArray(a, b);
                case JsonValueKind.Object:
                    return b.ValueKind == JsonValueKind.Object && DeepEqualsObject(a, b);
                default:
                    throw new JsonException("Invalid element type.");
            }
        }

        static bool DeepEqualsArray(JsonElement a, JsonElement b)
        {
            var al = a.GetArrayLength();
            var bl = b.GetArrayLength();
            if (al != bl)
                return false;

            for (var i = 0; i < al; i++)
                if (DeepEquals(a[i], b[i]) == false)
                    return false;

            return true;
        }

        static bool DeepEqualsObject(JsonElement a, JsonElement b)
        {
            var al = GetPropertyCount(a);
            var bl = GetPropertyCount(b);

            if (al != bl)
                return false;

            foreach (var ai in a.EnumerateObject())
                if (!(b.TryGetProperty(ai.Name, out var bv) && DeepEquals(ai.Value, bv)))
                    return false;

            return true;
        }

    }

}
