using System;
using System.Collections.Generic;
using System.Linq;

namespace TetraPak.XP.Auth.Abstractions.OIDC
{
/// <summary>
    /// Implementation of <see cref="IAuthorityValidationStrategy"/> based on <see cref="StringComparison"/>.
    /// </summary>
    /// <seealso cref="AuthorityUrlValidationStrategy"/>
    public sealed class StringComparisonAuthorityValidationStrategy : IAuthorityValidationStrategy
    {
        readonly StringComparison _stringComparison;

        /// <summary>
        /// Constructor with <see cref="StringComparison"/> argument.
        /// </summary>
        /// <param name="stringComparison"></param>
        public StringComparisonAuthorityValidationStrategy(StringComparison stringComparison = StringComparison.Ordinal)
        {
            _stringComparison = stringComparison;
        }

        /// <summary>
        /// String comparison between issuer and authority (trailing slash ignored).
        /// </summary>
        /// <param name="issuerName"></param>
        /// <param name="expectedAuthority"></param>
        /// <returns></returns>
        public Outcome IsIssuerNameValid(string issuerName, string expectedAuthority)
        {
            if (string.IsNullOrWhiteSpace(issuerName)) 
                return Outcome.Fail("Issuer name is missing");

            return string.Equals(issuerName.RemoveTrailingSlash(), expectedAuthority.RemoveTrailingSlash(), _stringComparison) 
                ? Outcome.Success() 
                : Outcome.Fail($"Issuer name does not match authority: {issuerName}");
        }

        /// <summary>
        /// String "starts with" comparison between endpoint and allowed authorities.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="allowedAuthorities"></param>
        /// <returns></returns>
        public Outcome IsEndpointValid(string endpoint, IEnumerable<string> allowedAuthorities)
        {
            if (string.IsNullOrEmpty(endpoint))
                return Outcome.Fail("endpoint is empty");

            return allowedAuthorities.Any(authority => endpoint.StartsWith(authority, _stringComparison)) 
                ? Outcome.Success() 
                : Outcome.Fail($"Endpoint belongs to different authority: {endpoint}");
        }
    }
}