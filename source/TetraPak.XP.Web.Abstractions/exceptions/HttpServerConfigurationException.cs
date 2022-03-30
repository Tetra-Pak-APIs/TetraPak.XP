using System;
using HttpStatusCode = TetraPak.XP.Microsoft.HttpStatusCode;

// ReSharper disable once CheckNamespace
namespace TetraPak.XP;

/// <summary>
///   Thrown to reflect configuration issues.
/// </summary>
public sealed class HttpServerConfigurationException : HttpServerException
{
    /// <summary>
    ///   (static property)<br/>
    ///   Gets or sets a default error message to be used when no message is specified.
    /// </summary>
    public static string DefaultErrorMessage { get; set; } = "Internal server error (probably configuration)";
    
    /// <summary>
    ///   Initializes the <see cref="HttpServerConfigurationException"/>.
    /// </summary>
    /// <param name="message">
    ///   (optional; default=<see cref="DefaultErrorMessage"/>)<br/>
    ///   A message describing the server configuration issue.
    /// </param>
    /// <param name="innerException">
    ///   (optional)<br/>
    ///   The exception that is the cause of the current exception.
    /// </param>
    public HttpServerConfigurationException(string? message = null, Exception? innerException = null)
    : base(HttpStatusCode.InternalServerError, message ?? "Internal server error (probably configuration)", innerException)
    {
    }
}