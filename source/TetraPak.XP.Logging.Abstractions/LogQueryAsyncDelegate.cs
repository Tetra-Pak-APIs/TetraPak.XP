using System.Collections.Generic;
using System.Threading.Tasks;

namespace TetraPak.XP.Logging.Abstractions
{
    public delegate Task<IEnumerable<ILogEvent>> LogQueryAsyncDelegate(params LogRank[] ranks);
}