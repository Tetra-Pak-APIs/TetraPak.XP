namespace TetraPak.XP
{
    /// <summary>
    ///   Specifies a browser experience. 
    /// </summary>
    public enum BrowserExperience
    {
        /// <summary>
        ///   An optimized, staying inside the application.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This option is usually the preferred experience as it combines the abilities of an external system browser,
        ///     enabling access to device certificates to handle SSO mechanics, while allowing for a somewhat coherent
        ///     user experience that makes the browser appear part of your application.
        ///   </para>
        ///   <para>
        ///     The drawback might be a technical difficulty dismissing the browser from your code on some platforms,
        ///     such as Android. 
        ///   </para> 
        /// </remarks>
        InternalSystem,
        
        /// <summary>
        ///   An internal web view control.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This option allows for full control of the web view lifecycle and represents the most coherent
        ///     user experience.
        ///   </para>
        ///   <para>
        ///     The drawback is a web view cannot participate in SSO mechanisms that requires access to protected
        ///     device certificates. This might require the user to manually submit her credentials, even on a
        ///     managed device. 
        ///   </para> 
        /// </remarks>
        Internal,
        
        /// <summary>
        ///   The (default) system browser.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This option allows very little control over the browser experience and exposes the user with the 
        ///     experience she is "leaving" your application during the web based flow. Usually, this option is
        ///     a last resort if the <see cref="InternalSystem"/> option is not available.
        ///     The external system browser experience, unlike the <see cref="Internal"/>, can participate in
        ///     authorization flows that requires access to protected device certificates. 
        ///   </para>
        /// </remarks>
        ExternalSystem
    }
}