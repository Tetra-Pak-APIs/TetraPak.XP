using System;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Represents a bearer token (a token with 'Bearer ' prefix).
    /// </summary>
    public class BearerToken : ActorToken
    {
        /// <summary>
        ///   Gets the bearer token qualifier.
        /// </summary>
        public const string Qualifier = "Bearer ";

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Qualifier}{base.ToString()}";
        }

        /// <summary>
        ///   Converts the string representation of a bearer token to its <see cref="BearerToken"/> equivalent.
        /// </summary>
        /// <param name="s">
        ///   A string containing a token to convert.
        /// </param>
        /// <returns>
        ///   A <see cref="BearerToken"/> equivalent to the value contained in s.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="s"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="FormatException">
        ///   <paramref name="s"/> is not in the correct format.
        /// </exception>
        public static BearerToken Parse(string s)
        {
            if (s is null)
                throw new ArgumentNullException(nameof(s));

            if (string.IsNullOrWhiteSpace(s))
                throw new FormatException("Token cannot be an empty string");

            if (!tryParse(s, out var identity))
                throw new FormatException("Invalid bearer token");

            return new BearerToken
            {
                Identity = identity
            };
        }

        /// <inheritdoc />
#if NET5_0_OR_GREATER
        protected override bool OnTryParse(string value, [NotNullWhen(true)] out string? identity) 
#else
        protected override bool OnTryParse(string value, out string? identity) 
#endif
            => tryParse(value, out identity);

        /// <summary>
        ///   Converts the string representation of a bearer token.
        ///   A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="s">
        ///   A string containing a bearer token to convert.
        /// </param>
        /// <param name="token">
        ///   When this method returns, contains the token equivalent of the value contained in <paramref name="s"/>,
        ///   if the conversion succeeded, or null if the conversion failed.
        ///   The conversion fails if the s parameter is null or Empty, or is not of the correct format.
        ///   This parameter is passed uninitialized; any value originally supplied in result will be overwritten.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="s"/> was converted successfully; otherwise, <c>false</c>.
        /// </returns>
#if NET5_0_OR_GREATER
        public static bool TryParse(string s, [NotNullWhen(true)] out BearerToken? token)
#else
        public static bool TryParse(string s, out BearerToken? token)
#endif
        {
            token = null;
            if (string.IsNullOrWhiteSpace(s) || !tryParse(s, out var identity))
                return false;

            token = new BearerToken(identity, false);
            return true;
        }

#if NET5_0_OR_GREATER
        static bool tryParse(string value, [NotNullWhen(true)] out string? identity)
#else
        static bool tryParse(string value, out string? identity)
#endif
        {
            identity = null;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim();
            if (!value.StartsWith(Qualifier))
                return false;

#if NET5_0_OR_GREATER            
            identity = value[Qualifier.Length..];
#else
            identity = value.Substring(Qualifier.Length);
#endif
            return true;
        }

        BearerToken()
        {
        }

        public BearerToken(string stringValue, bool parse = true) 
        : base(stringValue, parse)
        {
        }
    }
    
    public static class BearerTokenHelper
    {
        public static BearerToken ToBearerToken(this string self)
        {
            var token = self.EnsurePrefix(BearerToken.Qualifier);
            return BearerToken.TryParse(token, out var bearerToken)
                ? bearerToken
                : throw new FormatException($"Cannot convert string \"{self}\" to bearer token");
        }
    }
}