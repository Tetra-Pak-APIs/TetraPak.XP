using Xamarin.Forms;

namespace TetraPak.XP.Mobile.Android
{
    /// <summary>
    ///   Provides convenient helper methods for initializing Tetra Pak SDK
    ///   for iOS application. 
    /// </summary>
    public static class XamarinFormsSkdHelper
    {
        // ReSharper disable once NotAccessedField.Local
#pragma warning disable CS8618
        static Application s_application;
#pragma warning restore CS8618

        /// <summary>
        ///   Forces linker to include iOS implementation of Tetra Pak SDK services.
        /// </summary>
        /// <param name="application">
        ///   The Xamarin Forms application.
        /// </param>
        /// <returns>
        ///   The Xamarin Forms application.
        /// </returns>
        public static Application WithTetraPakSDK(this Application application)
        {
            // note this is just to force linking in the assembly
            s_application = application;
            return application;
        }
    }
}
