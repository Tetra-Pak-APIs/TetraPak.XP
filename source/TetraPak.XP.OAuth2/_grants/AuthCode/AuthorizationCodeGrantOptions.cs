using TetraPak.XP.Auth.Abstractions;

namespace TetraPak.XP.OAuth2.AuthCode
{
    public static class AuthorizationCodeGrantOptionsHelper
    {
        const string DataKeyHtmlResponseOnSuccess = "__htmlResponse_success";
        const string DataKeyHtmlResponseOnError = "__htmlResponse_error";
        
        public static GrantOptions WithHtmlResponse(
            this GrantOptions options,
            string? htmlResponseOnSuccess,
            string? htmlResponseOnError)
        {
            if (!string.IsNullOrEmpty(htmlResponseOnSuccess))
            {
                options.SetData(DataKeyHtmlResponseOnSuccess, htmlResponseOnSuccess!);
            }
            if (!string.IsNullOrEmpty(htmlResponseOnError))
            {
                options.SetData(DataKeyHtmlResponseOnError, htmlResponseOnError!);
            }

            return options;
        }

        internal static string? GetHtmlResponseOnSuccess(this GrantOptions options)
        {
            return options.GetData<string>(DataKeyHtmlResponseOnSuccess);
        }
        
        internal static string? GetHtmlResponseOnError(this GrantOptions options)
        {
            return options.GetData<string>(DataKeyHtmlResponseOnError);
        }
    }
}