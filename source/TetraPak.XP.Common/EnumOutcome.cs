using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TetraPak.XP
{
#if DEBUG
    [DebuggerDisplay("{" + nameof(DebugDisplay) + "}")]
#endif
    public class EnumOutcome<T> : Outcome<IReadOnlyCollection<T>>
    {
        /// <summary>
        ///   Gets the total available number of entities.
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        ///   Gets the number of entities in the outcome.
        /// </summary>
        public virtual int Count { get; }

#if DEBUG
        public string DebugDisplay => $"Count={Count}; Total={TotalCount}";
#endif

        public static EnumOutcome<T> Success(T singleValue, int totalCount = 0) =>
            new(true, new T[] { singleValue }, totalCount);

        public static EnumOutcome<T> Success(IReadOnlyCollection<T> value, int totalCount = 0) =>
            new(true, value, totalCount == 0 ? value.Count : totalCount);

        public static EnumOutcome<T> Fail() => new(false, default!, 0);

        public new static EnumOutcome<T> Fail(Exception exception) => new(false, default!, 0, exception);

        public static EnumOutcome<T> Fail(T[] value, Exception exception) => new(false, value, 0, exception);

        protected EnumOutcome(
            bool evaluated,
            IReadOnlyCollection<T>? value,
            int totalCount,
            Exception? exception = null)
        : base(evaluated, null, exception, value)
        {
            TotalCount = totalCount;
            Count = value?.Count ?? 0;
        }
    }
}