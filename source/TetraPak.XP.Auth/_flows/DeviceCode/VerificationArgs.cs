using System;
using System.Threading;

namespace TetraPak.XP.Auth.DeviceCode
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

        internal VerificationArgs(DeviceCodeCodeResponseBody responseBody, CancellationTokenSource cancellationTokenSource)
        {
            VerificationUri = new Uri(responseBody.VerificationUri);
            UserCode = responseBody.UserCode;
            ExpiresIn = TimeSpan.FromSeconds(responseBody.ExpiresIn);
            _cancellationTokenSource = cancellationTokenSource;
        }
    }
}