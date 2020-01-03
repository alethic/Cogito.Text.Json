using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Cogito.Text.Json
{

    /// <summary>
    /// Defines methods to support the comparison of <see cref="JsonElement"/> items for equality.
    /// </summary>
    public class JsonElementEqualityComparer : IEqualityComparer<JsonElement>
    {

        const int H = 31;

        public static readonly JsonElementEqualityComparer Default = new JsonElementEqualityComparer();

        /// <summary>
        /// Returns <c>true</c> if the two elements are equal, ignoring object property order.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static bool DeepEquals(JsonElement a, JsonElement b)
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
            var al = a.GetPropertyCount();
            var bl = b.GetPropertyCount();

            if (al != bl)
                return false;

            foreach (var ai in a.EnumerateObject())
                if (!(b.TryGetProperty(ai.Name, out var bv) && DeepEquals(ai.Value, bv)))
                    return false;

            return true;
        }

        /// <summary>
        /// Returns <c>true</c> if the two elements are equal.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(JsonElement x, JsonElement y)
        {
            return DeepEquals(x, y);
        }

        /// <summary>
        /// Returns the hashcode for the value of the instance.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(JsonElement obj)
        {
            var h = 17;

            switch (obj.ValueKind)
            {
                case JsonValueKind.Undefined:
                    h = h * H + JsonValueKind.Undefined.GetHashCode();
                    break;
                case JsonValueKind.Null:
                    h = h * H + JsonValueKind.Null.GetHashCode();
                    break;
                case JsonValueKind.False:
                    h = h * H + JsonValueKind.False.GetHashCode();
                    break;
                case JsonValueKind.True:
                    h = h * H + JsonValueKind.True.GetHashCode();
                    break;
                case JsonValueKind.String:
                    h = h * H + JsonValueKind.String.GetHashCode();
                    h = h * H + obj.GetString().GetHashCode();
                    break;
                case JsonValueKind.Number when obj.TryGetInt64(out var l):
                    h = h * H + JsonValueKind.Number.GetHashCode();
                    h = h * H + l.GetHashCode();
                    break;
                case JsonValueKind.Number when obj.TryGetDouble(out var d):
                    h = h * H + JsonValueKind.Number.GetHashCode();
                    h = h * H + d.GetHashCode();
                    break;
                case JsonValueKind.Array:
                    h = h * H + JsonValueKind.Array.GetHashCode();
                    h = h * H + GetHashCodeArray(obj);
                    break;
                case JsonValueKind.Object:
                    h = h * H + JsonValueKind.Object.GetHashCode();
                    h = h * H + GetHashCodeObject(obj);
                    break;
                default:
                    throw new JsonException("Invalid element type.");
            }

            return h;
        }

        int GetHashCodeArray(JsonElement obj)
        {
            var h = 17;
            var p = 0;

            // add hash of position + each item
            foreach (var i in obj.EnumerateArray())
            {
                h = h * H + p++.GetHashCode();
                h = h * H + GetHashCode(i);
            }

            // trail with hash of total count
            h = h * H + p.GetHashCode();

            return h;
        }

        int GetHashCodeObject(JsonElement obj)
        {
            var h = 17;
            var p = 0;

            foreach (var i in obj.EnumerateObject().OrderBy(i => i.Name))
            {
                h = h * H + p++.GetHashCode();
                h = h * H + i.Name.GetHashCode();
                h = h * H + GetHashCode(i.Value);
            }

            h = h * H + p.GetHashCode();

            return h;
        }

    }

}