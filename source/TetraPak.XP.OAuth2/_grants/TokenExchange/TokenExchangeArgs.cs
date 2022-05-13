using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.OAuth2.TokenExchange
{
    /// <summary>
    ///   Provides the options to be used for a token exchange process (see <see cref="ITokenExchangeGrantService.AcquireTokenAsync"/>).
    ///   See https://tools.ietf.org/id/draft-ietf-oauth-token-exchange-19.html for more details,
    /// </summary>
    public sealed class TokenExchangeArgs
    {
        /// <summary>
        ///   Gets the credentials to be used for token exchange (typically a <see cref="BasicAuthCredentials"/>
        ///   with client id/client secret. 
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Credentials Credentials { get; set; }
        
        /// <summary>
        ///   Gets the subject token to be exchanged.
        /// </summary>
        public ActorToken SubjectToken { get; set; }

        /// <summary>
        ///   Gets the subject token type.
        /// </summary>
        public string SubjectTokenType { get; set; }

        /// <summary>
        ///   (optional)<br/>
        ///   Gets the a security token that represents the identity of the acting party. Typically, this will
        ///   be the party that is authorized to use the requested security token and act on behalf of the subject.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string? ActorToken { get; set; }

        /// <summary>
        ///   (optional)<br/>
        ///   Gets the an identifier that indicates the type of the security token in the
        ///   <see cref="ActorToken"/> parameter. This is REQUIRED when the <see cref="ActorToken"/> parameter is set
        ///   but MUST NOT be included otherwise.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string? ActorTokenType { get; set; }

        /// <summary>
        ///   (optional)<br/>
        ///   A URI that indicates the target service or resource where the client intends to use the
        ///   requested security token.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Uri? Resource { get; set; }
        
        /// <summary>
        ///   (optional)<br/>
        ///   Gets or sets the logical name of the target service where the client intends to use the
        ///   requested security token. See 
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string? Audience { get; set; }
        
        /// <summary>
        ///   (optional)<br/>
        ///   A list of case-sensitive strings, as defined in Section 3.3 of [RFC6749], that allow the client to
        ///   specify the desired scope of the requested security token in the context of the service or resource
        ///   where the token will be used. The values and associated semantics of scope are service specific and
        ///   expected to be described in the relevant service documentation.
        /// </summary>
        public IEnumerable<string> Scope { get; set; }

        /// <summary>
        ///   (optional)<br/>
        ///   An identifier, as described in Section 3 of [RFC6749], for the type of the requested security token.
        ///   If the requested type is unspecified, the issued token type is at the discretion of the authorization
        ///   server and may be dictated by knowledge of the requirements of the service or resource indicated by the
        ///   resource or audience parameter.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string? RequestedTokenType { get; set; }

        internal string ToHttpJsonBody() => JsonSerializer.Serialize(ToDictionary());

        internal IDictionary<string,string> ToDictionary()
        {
            var dictionary = new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:token-exchange",
                ["subject_token"] = SubjectToken!,
                ["subject_token_type"] = SubjectTokenType,
            };
            if (ActorToken is { })
            {
                dictionary["actor_token"] = ActorToken;
                dictionary["actor_token_type"] = ActorTokenType!;
            }
            if (Resource is { })
            {
                dictionary["resource"] = Resource.AbsoluteUri;
            }
            if (Audience is { })
            {
                dictionary["audience"] = Audience;
            }
            if (Scope.Any())
            {
                dictionary["scope"] = Scope.ConcatEnumerable(" ");
            }
            if (RequestedTokenType is { })
            {
                dictionary["requested_token_type"] = RequestedTokenType;
            }

            return dictionary;
        }

        public TokenExchangeArgs(
            Credentials credentials, 
            ActorToken subjectToken, 
            string subjectTokenType,
            params string[] scope)
        {
            if (string.IsNullOrWhiteSpace(subjectToken)) throw new ArgumentNullException(nameof(subjectToken));
            if (string.IsNullOrWhiteSpace(subjectTokenType)) throw new ArgumentNullException(nameof(subjectTokenType));

            Credentials = credentials;
            SubjectToken = subjectToken.Identity!;
            SubjectTokenType = subjectTokenType;
            Scope = scope.Length != 0
                ? scope
                : Array.Empty<string>();
        }
    }
}