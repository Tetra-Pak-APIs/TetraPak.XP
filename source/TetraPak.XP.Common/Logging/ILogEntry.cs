﻿using System;

namespace TetraPak.XP.Logging
{
    public interface ILogEntry
    {
        LogRank Rank { get; }

        DateTime Time { get; }

        string Message { get; }
    }
}