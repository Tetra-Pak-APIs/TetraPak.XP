using System;

namespace TetraPak.XP.Auth.Abstractions
{

    public static class BasicAuthCredentialsHelper
    {
        /// <summary>
        ///   Converts <see cref="Credentials"/> to <see cref="BasicAuthCredentials"/>.
        /// </summary>
        /// <param name="credentials">
        ///   The credentials to be converted. 
        /// </param>
        /// <returns>
        ///     A <see cref="BasicAuthCredentials"/> object.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   The <paramref name="credentials"/> was unassigned. 
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///   The provided <paramref name="credentials"/> <see cref="Credentials.Secret"/> property was unassigned. 
        /// </exception>
        public static BasicAuthCredentials ToBasicAuthCredentials(this Credentials? credentials)
        {
            return credentials switch
            {
                null => throw new ArgumentNullException(nameof(credentials)),
                BasicAuthCredentials basicAuthCredentials => basicAuthCredentials,
                _ => new BasicAuthCredentials(
                    credentials.Identity,
                    credentials.Secret.IsAssigned()
                        ? credentials.Secret!
                        : throw new InvalidOperationException("Basic authentication requires a secret"))
            };
        }
    }
}