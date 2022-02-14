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
            public const string Identity = "IdTokens";

            /// <summary>
            ///   Identifies the cache repository used for tokens acquired from token exchange grants. 
            /// </summary>
            public const string TokenExchange = "TxTokens";

            /// <summary>
            ///   Identifies the cache repository used for tokens acquired from client credential grants. 
            /// </summary>
            public const string ClientCredentials = "CcTokens";

            /// <summary>
            ///   Identifies the cache repository used for JWT bearer tokens acquired from the local DevProxy. 
            /// </summary>
            public const string DevProxy = "DpTokens";
            
            /// <summary>
            ///   Identifies the cache repository used for tokens acquired from device code grants. 
            /// </summary>
            public const string DeviceCodeCredentials = "DcTokens";

        }
    }
}