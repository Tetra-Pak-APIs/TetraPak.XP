using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TetraPak.XP.Web
{
    /// <summary>
    ///   See <see cref="ILoopbackBrowser.GetLoopbackAsync"/> for more info on how to use this delegate.
    /// </summary>
    public delegate Task<LoopbackFilterOutcome> LoopbackFilter(HttpRequest request);
}