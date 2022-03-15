using System;
using System.Threading;

namespace TetraPak.XP.OAuth2.DeviceCode
{
    public class VerificationArgs
    {
        // todo Consider supporting cancelling the request
        readonly CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        ///   
        /// </summary>
        public Uri VerificationUri { get; }

        public string UserCode { get;  }

        public TimeSpan ExpiresIn { get; }
        
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