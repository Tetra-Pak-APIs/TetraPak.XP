using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using TetraPak.XP;
using TetraPak.XP.CLI;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.Web.Http;

namespace nugt.policies
{
    [NugetPolicy(Name)]
    sealed class PushNugetPacksPolicy : DistributeNugetPolicy // instantiated via NugetPolicy attribute
    {
        readonly TetraPakHttpClientProvider _clientProvider;
        const string Name = "push";
        const string ArgApiKey1 = "-ak";                          // <key>
        const string ArgApiKey2 = "-api-key";                     // -- " --
        const string ArgSkipDuplicate1 = "-sd";                   // (flag)
        const string ArgSkipDuplicate2 = "--skip-duplicate";      // -- " --
        const string ArgDeleteOnSuccess1 = "-dos";                // (flag)
        const string ArgDeleteOnSuccess2 = "--delete-on-success"; // -- " --

        protected override bool IsAssumingReleaseBinFolder => false;

        string? ApiKey { get; set; }

        bool SkipDuplicate { get; set; }

        bool DeleteOnSuccess { get; set; }

        public override async Task<Outcome> RunAsync()
        {
            foreach (var nugetPackageFile in NugetPackageFiles!)
            {
                var outcome = !IsSimulating
                    ? await pushAsync(nugetPackageFile)
                    : Outcome.Success();
                if (!outcome)
                    return outcome;

                WriteToConsole($"Pushed package {nugetPackageFile} to {SourceValue}");
                if (!DeleteOnSuccess)
                    continue;
                
                try
                {
                    if (!IsSimulating)
                    {
                        File.Delete(nugetPackageFile.PhysicalPath);
                    }
                    WriteToConsole($"Deleted {nugetPackageFile.PhysicalPath}", ConsoleColor.Magenta);
                }
                catch (Exception ex)
                {
                    ex = new Exception($"Failed when deleting nuget {nugetPackageFile.Name}, pushing it", ex);
                    return Outcome.Fail(ex);
                }
            }
            
            return Outcome.Success();
        }

        protected override Outcome<Uri> OnResolveRemoteNugetRepository(string uriString)
        {
            // todo consider option to configure multiple 'well known' repository names
            if (uriString.Equals("nuget.org", StringComparison.InvariantCultureIgnoreCase))
                return Outcome<Uri>.Success(new Uri("https://www.nuget.org/api/v2/package"));

            return Uri.TryCreate(uriString, UriKind.Absolute, out var uri)
                ? Outcome<Uri>.Success(uri)
                : Outcome<Uri>.Fail($"Not a valid URI: {uriString}");
        }

        // overridden to ensure the source is not a local folder 
        protected override Outcome<DirectoryInfo> OnResolveLocalFolder(string value)
        {
            var outcome = base.OnResolveLocalFolder(value);
            return outcome
                ? Outcome<DirectoryInfo>.Fail($"Expected remote nuget repository (not local folder: '{value}')")
                : outcome;
        }

        protected override Outcome TryInit(CommandLineArgs args)
        {
            var outcome = base.TryInit(args);
            if (!outcome)
                return outcome;

            if (args.TryGetValue(out var apiKey, ArgApiKey1, ArgApiKey2))
            {
                ApiKey = apiKey;
            }

            SkipDuplicate = args.TryGetFlag(ArgSkipDuplicate1, ArgSkipDuplicate2);
            DeleteOnSuccess = args.TryGetFlag(ArgDeleteOnSuccess1, ArgDeleteOnSuccess2);
            
            return Outcome.Success();
        }

        async Task<Outcome> pushAsync(IFileInfo nugetPackageFile)
        {
            var clientOutcome = await _clientProvider.GetHttpClientAsync();
            if (!clientOutcome)
                return clientOutcome;

            var client = clientOutcome.Value!;
            using var content = new MultipartFormDataContent(); 
            await using var fileStream = new FileStream(nugetPackageFile.PhysicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileStreamContent = new StreamContent(fileStream);
            content.Add(fileStreamContent, "package", "package.nupkg");
            
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = TargetService,
                Content = content
            };
            request.Headers.Add("X-NuGet-Protocol-Version", "4.1.0");
            if (ApiKey.IsAssigned())
            {
                request.Headers.Add("X-NuGet-ApiKey", ApiKey!);
            }

            // todo Manage rate limits
            try
            {
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Conflict && SkipDuplicate)
                        return Outcome.Success();
                    
                    return Outcome.Fail(new HttpServerException(
                        response, 
                        $"Failed when pushing nuget {nugetPackageFile.Name}"));
                }
            }
            catch (Exception ex)
            {
                ex = new Exception($"Failed when pushing nuget {nugetPackageFile.Name}", ex);
                return Outcome.Fail(ex);
            }

            return Outcome.Success();
        }

        public PushNugetPacksPolicy(CommandLineArgs args, ILog log)
        : base(args, log)
        {
            _clientProvider = new TetraPakHttpClientProvider();
        }
    }
}