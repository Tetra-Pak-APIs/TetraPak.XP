namespace TetraPak.AspNet.Debugging
{
    static class TraceRequest
    {
        const string RequestInQualifier = "IN";
        const string RequestOutQualifier = "OUT";
        const string ResponseQualifier = "RESPONSE";
        const string ChevronsRight = ">>>";
        const string ChevronsLeft = "<<<";
        
        internal static string GetTraceRequestQualifier(HttpDirection direction, string? initiator, string? detail)
        {
            var nisse = direction switch
            {
                HttpDirection.In => 
                    $"{ChevronsRight}  {RequestInQualifier}{(detail is null ? "" : $" ({detail})")} {(initiator is null ? "" : $"{initiator}")}  {ChevronsRight}",
                
                HttpDirection.Out => 
                    $"{ChevronsRight}  {RequestOutQualifier}{(detail is null ? "" : $" ({detail})")}  {ChevronsRight}  {(initiator is null ? "" : $"{initiator}")}",
                
                HttpDirection.Response => 
                    $"{ChevronsLeft}  {(initiator is null ? "" : $"{initiator}{ChevronsRight}")}{ResponseQualifier}{(detail is null ? "" : $" ({detail})")}  {ChevronsLeft}",
                
                _ => string.Empty
            };
            return nisse;
        }
    }
}