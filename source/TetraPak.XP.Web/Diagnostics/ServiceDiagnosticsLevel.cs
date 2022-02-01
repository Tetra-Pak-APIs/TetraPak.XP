using System;

namespace TetraPak.XP.Web.Diagnostics
{
    [Flags]
    public enum ServiceDiagnosticsLevel
    {
        None = 0,
        
        Time = 1,
        
        Request = 2,
        
        Response = 4,
        
        Auth = 8,
        
        All = Time | Request | Response | Auth
    }
}