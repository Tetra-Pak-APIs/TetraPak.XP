using System;
using System.Threading;
using System.Threading.Tasks;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Auth
{
    public abstract class AbstractAuthenticator : IAuthenticator
    {
        /// <summary>
        ///   Gets a logger (if any).
        /// </summary>
        /// <seealso cref="LogInfo"/>
        /// <seealso cref="LogDebug"/>
        /// <seealso cref="LogError"/>
        protected ILog? Log => Config.Log;

        /// <summary>
        ///   Gets the configuration.
        /// </summary>
        protected AuthConfig Config { get; }

        /// <summary>
        ///   Gets a default cache key (the client Id).
        /// </summary>
        protected string CacheKey => $"{Config.Authority.Host}::{Config.ClientId}";

        /// <summary>
        ///   Logs a message of <see cref="LogRank.Information"/>.
        /// </summary>
        protected void LogInfo(string message) => Log.Information(message);

        /// <summary>
        ///   Logs a message of <see cref="LogRank.Debug"/>.
        /// </summary>
        protected void LogDebug(string message) => Log.Debug(message);

        /// <summary>
        ///   Logs an error an an optional message (of <see cref="LogRank.Error"/>).
        /// </summary>
        protected void LogError(Exception exception, string message) => Log.Error(exception, message);

        /// <summary>
        ///   Gets a value indicating whether token can be persisted.
        /// </summary>
        /// <seealso cref="AuthConfig.TokenCache"/>
        public bool IsCaching => Config.IsCaching;
        
        /// <inheritdoc />
        public abstract Task<Outcome<Grant>> GetAccessTokenAsync(
            bool allowCached = true, 
            CancellationTokenSource? cancellationTokenSource = null);
        
        /// <inheritdoc />
        public abstract Task<Outcome<Grant>> GetAccessTokenSilentlyAsync(
            CancellationTokenSource? cancellationTokenSource = null);

        /// <summary>
        ///   Initializes the authenticator.
        /// </summary>
        protected AbstractAuthenticator(AuthConfig config)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }
    }
}