﻿using System;
using System.Threading.Tasks;
using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.OAuth2.AuthCode;

namespace TetraPak.XP.OAuth2.ClientCredentials
{
    /// <summary>
    ///   Implementors of this interface are able to acquire a token using the
    ///   OAuth Client Credentials grant. 
    /// </summary>
    public interface IClientCredentialsGrantService : IGrantService
    {
        /// <summary>
        ///   Requests a token using the OAuth Client Credentials grant.   
        /// </summary>
        /// <param name="options">
        ///   Specifies the details for how to perform the grant request.
        /// </param>
        /// <returns>
        ///   An <see cref="Exception"/> instance indicating success/failure, and the requested token
        ///   when successful; otherwise an <see cref="Outcome"/>.
        /// </returns>
        Task<Outcome<Grant>> AcquireTokenAsync(GrantOptions options);
    }
}