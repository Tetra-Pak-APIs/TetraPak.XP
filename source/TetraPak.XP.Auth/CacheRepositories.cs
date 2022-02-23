namespace TetraPak.XP.Auth
{
    public static class CacheRepositories
    {
        /// <summary>
        ///   Provides identifiers for token caches. 
        /// </summary>
        public static class Tokens
        {
            /// <summary>
            ///   Identifies the cache repository used for identity tokens. 
            /// </summary>
            public const string Identity = "ID_Tokens";

            /// <summary>
            ///   Identifies the cache repository used for auth code issued tokens. 
            /// </summary>
            public const string OIDC = "OIDC_Tokens";

            /// <summary>
            ///   Identifies the cache repository used for tokens acquired from token exchange grants. 
            /// </summary>
            public const string TokenExchange = "TX_Tokens";

            /// <summary>
            ///   Identifies the cache repository used for tokens acquired from client credential grants. 
            /// </summary>
            public const string ClientCredentials = "CC_Tokens";

            /// <summary>
            ///   Identifies the cache repository used for JWT bearer tokens acquired from the local DevProxy. 
            /// </summary>
            public const string DevProxy = "DP_Tokens";
            
            /// <summary>
            ///   Identifies the cache repository used for tokens acquired from device code grants. 
            /// </summary>
            public const string DeviceCodeCredentials = "DcTokens";

        }
    }
}