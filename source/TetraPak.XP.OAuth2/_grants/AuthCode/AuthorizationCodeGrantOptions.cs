using System.IO;
using System.Threading.Tasks;
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

        public static GrantOptions WithHtmlResponseHandlers(
            this GrantOptions options,
            Task<string>? htmlResponseFileOnSuccessDelegate,
            Task<string>? htmlResponseFileOnErrorDelegate)
        {
            if (htmlResponseFileOnSuccessDelegate is {})
            {
                options.SetDataHandler(DataKeyHtmlResponseOnSuccess, () => htmlResponseFileOnSuccessDelegate);
            }
            if (htmlResponseFileOnErrorDelegate is {})
            {
                options.SetDataHandler(DataKeyHtmlResponseOnError, () => htmlResponseFileOnErrorDelegate);
            }

            return options;
        }

        internal static async Task<string?> GetHtmlResponseOnSuccessAsync(this GrantOptions options)
        {
            return await options.GetDataAsync<string>(DataKeyHtmlResponseOnSuccess);
        }
        
        internal static async Task<string?> GetHtmlResponseOnErrorAsync(this GrantOptions options)
        {
            return await options.GetDataAsync<string>(DataKeyHtmlResponseOnError);
        }
    }

    public delegate Task<string> HtmlFileLoaderDelegate(FileInfo htmlFileInfo);
}