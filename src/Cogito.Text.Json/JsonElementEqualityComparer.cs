using System;
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
            var h = new HashCode();

            switch (obj.ValueKind)
            {
                case JsonValueKind.Undefined:
                    h.Add(JsonValueKind.Undefined);
                    break;
                case JsonValueKind.Null:
                    h.Add(JsonValueKind.Null);
                    break;
                case JsonValueKind.False:
                    h.Add(JsonValueKind.False);
                    break;
                case JsonValueKind.True:
                    h.Add(JsonValueKind.True);
                    break;
                case JsonValueKind.String:
                    h.Add(JsonValueKind.String);
                    h.Add(obj.GetString());
                    break;
                case JsonValueKind.Number when obj.TryGetInt64(out var l):
                    h.Add(JsonValueKind.Number);
                    h.Add(l);
                    break;
                case JsonValueKind.Number when obj.TryGetDouble(out var d):
                    h.Add(JsonValueKind.Number);
                    h.Add(d);
                    break;
                case JsonValueKind.Array:
                    h.Add(JsonValueKind.Array);
                    h.Add(GetHashCodeArray(obj));
                    break;
                case JsonValueKind.Object:
                    h.Add(JsonValueKind.Object);
                    h.Add(GetHashCodeObject(obj));
                    break;
                default:
                    throw new JsonException("Invalid element type.");
            }

            return h.ToHashCode();
        }

        int GetHashCodeArray(JsonElement obj)
        {
            var h = new HashCode();

            // add hash of position + each item
            foreach (var i in obj.EnumerateArray())
                h.Add(GetHashCode(i));

            return h.ToHashCode();
        }

        int GetHashCodeObject(JsonElement obj)
        {
            var h = new HashCode();

            foreach (var i in obj.EnumerateObject().OrderBy(i => i.Name))
            {
                h.Add(i.Name);
                h.Add(GetHashCode(i.Value));
            }

            return h.ToHashCode();
        }

    }

}