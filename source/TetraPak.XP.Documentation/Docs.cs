namespace TetraPak.XP.Documentation
{
    /// <summary>
    ///   Root class for referencing documents and articles from code.
    /// </summary>
    public static class Docs
    {
        /// <summary>
        ///   Generates a standard "Please see: {url}" string.
        /// </summary>
        /// <param name="url">
        ///   The universal resource locator for the referenced text.
        /// </param>
        /// <returns>
        ///   A <see cref="string"/>.
        /// </returns>
        public static string PleaseSee(string url) => $"Please see {url}";
        
        /// <summary>
        ///   Represents the Tetra Pak developer portal. 
        /// </summary>
        public static class DevPortal
        {
            const string BasePath = "https://developer.tetrapak.com/products";
            
            /// <summary>
            ///   A link to a text about token types in token exchange flows. 
            /// </summary>
            public const string TokenExchangeSubjectTokenTypes = BasePath + 
                "/tetra-pak-enterprise-security/oauth2-grant-type-token-exchange#subject-token-types";
        }

        /// <summary>
        ///   Represents documents available through the SDK repository.
        /// </summary>
        public static class SdkRepo
        {
            const string BasePath = "https://github.com/Tetra-Pak-APIs/TetraPak.AspNet";
            
            // todo Write article about why claims transformation with system identities are not supported and automatically skipped
            /// <summary>
            ///   URL for text about why claims transformation with system identities are not supported and automatically skipped
            /// </summary>
            public const string ClaimsTransformationWithSystemIdentity = BasePath +
                "/--TODO--"; 
        }
    }
}