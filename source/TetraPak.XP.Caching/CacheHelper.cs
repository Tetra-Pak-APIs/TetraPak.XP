using System;

namespace TetraPak.XP.Caching
{
    
    public static class CacheHelper
    {
        public const string TagRemainingLifespan = "__remainingLifespan";

        public static TimeSpan GetRemainingLifespan<T>(this Outcome<T> outcome)
        {
            return outcome.TryGetValue(TagRemainingLifespan, out TimeSpan lifespan)
                ? lifespan
                : TimeSpan.Zero;
        }
    }
}