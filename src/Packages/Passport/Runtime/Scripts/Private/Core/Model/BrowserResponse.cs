using System;
using Immutable.Passport.Json;
using UnityEngine.Scripting;

namespace Immutable.Passport.Core
{
    [Preserve]
    public class BrowserResponse
    {
        public string responseFor;
        public string requestId;
        public bool success;
        public string errorType;
        public string error;
    }

    [Preserve]
    public class StringResponse : BrowserResponse
    {
        public string result;
    }

    [Preserve]
    public class BoolResponse : BrowserResponse
    {
        public bool result;
    }

    [Preserve]
    public static class BrowserResponseExtensions
    {
        /// <summary>
        /// Deserialises the json to StringResponse and returns the result
        /// See <see cref="Immutable.Passport.Core.BrowserResponse.StringResponse"></param>
        /// </summary>
        public static string GetStringResult(this string json)
        {
            StringResponse stringResponse = json.OptDeserializeObject<StringResponse>();
            if (stringResponse != null)
            {
                return stringResponse.result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Deserialises the json to BoolResponse and returns the result
        /// See <see cref="Immutable.Passport.Core.BrowserResponse.BoolResponse"></param>
        /// </summary>
        public static Nullable<bool> GetBoolResponse(this string json)
        {
            BoolResponse boolResponse = json.OptDeserializeObject<BoolResponse>();
            if (boolResponse != null)
            {
                return boolResponse.result;
            }
            else
            {
                return null;
            }
        }
    }
}
