using System.Collections.Generic;
using System.Threading.Tasks;

namespace TetraPak.XP.Logging
{
    public delegate Task<IEnumerable<ILogEntry>> LogQueryAsyncDelegate(params LogRank[] ranks);
}