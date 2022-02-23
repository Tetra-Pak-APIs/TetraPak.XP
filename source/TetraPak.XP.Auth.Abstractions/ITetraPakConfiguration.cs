using System.Threading.Tasks;
using TetraPak.XP.Web.Abstractions;

namespace TetraPak.XP.Auth.Abstractions
{
    public interface ITetraPakConfiguration : IServiceAuthConfig, IWebConfiguration
    {
        /// <summary>
        ///   Gets a value indicating whether the solution enables caching (please see remarks).
        /// </summary>
        /// <remarks>
        ///   Please note that caching can be configured for different purposes but this might be a handy
        ///   setting to quickly kill all caching. Enabling caching simply means more detailed caching
        ///   configuration is honored.
        /// </remarks>
        bool IsCaching { get; }

        /// <summary>
        ///   Constructs and returns a <see cref="AuthContext"/>. 
        /// </summary>
        /// <param name="grantType">
        ///   Specifies the requested <see cref="GrantType"/>.
        /// </param>
        /// <param name="options">
        ///   Options describing the request.
        /// </param>
        /// <returns></returns>
        Task<Outcome<AuthContext>> GetAuthContextAsync(GrantType grantType, GrantOptions options);
    }
}