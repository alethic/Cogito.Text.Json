using System.Text.Json;

namespace Cogito.Text.Json
{

    /// <summary>
    /// Provides extensions for <see cref="JsonElement"/> items relating to value equality.
    /// </summary>
    public static class JsonElementEqualityExtensions
    {

        /// <summary>
        /// Determines whether the two objects are completely equal.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool DeepEquals(this JsonElement self, JsonElement other)
        {
            return JsonElementEqualityComparer.Default.Equals(self, other);
        }

        /// <summary>
        /// Returns the hash code for the value of the instance.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int GetDeepHashCode(this JsonElement self)
        {
            return JsonElementEqualityComparer.Default.GetHashCode(self);
        }

    }

}
