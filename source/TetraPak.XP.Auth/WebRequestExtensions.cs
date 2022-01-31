using System;
using System.Text;

namespace TetraPak.XP.Auth
{
    internal static class WebRequestExtensions
    {
        public static System.Net.WebRequest WithBasicAuthentication(this System.Net.WebRequest self, string userId, string password)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));

            self.Headers["Authorization"] = $"Basic {EncodeBasicCredentials(userId, password)}";
            return self;
        }

        public static string EncodeBasicCredentials(string userId, string password)
        {
            var bytes = Encoding.ASCII.GetBytes($"{userId}:{password}");
            return Convert.ToBase64String(bytes);
        }
    }
}
