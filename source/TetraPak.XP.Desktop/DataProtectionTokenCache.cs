using Microsoft.AspNetCore.DataProtection;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Caching;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Desktop
{
    class DataProtectionTokenCache : SimpleCache, ITokenCache
    {
        public override bool IsTypeStrict
        {
            get => false; 
            set { /* ignore */ }
        }

        public DataProtectionTokenCache(IDataProtectionProvider protectionProvider,  ILog? log = null) 
        : base(log, new DataProtectionSecureCacheDelegate(protectionProvider, log))
        {
        }
    }
}