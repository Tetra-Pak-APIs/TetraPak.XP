using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.StringValues;
using TetraPak.XP.Web.Abstractions;
using HttpMethod = Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpMethod;

namespace TetraPak.XP.Web.Http
{
    /// <summary>
    ///   Provides extension and convenience method for <see cref="HttpContext"/>.
    /// </summary>
    public static class HttpContextHelper
    {
        static readonly IDictionary<HttpMethod, string> s_httpMethodToStringVerbMap = new Dictionary<HttpMethod, string>
        {
            [HttpMethod.Connect] = HttpVerbs.Connect,
            [HttpMethod.Custom] = HttpVerbs.Custom,
            [HttpMethod.Delete] = HttpVerbs.Delete,
            [HttpMethod.Get] = HttpVerbs.Get,
            [HttpMethod.Head] = HttpVerbs.Head,
            [HttpMethod.Options] = HttpVerbs.Options,
            [HttpMethod.Patch] = HttpVerbs.Patch,
            [HttpMethod.Post] = HttpVerbs.Post,
            [HttpMethod.Put] = HttpVerbs.Put,
            [HttpMethod.Trace] = HttpVerbs.Trace
        };

        static readonly IDictionary<string, HttpMethod> s_httpStringVerbToMethodMap = s_httpMethodToStringVerbMap.ToInverted();

        /// <summary>
        ///   Gets a standardized value used for referencing a unique request. 
        /// </summary>
        /// <param name="request">
        ///   The <see cref="HttpRequest"/>.
        /// </param>
        /// <param name="tetraPakConfig">
        ///   Carries the Tetra Pak authorization configuration.
        /// </param>
        /// <param name="enforce">
        ///   (optional; default=<c>false</c>)<br/>
        ///   When set, a random unique string will be generated and attached to the request.  
        /// </param>
        /// <returns>
        ///   A unique <see cref="string"/> value. 
        /// </returns>
        public static LogMessageId? GetMessageId(
            this HttpRequest request,
            ITetraPakConfiguration? tetraPakConfig,
            bool enforce = false)
        {
            var key = tetraPakConfig?.MessageIdHeader ?? Headers.MessageId;
            var value = request.Headers.GetSingleValue(key, enforce ? new RandomString() : null, enforce);
            return value is { }
                ? (LogMessageId)value
                : null;
        }
        
        /// <summary>
        ///   Gets a standardized value used for referencing a unique request. 
        /// </summary>
        /// <param name="headers">
        ///   The collection of request/response headers.
        /// </param>
        /// <param name="tetraPakConfig">
        ///   Carries the Tetra Pak authorization configuration.
        /// </param>
        /// <param name="enforce">
        ///   (optional; default=<c>false</c>)<br/>
        ///   When set, a random unique string will be generated and attached to the request.  
        /// </param>
        /// <returns>
        ///   A unique <see cref="string"/> value. 
        /// </returns>
        public static LogMessageId? GetMessageId(
            this HttpHeaders headers,
            ITetraPakConfiguration? tetraPakConfig,
            bool enforce = false)
        {
            var key = tetraPakConfig?.MessageIdHeader ?? Headers.MessageId;
            var value = headers.GetSingleValue(key, enforce ? new RandomString() : null, enforce);
            return value is { }
                ? (LogMessageId)value
                : null;
        }
        
        public static LogMessageId? GetMessageId(
            this HttpClient client,
            ITetraPakConfiguration? tetraPakConfig,
            bool enforce = false)
        {
            return client.DefaultRequestHeaders.GetMessageId(tetraPakConfig, enforce);
        }
        
        /// <summary>
        ///   Gets (and, optionally, sets) a single header value.
        /// </summary>
        /// <param name="dictionary">
        ///   The header dictionary to get (set) value from.
        /// </param>
        /// <param name="key">
        ///   Identifies the header value.
        /// </param>
        /// <param name="useDefault">
        ///   (optional)<br/>
        ///   A default value to be used if one cannot be found in the header dictionary.
        /// </param>
        /// <param name="setDefault">
        ///   (optional; default=<c>false</c>); only applies if <paramref name="useDefault"/> is assigned)<br/>
        ///   When set, the <paramref name="useDefault"/> value will automatically be added to the header dictionary,
        ///   affecting the request.
        /// </param>
        /// <returns>
        ///   A (single) <see cref="string"/> value.
        /// </returns>
        public static string? GetSingleValue(
            this IHeaderDictionary dictionary,
            string key,
            string? useDefault,
            bool setDefault = false)
        {
            if (dictionary.TryGetValue(key, out var values))
                return values.First();

            if (string.IsNullOrWhiteSpace(useDefault))
                return null;

            if (setDefault)
            {
                dictionary.Add(key, useDefault);
            }

            return useDefault;
        }
        
        public static string? GetSingleValue(
            this HttpHeaders headers,
            string key,
            string? useDefault,
            bool setDefault = false)
        {
            if (headers.TryGetValues(key, out var values))
                return values.First();

            if (string.IsNullOrWhiteSpace(useDefault))
                return null;

            if (setDefault)
            {
                headers.TryAddWithoutValidation(key, useDefault);
            }

            return useDefault;
        }

        // /// <summary>
        // ///   Returns the request access token, or <c>null</c> if unavailable. 
        // /// </summary>
        // /// <returns>
        // ///   An <see cref="ActorToken"/> instance representing the request's access token if one can be obtained;
        // ///   otherwise <c>null</c>.
        // /// </returns>
        // /// <seealso cref="GetIdentityToken(Microsoft.AspNetCore.Http.HttpContext,TetraPakConfig)"/>
        // public static ActorToken? GetIdentityToken(this HttpRequest self, TetraPakConfig config) 
        //     => self.HttpContext.GetIdentityToken(config);

        // /// <summary>
        // ///   Returns the request identity token, or <c>null</c> if unavailable.
        // /// </summary>
        // /// <param name="self">
        // ///   The request <see cref="HttpContext"/> object.
        // /// </param>
        // /// <param name="config">
        // ///   (optional)<br/>
        // ///   The Tetra Pak integration configuration object. When passed the method will look
        // ///   for the identity token in the header specified by <see cref="TetraPakConfig.AuthorizationHeader"/>.
        // ///   If not the identity token is assumed to be carried by the header named as <see cref="AmbientData.Keys.IdToken"/>.
        // /// </param>
        // /// <returns>
        // ///   An <see cref="ActorToken"/> object representing the request's identity token if one can be obtained;
        // ///   otherwise <c>null</c>.
        // /// </returns>
        // /// <seealso cref="GetIdentityToken(Microsoft.AspNetCore.Http.HttpRequest,TetraPakConfig)"/>
        // public static ActorToken? GetIdentityToken(this HttpContext self, TetraPakConfig? config = null)
        // {
        //     var task = GetIdentityTokenAsync(self, config);
        //     var outcome = task.ConfigureAwait(false).GetAwaiter().GetResult();
        //     return outcome
        //         ? outcome.Value
        //         : null;
        // }
        
        // /// <summary>
        // ///   Asynchronously returns the request identity token, or <c>null</c> if unavailable.
        // /// </summary>
        // /// <param name="self">
        // ///   The request <see cref="HttpContext"/> object.
        // /// </param>
        // /// <param name="authConfig">
        // ///   (optional)<br/>
        // ///   The Tetra Pak integration configuration object. When passed the method will look
        // ///   for the identity token in the header specified by <see cref="TetraPakConfig.AuthorizationHeader"/>.
        // ///   If not the identity token is assumed to be carried by the header named as <see cref="AmbientData.Keys.IdToken"/>.
        // /// </param>
        // /// <returns>
        // ///   An <see cref="ActorToken"/> object representing the request's identity token if one can be obtained;
        // ///   otherwise <c>null</c>.
        // /// </returns>
        // public static Task<Outcome<ActorToken>> GetIdentityTokenAsync(this HttpContext self, TetraPakConfig? authConfig = null)
        // {
        //     if (self.Items.TryGetValue(AmbientData.Keys.IdToken, out var obj) && obj is string s 
        //             && ActorToken.TryParse(s, out var actorToken))
        //         return Task.FromResult(Outcome<ActorToken>.Success(actorToken));
        //     
        //     var headerIdent = authConfig?.AuthorizationHeader ?? AmbientData.Keys.IdToken;
        //     s = self.Request.Headers[headerIdent].ToString();
        //     if (s is {} && ActorToken.TryParse(s, out actorToken))
        //         return Task.FromResult(Outcome<ActorToken>.Success(actorToken));
        //
        //     return Task.FromResult(Outcome<ActorToken>.Fail(new Exception("Id token not found")));
        // }
        
        // /// <summary>
        // ///   Gets all tokens from an <see cref="HttpContext"/>.
        // /// </summary>
        // /// <param name="self">
        // ///   The <see cref="HttpContext"/>.
        // /// </param>
        // /// <returns>
        // ///   
        // /// </returns>
        // public static async Task<EnumOutcome<ActorToken>> GetActorTokensAsync(this HttpContext? self)
        // {
        //     if (self is null)
        //         return EnumOutcome<ActorToken>.Fail(new Exception("No HTTP context available"));
        //         
        //     var values = self.Request.Headers[HeaderNames.Authorization];
        //     if (!values.Any())
        //     {
        //         // the context is still in auth flow; use different mechanism ...
        //         var tokenList = new List<ActorToken>();
        //         var accessTokenOutcome = await self.GetAccessTokenAsync();
        //         if (accessTokenOutcome)
        //         {
        //             tokenList.Add(accessTokenOutcome.Value!);
        //         }
        //         var idTokenOutcome = await self.GetIdentityTokenAsync();
        //         if (idTokenOutcome)
        //         {
        //             tokenList.Add(idTokenOutcome.Value!);
        //         }
        //         return tokenList.Any()
        //             ? EnumOutcome<ActorToken>.Success(tokenList.ToArray())
        //             : EnumOutcome<ActorToken>.Fail(new Exception("Tokens not found"));
        //     }
        //
        //     var list = new List<ActorToken>();
        //     foreach (var stringValue in values)
        //     {
        //         if (ActorToken.TryParse(stringValue, out var token))
        //             list.Add(token);
        //     }
        //     return EnumOutcome<ActorToken>.Success(list.ToArray());
        // }

        // /// <summary> todo
        // ///   Gets a telemetry level from the request (if any).
        // /// </summary>
        // /// <param name="request">
        // ///   The <see cref="HttpRequest"/>.
        // /// </param>
        // /// <param name="logger">
        // ///   A logger provider.
        // /// </param>
        // /// <param name="useDefault">
        // ///   A default telemetry level to be returned when no level was specified, or when 
        // ///   the specified telemetry level could not be successfully parsed.  
        // /// </param>
        // /// <returns>
        // ///   A <see cref="ServiceDiagnosticsLevel"/> value.
        // /// </returns>
        // public static ServiceDiagnosticsLevel GetDiagnosticsLevel(
        //     this HttpRequest request,
        //     ILog? logger,
        //     ServiceDiagnosticsLevel useDefault = ServiceDiagnosticsLevel.None)
        // {
        //     if (!request.Headers.TryGetValue(Headers.ServiceDiagnostics, out var values))
        //         return useDefault;
        //
        //     var value = values.First();
        //     if (Enum.TryParse<ServiceDiagnosticsLevel>(values, true, out var level)) 
        //         return level;
        //     
        //     logger.Warning($"Unknown telemetry level requested: '{value}'");
        //     return useDefault;
        // }

        /// <summary>
        ///   Sets a value to the <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="self"> 
        ///   The <see cref="HttpContext"/>.
        /// </param>
        /// <param name="key">
        ///   Identifies the value to be set.
        /// </param>
        /// <param name="value">
        ///   The value to be set.
        /// </param>
        public static void SetValue(this HttpContext self, string key, object value) => self.Items[key] = value; 

        /// <summary>
        ///   Gets a value from <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="self">
        ///   The <see cref="HttpContext"/>.
        /// </param>
        /// <param name="key">
        ///   Identifies the requested value.
        /// </param>
        /// <param name="useDefault">
        ///   (optional)<br/>
        ///   A default value to be returned if the requested value is not carried by <paramref name="self"/>. 
        /// </param>
        /// <typeparam name="T">
        ///   The type of value requested.
        /// </typeparam>
        /// <returns>
        ///   The requested value if carried by the <see cref="HttpContext"/>;
        ///   otherwise the <paramref name="useDefault"/> value.
        /// </returns>
        public static T GetValue<T>(this HttpContext self, string key, T? useDefault = default)
            => self.Items.TryGetValue(key, out var obj) && obj is T tValue ? tValue : useDefault!; 

        // /// <summary>
        // ///   Writes a HTTP response.
        // /// </summary>
        // /// <param name="context">
        // ///   The <see cref="HttpContext"/>.
        // /// </param>
        // /// <param name="statusCode">
        // ///   The status code to be returned.
        // /// </param>
        // /// <param name="content">
        // ///   (optional)<br/>
        // ///   Content to be returned (objects will be JSON serialized).
        // /// </param>
        // /// <param name="cancellationToken">
        // ///   (optional)<br/>
        // ///   A cancellation token.
        // /// </param>
        // public static Task RespondAsync(this HttpContext context, 
        //     HttpStatusCode statusCode, 
        //     object? content = null,
        //     CancellationToken cancellationToken = default)
        // {
        //     string? contentType = null;
        //     string? stringContent;
        //     if (content is string stringValue)
        //     {
        //         stringContent = stringValue;
        //     }
        //     else
        //     {
        //         stringContent = content?.ToJson();
        //         contentType = stringContent is { } ? "application/json" : null;
        //     }
        //
        //     return context.RespondAsync(statusCode, stringContent, contentType, cancellationToken);
        // }

        // /// <summary>
        // ///   Writes a HTTP response.
        // /// </summary>
        // /// <param name="context">
        // ///   The <see cref="HttpContext"/>.
        // /// </param>
        // /// <param name="statusCode">
        // ///   The status code to be returned.
        // /// </param>
        // /// <param name="content">
        // ///   (optional)<br/>
        // ///   Content to be returned.
        // /// </param>
        // /// <param name="contentType">
        // ///   (optional)<br/>
        // ///   A content MIME type tp be returned.
        // /// </param>
        // /// <param name="cancellationToken">
        // ///   (optional)<br/>
        // ///   A cancellation token.
        // /// </param>
        // public static async Task RespondAsync(this HttpContext context,
        //     HttpStatusCode statusCode, 
        //     string? content = null,
        //     string? contentType = null, 
        //     CancellationToken cancellationToken = default)
        // {
        //     context.Response.StatusCode = (int) statusCode;
        //     if (content is null)
        //         return;
        //
        //     if (contentType is null)
        //     {
        //         if (isProbablyJson())
        //         {
        //             contentType = "application/json";
        //         }
        //     }
        //     context.Response.ContentType = contentType ?? "";
        //     await context.Response.WriteAsync(content, cancellationToken);
        //
        //     bool isProbablyJson()
        //     {
        //         if (content.Length < 2)
        //             return false;
        //
        //         if (content.StartsWith('{') && content.EndsWith('}'))
        //             return true;
        //
        //         return content.StartsWith('[') && content.EndsWith(']');
        //     }
        // }
        
        /// <summary>
        ///   Obtains a value from a specified <see cref="HttpRequest"/> element (such as headers or query). 
        /// </summary>
        /// <param name="request">
        ///   The extended <see cref="HttpRequest"/>.
        /// </param>
        /// <param name="element">
        ///   The element to obtain the value from.
        /// </param>
        /// <param name="key">
        ///   Identifies the requested value. 
        /// </param>
        /// <returns>
        ///   The requested value (a <see cref="string"/>) if found by the <paramref name="element"/>;
        ///   otherwise <c>null</c>.  
        /// </returns>
        public static string? GetItemValue(this HttpRequest request, HttpRequestElement element, string key)
            => element switch
            {
                HttpRequestElement.Header => request.Headers.ContainsKey(key) 
                    ? request.Headers[key].ToString()
                    : null,
                HttpRequestElement.Query => request.Query.ContainsKey(key) 
                    ? request.Query[key].ToString()
                    : null,
                _ => null
            };

        /// <summary>
        ///   Examines a <see cref="HttpRequest"/> applying a criteria (<see cref="HttpComparison"/>)
        ///   and returns a value to indicate whether it is a match. 
        /// </summary>
        /// <param name="request">
        ///   The extended <see cref="HttpRequest"/>.
        /// </param>
        /// <param name="criteria">
        ///   Specifies the criteria.
        /// </param>
        /// <param name="comparison">
        ///   Specifies how to compare <see cref="string"/>s.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="criteria"/> results in a match; otherwise <c>false</c>.
        /// </returns>
        /// <seealso cref="IsMatch(HttpComparison,HttpRequest,StringComparison)"/>
        public static bool IsMatch(
            this HttpRequest request,
            HttpComparison criteria, 
            StringComparison comparison = StringComparison.InvariantCulture)
        {
            return criteria.IsMatch(request, comparison);
        }
        
        /// <summary>
        ///   Applies a criteria to a <see cref="HttpRequest"/>
        ///   and returns a value to indicate whether it is a match.
        /// </summary>
        /// <param name="criteria">
        ///   The extended <see cref="HttpComparison"/> criteria.
        /// </param>        /// <param name="request">
        ///   The <see cref="HttpRequest"/>.
        /// </param>
        /// <param name="comparison">
        ///   Specifies how to compare <see cref="string"/>s.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="criteria"/> results in a match; otherwise <c>false</c>.
        /// </returns>
        /// <seealso cref="IsMatch(HttpRequest,HttpComparison,StringComparison)"/>
        public static bool IsMatch(
            this HttpComparison criteria, 
            HttpRequest request, 
            StringComparison comparison = StringComparison.InvariantCulture)
        {
            return criteria.IsMatch(request, comparison);
        }
        
        /// <summary>
        ///   Validates a <see cref="string"/> as a HTTP method (verb) and throws a <see cref="FormatException"/>
        ///   if it is not recognised.  
        /// </summary>
        /// <param name="value">
        ///   The <see cref="string"/> to be validated.
        /// </param>
        /// <param name="allowNull">
        ///   (optional; default=<c>true</c>)<br/>
        ///   Specifies whether the exception is also thrown when <paramref name="value"/> is <c>null</c>. 
        /// </param>
        /// <returns>
        ///   The <paramref name="value"/> (can be <c>null</c> if <paramref name="allowNull"/> is set).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="value"/> was unassigned (<c>null</c>) and <paramref name="allowNull"/> was not set.
        /// </exception>
        /// <exception cref="FormatException">
        ///   <paramref name="value"/> was not recognized as a HTTP method.
        /// </exception>
        public static string ValidateHttpMethod(string? value, bool allowNull = true)
        {
            var test = value?.Trim().ToUpper();
            switch (value)
            {
                case "GET":
                case "POST":
                case "PUT":
                case "PATCH":
                case "DELETE":
                case "HEAD":
                case "OPTIONS":
                case "TRACE":
                case "CONNECT":
                    return test!;
                
                case null:
                    if (allowNull)
                        return null!;
                     
                    throw new ArgumentNullException(nameof(value));

                default:
                    throw new FormatException($"Invalid HTTP method: {value}");
            }
        }

        /// <summary>
        ///   Validates the items of a <see cref="MultiStringValue"/> as HTTP httpMethods (verbs)
        ///   and returns them (on success) or throws a <see cref="FormatException"/> (on failure).  
        /// </summary>
        /// <param name="value">
        ///   The <see cref="MultiStringValue"/> to be validated.
        /// </param>
        /// <param name="allowNull">
        ///   (optional; default=<c>true</c>)<br/>
        ///   Specifies whether the exception is also thrown when <paramref name="value"/> is <c>null</c>. 
        /// </param>
        /// <returns>
        ///   The <paramref name="value"/> (can be <c>null</c> if <paramref name="allowNull"/> is set).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="value"/> was unassigned (<c>null</c>) and <paramref name="allowNull"/> was not set.
        /// </exception>
        /// <exception cref="FormatException">
        ///   <paramref name="value"/> was not recognized as a HTTP method.
        /// </exception>
        public static MultiStringValue? ValidateHttpMethods(MultiStringValue? value, bool allowNull = true)
        {
            if (value is null || value.IsEmpty)
            {
                if (allowNull)
                    return null;
            }

            var verbs = new List<string>();
            foreach (var item in value!.Items)
            {
                verbs.Add(ValidateHttpMethod(item));
            }

            return new MultiStringValue(verbs.ToArray());
        }

        /// <summary>
        ///   Casts a collection of <see cref="HttpMethod"/> enum values into a collection of
        ///   equivalent <see cref="string"/> values.
        /// </summary>
        /// <param name="httpMethods">
        ///   The enum values to be cast into <see cref="string"/>s.
        /// </param>
        /// <returns>
        ///   An array of <see cref="string"/>.
        /// </returns>
        public static string[] ToStringVerbs(this IEnumerable<HttpMethod> httpMethods) 
            => 
            httpMethods.Select(m => s_httpMethodToStringVerbMap[m]).ToArray();

        /// <summary>
        ///   Casts a <see cref="HttpMethod"/> enum value into its equivalent <see cref="string"/> value.
        /// </summary>
        /// <param name="httpMethod">
        ///   The <see cref="HttpMethod"/> value to be cast.
        /// </param>
        /// <returns>
        ///   A <see cref="string"/> value.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        ///   The <see cref="HttpMethod"/> is not recognized.
        /// </exception>
        public static string ToStringVerb(this HttpMethod httpMethod) => s_httpMethodToStringVerbMap[httpMethod];

        /// <summary>
        ///   Casts the <see cref="string"/> representation of a HTTP method into a <see cref="HttpMethod"/>. 
        /// </summary>
        /// <param name="stringVerb">
        ///   The textual HTTP method (verb) string representation.
        /// </param>
        /// <returns>
        ///   A <see cref="HttpMethod"/> value.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        ///   The <paramref name="stringVerb"/> value is not a recognized HTTP method.
        /// </exception>
        public static HttpMethod ToHttpMethod(this string stringVerb) => s_httpStringVerbToMethodMap[stringVerb.ToUpper()];
    }
}