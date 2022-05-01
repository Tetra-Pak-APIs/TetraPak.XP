using System;

namespace TetraPak.XP
{
    public interface IDateTimeSource
    {
        DateTime GetNow();

        DateTime GetUtcNow();

        DateTime GetToday();
    }
}