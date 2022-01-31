// using System;
// using System.Threading.Tasks;
// using TetraPak.XP.Caching;
// using TetraPak.XP.Logging;
//
// namespace TetraPak.XP.Auth
// {
//     /// <summary>
//     ///   Simply enforces a target sub repository - 'securityTokens' - and the use of a secure cache. obsolete
//     /// </summary>
//     class TokenCacheDelegate : SimpleCacheDelegate
//     {
//         const string DefaultTokenCacheRepository = "securityTokens";
//         
//         public override async Task<Outcome> CreateAsync(ITimeLimitedRepositoryEntry entry, bool strict)
//         {
//             
//             var path = (SimpleCachePath)entry.Path;
//             return IsTargetRepository(path)
//                 ? await DelegateCreateAsync(entry, strict)
//                 : Outcome<ITimeLimitedRepositoryEntry>.Fail(new Exception($"Not the target repository: {path}"));
//         }
//         
//         public override async Task<Outcome<ITimeLimitedRepositoryEntry>> ReadRawEntryAsync(DynamicPath path)
//         {
//             return IsTargetRepository(path)
//                 ? await DelegateReadRawEntryAsync(path)
//                 : Outcome<ITimeLimitedRepositoryEntry>.Fail(new Exception($"Not the target repository: {path}"));
//         }
//
//         public override Task<Outcome> UpdateAsync(ITimeLimitedRepositoryEntry entry, bool strict)
//         {
//             var path = (SimpleCachePath)entry.Path;
//             
//             
//             return base.UpdateAsync(entry, strict);
//         }
//         
//         
//
//         public TokenCacheDelegate(ILog? log, string targetRepository = TokenCacheRepository) 
//         : base(log)
//         {
//             WithTargetRepository(targetRepository);
//         }
//     }
// }