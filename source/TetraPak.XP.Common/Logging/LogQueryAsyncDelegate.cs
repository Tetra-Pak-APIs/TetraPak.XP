using System.Collections.Generic;
using System.Threading.Tasks;

namespace TetraPak.XP.Logging
{
    public delegate Task<IEnumerable<ILogEntry>> QueryAsyncDelegate(params LogRank[] ranks);
}