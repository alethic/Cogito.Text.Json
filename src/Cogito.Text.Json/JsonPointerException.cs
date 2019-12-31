using System;

namespace Cogito.Text.Json
{

    public class JsonPointerException : Exception
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public JsonPointerException()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="message"></param>
        public JsonPointerException(string message) :
            base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public JsonPointerException(string message, Exception innerException) :
            base(message, innerException)
        {

        }

    }

}
