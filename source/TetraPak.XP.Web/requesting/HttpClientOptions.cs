using System.Net.Http;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Web.credentialsAndTokens;

namespace TetraPak.XP.Web
{
    /// <summary>
    ///   Used to configure a <see cref="HttpClient"/> through a <see cref="IHttpClientProvider"/>.
    /// </summary>
    public class HttpClientOptions
    {
        /// <summary>
        ///   (default=<c>false</c>)<br/>
        ///   Gets or sets a value specifying whether the requested <see cref="HttpClient"/> should be
        ///   transient (otherwise a singleton instance till be returned). 
        /// </summary>
        public bool IsClientTransient { get; set; }
        
        /// <summary>
        ///   Gets or sets an authentication header value to be used for the requested client.
        /// </summary>
        public ActorToken? ActorToken { get; set; }

        /// <summary>
        ///   Gets or sets an (optional) authorization service. When set this is an indicator that the
        ///   requested client should be automatically authorized as per the configured <see cref="AuthConfig"/>.   
        /// </summary>
        public IAuthorizationService? AuthorizationService { get; set; }
        
        /// <summary>
        ///   A custom <see cref="HttpMessageHandler"/> to be used by the requested <see cref="HttpClient"/>.
        ///   Can be assigned with <see cref="WithMessageHandler"/>.
        /// </summary>
        /// <seealso cref="WithMessageHandler"/>
        public HttpMessageHandler? MessageHandler { get; private set; }

        /// <summary>
        ///   Gets or sets the configuration required for authenticating the client. 
        /// </summary>
        public IServiceAuthConfig? AuthConfig { get; set; }

        /// <summary>
        ///   (intended for internal use; default=<c>false</c>)<br/>
        ///   Gets a value specifying whether to force a new client authorization (replacing any cached authorization). 
        /// </summary>
        /// <seealso cref="RequestForcedAuthorization"/>
        public bool ForceAuthorization { get; private set; }

        /// <summary>
        ///   Clears the <see cref="ForceAuthorization"/>, forcing a renewed client authorization.
        /// </summary>
        public void RequestForcedAuthorization(bool value = true)
        {
            ForceAuthorization = value;
        }

        /// <summary>
        ///   Fluid API for assigning a (custom) <see cref="HttpMessageHandler"/> to the requested
        ///   HTTP client.
        /// </summary>
        /// <returns>
        ///   <c>this</c>
        /// </returns>
        /// <seealso cref="MessageHandler"/>
        public HttpClientOptions WithMessageHandler(HttpMessageHandler messageHandler)
        {
            MessageHandler = messageHandler;
            return this;
        }
        
        /// <summary>
        ///   Fluid API for requesting client authorization.
        /// </summary>
        /// <param name="actorToken">
        ///   The actor token of the current request (if any).
        /// </param>
        /// <param name="authorizationService">
        ///   (optional)<br/>
        ///   A (custom) authorization service to be used for authorizing the requested client.
        /// </param>
        public HttpClientOptions WithAuthorization(
            ActorToken? actorToken,
            IAuthorizationService? authorizationService = null)
        {
            ActorToken = actorToken;
            AuthorizationService = authorizationService;
            return this;
        }

        /// <summary>
        ///   Initializes the <see cref="HttpClientOptions"/>.
        /// </summary>
        /// <param name="isClientTransient">
        ///   Initializes <see cref="IsClientTransient"/>.
        /// </param>
        public HttpClientOptions(bool isClientTransient = false)
        {
            IsClientTransient = isClientTransient;
            ForceAuthorization = false;
        }
    }
}