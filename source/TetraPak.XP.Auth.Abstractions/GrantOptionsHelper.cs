namespace TetraPak.XP.Auth.Abstractions
{
    public static class GrantOptionsHelper
    {
        static class DataKeys
        {
            internal const string AppCredentials = "__" + nameof(AppCredentials);

            internal const string AuthorityInfo = "__" + nameof(AuthorityInfo);
        }
        
        /// <summary>
        ///   (fluent api)<br/>
        ///   Assigns the <see cref="GrantOptions.Flags"/> property and returns the <see cref="GrantOptions"/>.
        /// </summary>
        /// <param name="options">
        ///   The <see cref="GrantOptions"/> object to be used for the grant request.   
        /// </param>
        /// <param name="scope">
        ///   The scope to be requested with the grant.
        /// </param>
        public static GrantOptions WithScope(this GrantOptions options, GrantScope scope)
        {
            options.Scope = scope;
            return options;
        }

        /// <summary>
        ///   (fluent api)<br/>
        ///   Adds an arbitrary data value to the <see cref="GrantOptions"/> options and returns it.
        /// </summary>
        /// <param name="options">
        ///   The <see cref="GrantOptions"/> object to be used for the grant request.   
        /// </param>
        /// <param name="key">
        /// 
        /// </param>
        /// <param name="data"></param>
        public static GrantOptions WithData(this GrantOptions options, string key, object data)
        {
            options.SetData(key, data);
            return options;
        }

        public static GrantOptions WithClientCredentials(this GrantOptions options, Credentials? clientCredentials)
            => clientCredentials is {} ? options.WithData(DataKeys.AppCredentials, clientCredentials) : options;

        public static Credentials? GetClientCredentials(this GrantOptions options)
            => options.GetData<Credentials?>(DataKeys.AppCredentials);

        public static GrantOptions WithAuthInfo(this GrantOptions options, IAuthInfo? authInfo)
            => authInfo is {} ? options.WithData(DataKeys.AuthorityInfo, authInfo) : options;

        public static IAuthInfo? GetAuthInfo(this GrantOptions options)
            => options.GetData<IAuthInfo?>(DataKeys.AuthorityInfo);

        /// <summary>
        ///   Inspects the <see cref="GrantOptions"/> and, optionally, a <see cref="ITetraPakConfiguration"/>
        ///   object and returns a value indicating whether grant caching is currently enabled.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static bool IsGrantCachingEnabled(this GrantOptions options, ITetraPakConfiguration? config)
        {
            return config is { } 
                ? config.IsCaching
                  && options.IsCaching : options.IsCaching;
        }
    }
}