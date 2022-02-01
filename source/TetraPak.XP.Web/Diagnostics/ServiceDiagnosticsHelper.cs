using Microsoft.AspNetCore.Http;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Web.Diagnostics
{
    public static class ServiceDiagnosticsHelper
    {
        const string KeyDiagnostics = "_tp_diag";
        
        public static ServiceDiagnostics? BeginDiagnostics(this HttpContext self, ILog? logger)
        {
            var level = self.Request.GetDiagnosticsLevel(logger);
            if (level == ServiceDiagnosticsLevel.None)
                return null;
            
            var diagnostics = new ServiceDiagnostics();
            self.SetValue(KeyDiagnostics, diagnostics);
            
            // todo support telemetry log levels 'Request' and 'Response'

            return diagnostics;
        }
        
        /// <summary>
        ///   Returns a <see cref="ServiceDiagnostics"/> object if available; otherwise <c>null</c>. 
        /// </summary>
        public static ServiceDiagnostics? GetDiagnostics(this HttpContext self)
        {
            return self.GetValue<ServiceDiagnostics>(KeyDiagnostics);
        }

        public static void DiagnosticsStartTimer(this HttpContext self, string key) 
            => self.GetDiagnostics()?.StartTimer(key);
        
        public static long? DiagnosticsStopTimer(this HttpContext self, string key, bool stopTimer = true) 
            => self.GetDiagnostics()?.GetElapsedMs(key, stopTimer);
        
        public static ServiceDiagnostics End(this ServiceDiagnostics? self)
        {
            if (self is null)
                return null!;

            self.StopTimer();
            return self;
        }

    }
}