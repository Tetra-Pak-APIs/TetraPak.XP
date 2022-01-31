using System;

namespace TetraPak.XP
{
    /// <summary>
    ///   To be thrown when there is a conflict of identities.
    /// </summary>
    public class IdentityConflictException : Exception
    {
        /// <summary>
        ///   Gets the conflicting identity.
        /// </summary>
        public string Identity { get; }

        /// <summary>
        ///   Initializes the <see cref="IdentityConflictException"/>.
        /// </summary>
        /// <param name="identity">
        ///   Assigns the <see cref="Identity"/>
        /// </param>
        /// <param name="message">
        ///   The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">
        ///   The exception that is the cause of the current exception, or a <c>null</c> reference
        ///   (<c>Nothing</c> in Visual Basic) if no inner exception is specified.
        /// </param>
        public IdentityConflictException(string identity, string? message = null, Exception? innerException = null)
        : base(message, innerException)
        {
            Identity = identity;
        }
    }
}