using System;
using System.Collections.Generic;

namespace TetraPak.XP.Auth.Abstractions.OIDC
{
/// <summary>
    /// <para>Implementation of <see cref="IAuthorityValidationStrategy"/> based on <see cref="Uri"/> equality.
    /// Trailing slash is also ignored.</para>
    /// </summary>
    /// <seealso cref="StringComparisonAuthorityValidationStrategy"/>
    public sealed class AuthorityUrlValidationStrategy : IAuthorityValidationStrategy
    {
        /// <inheritdoc/>
        public Outcome IsIssuerNameValid(string issuerName, string expectedAuthority)
        {
            if (!Uri.TryCreate(expectedAuthority.RemoveTrailingSlash(), UriKind.Absolute, out var expectedAuthorityUrl))
                throw new ArgumentOutOfRangeException(nameof(expectedAuthority), "Authority must be a valid URL." );

            if (string.IsNullOrWhiteSpace(issuerName))
                return Outcome.Fail("Issuer name is missing");

            if (!Uri.TryCreate(issuerName.RemoveTrailingSlash(), UriKind.Absolute, out var issuerUrl))
                return Outcome.Fail("Issuer name is not a valid URL");

            return expectedAuthorityUrl.Equals(issuerUrl) 
                ? Outcome.Success()
                : Outcome.Fail($"Issuer name does not match authority: {issuerName}");
        }

        /// <inheritdoc/>
        public Outcome IsEndpointValid(string endpoint, IEnumerable<string> allowedAuthorities)
        {
            if (string.IsNullOrEmpty(endpoint))
                return Outcome.Fail("endpoint is empty");

            if (!Uri.TryCreate(endpoint.RemoveTrailingSlash(), UriKind.Absolute, out var endpointUrl))
                return Outcome.Fail("Endpoint is not a valid URL");

            foreach (var authority in allowedAuthorities)
            {
                if (!Uri.TryCreate(authority.RemoveTrailingSlash(), UriKind.Absolute, out var authorityUrl))
                    throw new ArgumentOutOfRangeException(nameof(allowedAuthorities), "Authority must be a URL.");

                var expectedString = authorityUrl.ToString();
                var testString = endpointUrl.ToString();

                if (testString.StartsWith(expectedString, StringComparison.Ordinal))
                    return Outcome.Success();
            }

            return Outcome.Fail($"Endpoint belongs to different authority: {endpoint}");
        }
    }
}