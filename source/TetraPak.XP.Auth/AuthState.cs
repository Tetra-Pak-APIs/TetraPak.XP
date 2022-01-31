using System;
using System.Security.Cryptography;
using System.Text;

namespace TetraPak.XP.Auth
{
    internal struct AuthState
    {
        const string IdentCodeChallengeMethod = "S256";

        public bool IsUsed { get; }

        public bool IsPKCEUsed { get; }

        public string State { get; }

        public string? Verifier { get; }

        public string CodeChallenge { get; }

        public string CodeChallengeMethod { get; }

        static string makeVerifier(uint length = 32, string clientId = null)
        {
            return string.IsNullOrEmpty(clientId)
                ? randomDataBase64(length)
                : $"{clientId}{randomDataBase64(32)}";
        }

        static string randomDataBase64(uint length)
        {
            var rng = new RNGCryptoServiceProvider();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return base64EncodeNoPadding(bytes);
        }

        static string base64EncodeNoPadding(byte[] buffer)
        {
            return base64UrlEncode(buffer).TrimEnd('=');
        }

        static string base64UrlEncode(byte[] buffer)
        {
            return Convert.ToBase64String(buffer)
                .Replace("/", "_")
                .Replace('+', '-')
                .Replace("=", "");
        }

        static byte[] sha256(string input) => new SHA256Managed().ComputeHash(Encoding.ASCII.GetBytes(input));

        public AuthState(bool useState, bool usePkceUsed, string clientId)
        {
            IsUsed = useState;
            IsPKCEUsed = usePkceUsed;

            if (!useState)
            {
                State = Verifier = CodeChallenge = CodeChallengeMethod = null;
                return;
            }

            State = randomDataBase64(32);
            if (!IsPKCEUsed)
            {
                Verifier = CodeChallenge = CodeChallengeMethod = null;
                return;
            }

            Verifier = makeVerifier(32, clientId);
            CodeChallenge = base64UrlEncode(sha256(Verifier));
            CodeChallengeMethod = IdentCodeChallengeMethod;
        }
    }
}
