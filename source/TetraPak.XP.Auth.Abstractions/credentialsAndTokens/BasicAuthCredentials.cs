using System;
using System.Text;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Represents basic authentication credentials. 
    /// </summary>
    public sealed class BasicAuthCredentials : Credentials
    {
        const string SecretQualifier = ":";
        const string NewSecretQualifier = " ; ";
        internal const string Scheme = "Basic ";

        /// <summary>
        ///   Gets the basic auth credentials encoded form.  
        /// </summary>
        public string Encoded => encode();

        string encode()
        {
            var bytes = string.IsNullOrWhiteSpace(NewSecret)
                ? Encoding.ASCII.GetBytes($"{Identity}{SecretQualifier}{Secret}")
                : Encoding.ASCII.GetBytes($"{Identity}{SecretQualifier}{Secret}{NewSecretQualifier}{NewSecret}");

            return Convert.ToBase64String(bytes);
        }

        /// <inheritdoc />
        public override string ToString() => $"{Scheme} {Encoded}";

        /// <summary>
        ///   Implicitly casts <see cref="BasicAuthCredentials"/> to a <see cref="string"/>.
        /// </summary>
        /// <param name="basicAuthCredentials">
        ///   The <see cref="BasicAuthCredentials"/> to be cast.
        /// </param>
        /// <returns>
        ///   The <see cref="string"/> representation of the <see cref="BasicAuthCredentials"/>.
        /// </returns>
        public static implicit operator string(BasicAuthCredentials basicAuthCredentials) =>
            basicAuthCredentials.ToString();

        static Outcome<(string identity, string secret, string newSecret)> decode(string encoded)
        {
            if (encoded.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase))
            {
                encoded = encoded.Substring(Scheme.Length);
            }

            try
            {
                var bytes = Convert.FromBase64String(encoded);
                var credentials = Encoding.UTF8.GetString(bytes).Split(SecretQualifier[0]);
                if (credentials.Length != 2)
                    return Outcome<(string, string, string)>.Fail(new FormatException($"Expected two elements but found {credentials.Length}"));

#if NET5_0_OR_GREATER
                var splitSecret = credentials[1].Split(NewSecretQualifier, StringSplitOptions.RemoveEmptyEntries);
#else
                var splitSecret = credentials[1].Split(new[] {NewSecretQualifier}, StringSplitOptions.RemoveEmptyEntries);
#endif
                return Outcome<(string identity, string secret, string newSecret)>.Success(splitSecret.Length == 1
                    ? (credentials[0], credentials[1], null!)
                    : (credentials[0], splitSecret[0], splitSecret[1]));
            }
            catch (Exception ex)
            {
                return Outcome<(string identity, string secret, string newSecret)>.Fail(ex);
            }
        }

        /// <summary>
        ///   Parses a <see cref="string"/> value to produce a <see cref="BasicAuthCredentials"/> object.
        /// </summary>
        /// <param name="stringValue">
        ///   The <see cref="string"/> value.
        /// </param>
        /// <returns>
        ///   A <see cref="BasicAuthCredentials"/> object if parsing was successful;
        ///   otherwise <c>null</c>. 
        /// </returns>
        public static BasicAuthCredentials Parse(string stringValue)
        {
            var outcome = decode(stringValue);
            return !outcome 
                ? null! 
                : new BasicAuthCredentials(outcome.Value.identity, outcome.Value.secret);
        }

        /// <summary>
        ///   Initializes a <see cref="BasicAuthCredentials"/> object.
        /// </summary>
        /// <param name="encoded">
        ///   The textual and encoded representation of a <see cref="BasicAuthCredentials"/> value.
        /// </param>
        /// <exception cref="FormatException">
        ///   The <paramref name="encoded"/> value could not be successfully parsed.
        /// </exception>
        public BasicAuthCredentials(string encoded)
        {
            if (string.IsNullOrWhiteSpace(encoded))
            {
                Identity = "";
                Secret = "";
                return;
            }

            var outcome = decode(encoded);
            if (!outcome)
                throw new FormatException($"Invalid basic auth: '{encoded}'");

            Identity = outcome.Value.identity;
            Secret = outcome.Value.secret;
            NewSecret = outcome.Value.newSecret;
        }

        /// <summary>
        ///   Initializes a <see cref="BasicAuthCredentials"/> object.
        /// </summary>
        /// <param name="identity">
        ///   Initializes <see cref="Credentials.Identity"/>.
        /// </param>
        /// <param name="secret">
        ///   Initializes <see cref="Credentials.Secret"/>.
        /// </param>
        /// <param name="newSecret">
        ///   Initializes <see cref="Credentials.NewSecret"/>.
        /// </param>
        public BasicAuthCredentials(string identity, string secret, string? newSecret = null)
        : base(identity, secret, newSecret)
        {
        }
    }
}