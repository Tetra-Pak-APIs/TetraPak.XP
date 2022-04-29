using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Caching;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Mobile
{
    /// <summary>
    ///   Provides a mobile implementation for the <see cref="ITokenCache"/> contract.
    ///   This service is automatically added by the <see cref="TetraPakMobileHostBuilderHelper.BuildTetraPakMobileHost"/>
    ///   extension method but can be supplied individually.  
    /// </summary>
    /// <seealso cref="TetraPakMobileHostBuilderHelper.AddMobileTokenCache"/>
    /// <seealso cref="TetraPakMobileHostBuilderHelper.BuildTetraPakMobileHost"/>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SecureStoreTokenCache : SimpleCache, ITokenCache
    {
        public override bool IsTypeStrict
        {
            get => false; 
            set { /* ignore */ }
        }
        
        public SecureStoreTokenCache(ILog? log = null) 
        : base(log)
        {
            AddDelegates(new SecureStoreCacheDelegate(this, log));
        }
    }
}