using System;
using System.Threading;

namespace TetraPak.XP.OAuth2.DeviceCode
{
    public class VerificationArgs
    {
        // todo Consider supporting cancelling the request
        readonly CancellationTokenSource _cancellationTokenSource;

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
        public void Cancel()
        {
            if (_cancellationTokenSource.Token.CanBeCanceled)
                _cancellationTokenSource.Cancel();
        }

        internal VerificationArgs(DeviceCodeAuthCodeResponseBody authCodeResponseBody, CancellationTokenSource cancellationTokenSource)
        {
            VerificationUri = new Uri(authCodeResponseBody.VerificationUri);
            UserCode = authCodeResponseBody.UserCode;
            ExpiresIn = TimeSpan.FromSeconds(authCodeResponseBody.ExpiresIn);
            _cancellationTokenSource = cancellationTokenSource;
        }
    }
}