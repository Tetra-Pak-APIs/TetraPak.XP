﻿using TetraPak.Auth.Xamarin.common;

namespace TetraPak.XP.Auth.Abstractions.OIDC
{
    static class InternalStringExtensions
    {
        public static string RemoveTrailingSlash(this string self) => self.EnsureNotEndsWith("/");
    }
}