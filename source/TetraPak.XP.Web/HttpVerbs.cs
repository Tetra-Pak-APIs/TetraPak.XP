using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;

namespace TetraPak.AspNet
{
    /// <summary>
    ///    Provides <see cref="string"/> constants for standard HTTP methods.
    /// </summary>
    public static class HttpVerbs
    {
        static readonly string[] s_allVerbs = {
            Connect,
            Custom,
            Delete,
            Get,
            Head,
            Options,
            Patch,
            Post,
            Put,
            Trace
        };
        
        /// <summary>
        ///   The HTTP 'CONNECT' method identifier. 
        /// </summary>
        public const string Connect = "CONNECT";

        /// <summary>
        ///   The HTTP 'CUSTOM' method identifier. 
        /// </summary>
        public const string Custom = "CUSTOM";

        /// <summary>
        ///   The HTTP 'DELETE' method identifier. 
        /// </summary>
        public const string Delete = "DELETE";

        /// <summary>
        ///   The HTTP 'GET' method identifier. 
        /// </summary>
        public const string Get = "GET";
        
        /// <summary>
        ///   The HTTP 'HEAD' method identifier. 
        /// </summary>
            public const string Head = "HEAD";
        
        /// <summary>
        ///   The HTTP 'OPTIONS' method identifier. 
        /// </summary>
        public const string Options = "OPTIONS";
        
        /// <summary>
        ///   The HTTP 'PATCH' method identifier. 
        /// </summary>
        public const string Patch = "PATCH";
        
        /// <summary>
        ///   The HTTP 'POST' method identifier. 
        /// </summary>
        public const string Post = "POST";
        
        /// <summary>
        ///   The HTTP 'PUT' method identifier. 
        /// </summary>
        public const string Put = "PUT";
        
        /// <summary>
        ///   The HTTP 'TRACE' method identifier. 
        /// </summary>
        public const string Trace = "TRACE";

        static string[] throwIfUnsupportedVerbs(string[] verbs, string verbsParamName)
        {
            for (var i = 0; i < verbs.Length; i++)
            {
                if (!s_allVerbs.Contains(verbs[i]))
                    throw new ArgumentOutOfRangeException(verbsParamName, $"Unsupported HTTP verb: \"{verbs[i]}\"");
            }

            return verbs;
        }

        /// <summary>
        ///   Examines a collection of <see cref="HttpMethod"/> items and returns it if one or
        ///   more items are found; otherwise returns a collection containing the <see cref="Get"/> HTTP method. 
        /// </summary>
        /// <param name="methods">
        ///   The collection of <see cref="HttpMethod"/> items to be examined.
        /// </param>
        /// <returns>
        ///   A <see cref="string"/> array containing the resulting HTTP verbs.
        /// </returns>
        public static string[] DefaultToGetVerb(this HttpMethod[]? methods) => methods.DefaultToVerbs(Get);

        /// <summary>
        ///   Examines a collection of <see cref="HttpMethod"/> items and returns it if one or
        ///   more items are found; otherwise returns a specified default collection of HTTP verbs. 
        /// </summary>
        /// <param name="methods">
        ///   The collection of <see cref="HttpMethod"/> items to be examined.
        /// </param>
        /// <param name="defaultVerbs">
        ///   One or more default verbs to be returned if no <see cref="HttpMethod"/> are assigned
        ///   (<paramref name="methods"/> is <c>null</c> or empty).
        /// </param>
        /// <returns>
        ///   A <see cref="string"/> array containing the resulting HTTP verbs.
        /// </returns>
        public static string[] DefaultToVerbs(this HttpMethod[]? methods, params string[] defaultVerbs)
        {
            #if DEBUG
            if (!defaultVerbs.Any())
                throw new ArgumentNullException(nameof(defaultVerbs), "Expected at least one default HTTP verb");
                
            throwIfUnsupportedVerbs(defaultVerbs, nameof(defaultVerbs));
            #endif
            
            return methods?.Any() ?? false
                ? methods.ToStringVerbs()
                : throwIfUnsupportedVerbs(defaultVerbs, nameof(defaultVerbs));
        }
    }
}