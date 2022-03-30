using System;
using System.Collections.Generic;

namespace TetraPak.XP.Auth.Abstractions.OIDC
{
    /// <summary>
    /// Authority validation strategy.
    /// </summary>
    public interface IAuthorityValidationStrategy
    {
        /// <summary>
        /// Validate issuer name found in Discovery Document.
        /// </summary>
        /// <param name="expectedAuthority">Authority expected.</param>
        /// <param name="issuerName">Authority declared in Discovery Document.</param>
        /// <returns>
        ///   An <see cref="Outcome"/> to indicate success/failure and or,
        ///   on failure, an <see cref="Exception"/> and textual error message.
        /// </returns>
        Outcome IsIssuerNameValid(string issuerName, string expectedAuthority);

        /// <summary>
        /// Validate end point found in Discovery Document.
        /// </summary>
        /// <param name="expectedAuthority">Authority expected.</param>
        /// <param name="endpoint">Endpoint declared in Discovery Document.</param>
        /// <returns>
        ///   An <see cref="Outcome"/> to indicate success/failure and or,
        ///   on failure, an <see cref="Exception"/> and textual error message.
        /// </returns>
        Outcome IsEndpointValid(string endpoint, IEnumerable<string> expectedAuthority);
    }
}
