using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace TetraPak.XP.Web.Diagnostics
{
    // todo SDK: Document service diagnostics
    public class ServiceDiagnostics : IEnumerable<KeyValuePair<string,object>>
    {
        /// <summary>
        ///   The prefix for timer values.
        /// </summary>
        /// <seealso cref="GetValues"/>
        public const string TimerPrefix = "tpd-time";

        readonly Dictionary<string, object> _values = new();
        
        /// <summary>
        ///   Gets the diagnosed service's elapsed time in milliseconds.
        /// </summary>
        [JsonPropertyName("roundtripTime")]
        public long ElapsedMilliseconds { get; private set; }

        public long? StopTimer() => GetElapsedMs(TimerPrefix);
        
        /// <summary>
        ///   Starts a timer to measure a specified source. 
        /// </summary>
        /// <param name="source">
        ///   Identifies a source to be timed.
        /// </param>
        public void StartTimer(string source)
        {
            source = source == TimerPrefix ? source : timerKey(source); 
            _values[source] = new Timer(DateTime.Now.Ticks);
        }

        /// <summary>
        ///   Returns the elapsed time and, optionally, stops the timer. 
        /// </summary>
        /// <param name="key">
        ///   Identifies the diagnostics source. 
        /// </param>
        /// <param name="stopTimer">
        ///   (optional; default=<c>true</c>)<br/>
        ///   Specifies whether to also stop the timer (if the timer was already stopped the request is ignored).
        /// </param>
        /// <returns>
        ///   The number of ticks that has elapsed since the timer was started until now (or it ended, when stopped).
        /// </returns>
        public long? GetElapsedMs(string? key = null, bool stopTimer = true)
        {
            key = key is null ? TimerPrefix : timerKey(key);
            if (!_values.TryGetValue(key, out var obj) || obj is not Timer timer)
                return null;

            return timer.ElapsedMs(stopTimer);
        }
        
        static string timerKey(string key) => $"{TimerPrefix}-{key}";

        /// <summary>
        ///   Returns all values, optionally filtered with a <paramref name="prefix"/> pattern.
        /// </summary>
        /// <param name="prefix">
        ///   (optional)<br/>
        ///   A prefix pattern for filtering result.
        /// </param>
        /// <returns>
        ///   A collection of <see cref="KeyValuePair{String,Object}"/>.
        /// </returns>
        public IEnumerable<KeyValuePair<string, object>> GetValues(string? prefix = null)
        {
            return string.IsNullOrWhiteSpace(prefix) 
                ? _values 
                : _values.Where(i => i.Key.StartsWith(prefix));
        }

        internal ServiceDiagnostics()
        {
            StartTimer(TimerPrefix);
        }
        
        /// <summary>
        ///   A service diagnostics timer.
        /// </summary>
        public class Timer
        {
            /// <summary>
            ///   Gets the timer start time (ticks).
            /// </summary>
            public long Started { get; }
            
            /// <summary>
            ///   Gets the timer end time (ticks).
            /// </summary>
            public long? Ended { get; private set; }

            /// <summary>
            ///   Returns the elapsed time and, optionally, stops the timer. 
            /// </summary>
            /// <param name="stop">
            ///   (optional; default=<c>true</c>)<br/>
            ///   Specifies whether to also stop the timer (if the timer was already stopped the request is ignored).
            /// </param>
            /// <returns>
            ///   The number of ticks that has elapsed since the timer was started until now (or it ended, when stopped).
            /// </returns>
            public long ElapsedMs(bool stop = true)
            {
                var end = DateTime.Now.Ticks;
                if (stop && !Ended.HasValue)
                    Ended = end;
                
                return (long) (Ended.HasValue
                    ? TimeSpan.FromTicks(Ended.Value - Started).TotalMilliseconds
                    : TimeSpan.FromTicks(end - Started).TotalMilliseconds);
            }

            internal Timer(long now)
            {
                Started = now;
            }
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}