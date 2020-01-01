namespace Cogito.Text.Json
{

    /// <summary>
    /// Provides the capability of navigating through JSON documents by pointer segment.
    /// </summary>
    public interface IJsonPointerNavigator
    {

        /// <summary>
        /// Selects the result given the specified <see cref="JsonPointer"/>.
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        IJsonPointerNavigator Select(JsonPointer pointer);

    }

}