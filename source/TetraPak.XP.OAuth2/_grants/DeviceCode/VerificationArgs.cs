using System;
using System.Threading.Tasks;

namespace TetraPak.XP.OAuth2.DeviceCode
{
    public class VerificationArgs
    {
        readonly IDeviceCodeGrantService _service;
        
        // todo Consider supporting cancelling the request
        // readonly CancellationTokenSource? _cancellationTokenSource;

        /// <summary>
        ///   Gets the user code to be verified. 
        /// </summary>
        public string UserCode { get;  }

        /// <summary>
        ///   Gets the URL to be used for verifying the user code. 
        /// </summary>
        public Uri VerificationUri { get; }

        /// <summary>
        ///   Gets the amount of time allowed for verifying the user code.
        /// </summary>
        public TimeSpan ExpiresIn { get; }
        
        /// <summary>
        ///   Cancels the verification process.
        /// </summary>
        public Task<bool> CancelAsync()
        {
            return _service.CanBeCanceled 
                ? _service.CancelAsync() 
                : Task.FromResult(false);
        }

        internal VerificationArgs(
            IDeviceCodeGrantService service,
            DeviceCodeAuthCodeResponseBody authCodeResponseBody)
        {
            _service = service;
            VerificationUri = new Uri(authCodeResponseBody.VerificationUri);
            UserCode = authCodeResponseBody.UserCode;
            ExpiresIn = TimeSpan.FromSeconds(authCodeResponseBody.ExpiresIn);
        }
    }
}