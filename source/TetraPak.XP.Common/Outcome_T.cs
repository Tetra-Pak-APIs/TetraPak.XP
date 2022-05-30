using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TetraPak.XP
{
    /// <summary>
    ///   A boolean value that can also carry another value. This is
    ///   very useful as a typical return value where you need an indication
    ///   for "success" and, when successful, a value.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of value carried by the <see cref="Outcome{T}"/>.
    /// </typeparam>
    /// <remarks>
    ///   Instances of type <see cref="Outcome{T}"/> can be implicitly cast to
    ///   a <c>bool</c> value. Very useful for testing purposes.
    /// </remarks>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class Outcome<T> : Outcome
    {
        /// <summary>
        ///   The value carried by the object.
        /// </summary>
        public T? Value { get; private set; }

        /// <summary>
        ///   Creates a <see cref="Outcome{T}"/> that equals <c>true</c> when cast to a
        ///   <see cref="bool"/> value.
        /// </summary>
        /// <param name="value">
        ///   The value (of type <typeparamref name="T"/>) to be carried by the
        ///   new <see cref="Outcome{T}"/> object.
        /// </param>
        /// <returns>
        ///   A <see cref="Outcome{T}"/> that represents a <c>true</c> value when
        ///   cast as a <see cref="bool"/> while also carrying a specified value.
        /// </returns>
        public static Outcome<T> Success(T value) => new(true, null!, null!, value);

        /// <summary>
        ///   Creates a <see cref="Outcome{T}"/> that equals <c>false</c> when cast to a
        ///   <see cref="bool"/> value.
        /// </summary>
        /// <param name="message">
        ///   A message to be carried by the <see cref="Outcome"/> object.
        ///   The message is simply passed as a message for the <see cref="Exception"/>
        ///   (useful for error handling).
        /// </param>
        /// <returns>
        ///   A <see cref="Outcome{T}"/> that represents a <c>true</c> value when
        ///   cast as a <see cref="Boolean"/> while also carrying a specified value.
        /// </returns>
        public new static Outcome<T> Fail(string message) => new(false, null!, new Exception(message), default!);

        /// <summary>
        ///   Creates and returns a failed outcome that carries an <see cref="Exception"/> as well as a value.
        /// </summary>
        /// <param name="exception">
        ///   Assigns <see cref="Exception"/>.
        /// </param>
        /// <param name="value">
        ///   Assigns <see cref="Value"/>.
        /// </param>
        /// <returns>
        ///   An <see cref="Outcome{T}"/> to indicate failure.
        /// </returns>
        public static Outcome<T> Fail(Exception exception, T value) => new(false, null!, exception, value);

        /// <summary>
        ///   Creates a <see cref="Outcome{T}"/> that equals <c>false</c> when cast to a
        ///   <see cref="bool"/> value.
        /// </summary>
        /// <param name="exception">
        ///   An <see cref="Exception"/> to be carried by the <see cref="Outcome{T}"/> object
        ///   (for error handling).
        /// </param>
        /// <returns>
        ///   A <see cref="Outcome{T}"/> that represents a <c>true</c> value when
        ///   cast as a <see cref="Boolean"/> while also carrying a specified value.
        /// </returns>
        public new static Outcome<T> Fail(Exception exception) => new(false, null!, exception, default!);

        /// <summary>
        ///   Creates and returns a 'Failed' <see cref="Outcome{T}"/> due to a operation being canceled.
        ///   The method simple invokes <see cref="Fail(System.Exception)"/> with a
        ///   <see cref="TaskCanceledException"/>.
        /// </summary>
        /// <param name="message">
        ///   (optional; default=<see cref="Outcome.DefaultCanceledMessage"/>)<br/>
        ///   A message to be used with the <see cref="TaskCanceledException"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="Outcome{T}"/> that represents a <c>true</c> value when
        ///   cast as a <see cref="Boolean"/> while also carrying a specified value.
        /// </returns>
        public new static Outcome<T> Cancel(string? message = null) 
            => Fail(new TaskCanceledException(message ?? DefaultCanceledMessage));
        
        public new Outcome<T> WithValue(string key, object? value, bool overwrite = false)
        {
            SetValue(key, value, overwrite);
            return this;
        }
        
        public override string ToString()   
        {
            return Evaluated ? $"success : {value()}" : $"fail{errorMessage()}";

            string value() => (ReferenceEquals(default, Value) ? "(null)" : Value.ToString())!;

            string errorMessage() => Exception is null ? "" : $" ({Exception.Message})";
        }

        /// <summary>
        ///   Implicitly converts the outcome to the expected value.
        /// </summary>
        /// <param name="outcome">
        ///   The outcome.
        /// </param>
        /// <returns>
        ///   The expected (successful) outcome value.
        /// </returns>
        public static implicit operator T?(Outcome<T?>? outcome) => outcome.Value;

        protected Outcome(bool evaluated, string? message, Exception? exception, T? value) 
        : base(evaluated, message, exception)
        {
            Value = value;
        }
    }

    public static class OutcomeHelper
    {
        public static bool WasCancelled(this Outcome outcome) => outcome.Exception is TaskCanceledException;
    }
}