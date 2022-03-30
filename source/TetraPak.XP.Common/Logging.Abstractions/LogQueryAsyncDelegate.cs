using System.Collections.Generic;
using System.Threading.Tasks;

namespace TetraPak.XP.Logging.Abstractions
{
    public delegate Task<IEnumerable<ILogEntry>> LogQueryAsyncDelegate(params LogRank[] ranks);
}