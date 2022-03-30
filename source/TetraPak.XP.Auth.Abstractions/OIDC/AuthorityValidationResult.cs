// using System;
//
// namespace TetraPak.XP.Auth.Abstractions.OIDC
// {
//     public struct AuthorityValidationResult
//     {
//         // ReSharper disable once InconsistentNaming
//         public static readonly AuthorityValidationResult SuccessResult = new(true, null);
//
//         public string ErrorMessage { get; }
//
//         public bool Success { get; }
//
//         private AuthorityValidationResult(bool success, string? message)
//         {
//             if (!success && string.IsNullOrEmpty(message))
//                 throw new ArgumentException("A message must be provided if success=false.", nameof(message));
//
//             ErrorMessage = message;
//             Success = success;
//         }
//
//         public static AuthorityValidationResult CreateError(string? message)
//         {
//             return new AuthorityValidationResult(false, message);
//         }
//
//         public override string ToString()
//         {
//             return Success ? "success" : ErrorMessage;
//         }
//     }
// }