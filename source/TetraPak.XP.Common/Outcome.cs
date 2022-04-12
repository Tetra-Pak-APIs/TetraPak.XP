using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TetraPak.XP
{
    /// <summary>
    ///   A base class for a boolean value that can also carry a message and an exception. 
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class Outcome
    {
        readonly string? _message;
        
        /// <summary>
        ///   Gets or sets a default message to reflect an outcome that failed due to the operaion
        ///   being cancelled, and not message was specified. 
        /// </summary>
        public static string DefaultCanceledMessage { get; set; } = "Operation was canceled";

        /// <summary>
        ///   The internal dictionary of arbitrary data. 
        /// </summary>
        protected Dictionary<string, object?>? Data { get; private set; }

        /// <summary>
        ///   Gets a flag indicating whether the outcome reflects a cancelled operation.
        /// </summary>
        public static bool IsCanceled { get; private set; }

        /// <summary>
        ///   Gets the value used when objects of this class is cast to a <see cref="bool"/> value.
        /// </summary>
        protected bool Evaluated { get; }

        /// <summary>
        ///   A message to be carried by the <see cref="Outcome"/> object
        ///   (useful for error handling).
        /// </summary>
        public string Message => _message ?? Exception?.Message ?? string.Empty; 

        /// <summary>
        ///   An <see cref="Exception"/> to be carried by the <see cref="Outcome"/> object
        ///   (useful for error handling).
        /// </summary>
        public Exception? Exception { get; }

        public T GetValue<T>(string key, T useDefault = default!)
        {
            if (Data is null || !Data.TryGetValue(key, out var obj) || obj is not T tv)
                return useDefault;

            return tv;
        }
        
        public bool TryGetValue<T>(string key, out T? value)
        {
            value = default;
            if (Data is null || !Data.TryGetValue(key, out var obj) || obj is not T tv)
                return false;

            value = tv;
            return true;
        }
        
        public Outcome WithValue(string key, object? value, bool overwrite = false)
        {
            SetValue(key, value, overwrite);
            return this;
        }

        protected void SetValue(string key, object? value, bool overwrite = false)
        {
            if (Data is null)
            {
                Data = new Dictionary<string, object?> { { key, value } };
                return;
            }

            if (!Data.TryGetValue(key, out _))
            {
                Data.Add(key, value);
                return;
            }

            if (!overwrite)
                throw new IdentityConflictException(key, $"Cannot add tag '{key}' to outcome (was already added)");

            Data[key] = value;
        }
        
        /// <summary>
        ///   Implicitly casts the <see cref="bool"/> to a <see cref="Outcome"/> value.
        /// </summary>
        public static implicit operator bool(Outcome self) => self.Evaluated;

        /// <summary>
        ///   Constructs and returns an <see cref="Outcome"/> that equals <c>true</c>
        ///   when cast to a <see cref="bool"/> value.
        /// </summary>
        /// <returns>
        ///   A <see cref="Outcome{T}"/> that represents a <c>true</c> value when
        ///   cast as a <see cref="bool"/> while also carrying a specified value.
        /// </returns>
        public static Outcome Success() => new(true, null!, null!);
        
        /// <summary>
        ///   Constructs and returns an <see cref="Outcome"/> with <see cref="Message"/>
        ///   that equals <c>true</c> when cast to a <see cref="bool"/> value.
        /// </summary>
        /// <returns>
        ///   A <see cref="Outcome{T}"/> that represents a <c>true</c> value when
        ///   cast as a <see cref="bool"/> while also carrying a specified value.
        /// </returns>
        public static Outcome Success(string message) => new(true, message, null!);
        
        /// <summary>
        ///   Creates and returns an <see cref="Outcome"/> that equals <c>false</c> when cast to a
        ///   <see cref="bool"/> value.
        /// </summary>
        /// <param name="message">
        ///   A message to be carried by the <see cref="Outcome"/> object.
        ///   The message is simply passed as a message for the  <see cref="Exception"/>
        ///   (useful for error handling).
        /// </param>
        /// <returns>
        ///   A <see cref="Outcome"/> that represents a <c>true</c> value when
        ///   cast as a <see cref="Boolean"/> while also carrying a specified value.
        /// </returns>
        /// <seealso cref="Fail(System.Exception)"/>
        public static Outcome Fail(string message) => new(false, null, new Exception(message));

        /// <summary>
        ///   Creates and returns an <see cref="Outcome"/> that equals <c>false</c> when cast to a
        ///   <see cref="bool"/> value.
        /// </summary>
        /// <param name="exception">
        ///   An <see cref="Exception"/> to be carried by the <see cref="Outcome"/> object
        ///   (for error handling).
        /// </param>
        /// <returns>
        ///   A <see cref="Outcome"/> that represents a <c>true</c> value when
        ///   cast as a <see cref="Boolean"/> while also carrying a specified value.
        /// </returns>
        public static Outcome Fail(Exception exception) => new(false, null!, exception);

        /// <summary>
        ///   Creates and returns a 'Failed' <see cref="Outcome"/> due to a operation being canceled.
        ///   The method simple invokes <see cref="Fail(System.Exception)"/> with a
        ///   <see cref="TaskCanceledException"/>.
        /// </summary>
        /// <param name="message">
        ///   (optional; default=<see cref="DefaultCanceledMessage"/>)<br/>
        ///   A message to be used with the <see cref="TaskCanceledException"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="Outcome"/> that represents a <c>true</c> value when
        ///   cast as a <see cref="Boolean"/> while also carrying a specified value.
        /// </returns>
        public static Outcome Cancel(string? message = null)
        {
            IsCanceled = true;
            return Fail(new TaskCanceledException(message ?? DefaultCanceledMessage));
        }

        /// <summary>
        ///   Overrides base implementation to reflect evaluated state ("success" / "fail").
        /// </summary>
        public override string ToString() =>
            IsCanceled 
                ? "cancelled" 
                : Evaluated 
                    ? "success" 
                    : "fail";

        /// <summary>
        ///   Initializes a <see cref="Outcome"/>.
        /// </summary>
        /// <param name="evaluated">
        ///   Initializes the <see cref="Evaluated"/> property.
        /// </param>
        /// <param name="message">
        ///   Initializes the <see cref="Message"/> property.
        /// </param>
        /// <param name="exception">
        ///   Initializes the <see cref="Outcome"/> property.
        /// </param>
        protected Outcome(bool evaluated, string? message, Exception? exception)
        {
            Evaluated = evaluated;
            _message = message;
            Exception = exception;
        }
    }
}
