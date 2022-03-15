namespace TetraPak.XP.Auth.Abstractions
{
    public static class GrantOptionsHelper
    {
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
    }
}