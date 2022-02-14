namespace TetraPak.XP.Web.Abstractions
{
    public static class Headers
    {
        public const string ServiceDiagnostics = "tp-diag";
        public const string ServiceDiagnosticsTime = "tp-diag-time";

        /// <summary>
        ///   Identifies a message id persisting throughout a request/response roundtrip. 
        /// </summary>
        public const string RequestMessageId = "api-flow-id"; 
    }
}