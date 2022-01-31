#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TetraPak.XP.Logging
{
     /// <summary>
    ///   Used to specify data for getting the current state.
    /// </summary>
    public class StateDumpContext
    {
        public string? Caption { get; }
        
        internal StringBuilder StringBuilder { get; }

        internal ILog? Log { get; private set; }

        /// <summary>
        ///   (default=<c>false</c>)<br/>
        ///   Gets or sets a value specifying whether to retain a trailing member separator (',').
        /// </summary>
        public bool IsRetainingTrailingSeparator { get; set; }

        public void Append(string value)
        {
            StringBuilder.Append(value);
        }
        
        public void Append(char value)
        {
            StringBuilder.Append(value);
        }
        
        public void AppendLine(string value)
        {
            StringBuilder.AppendLine(value);
        }

        public void Remove(int startIndex, int length)
        {
            try
            {
                StringBuilder.Remove(startIndex, length);
            }
            catch (Exception e) // nisse
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public int Length => StringBuilder.Length;

        public override string ToString() => StringBuilder.ToString();

        internal AttachedStateDumpHandler[]? AttachedStateDumpsHandlers { get; private set; }

        public Indentation Indentation { get; set; }

        /// <summary>
        ///   (fluent API)<br/>
        ///   Assigns the <see cref="Indentation"/> property and returns <c>this</c>.
        /// </summary>
        public StateDumpContext WithIndentation(Indentation indentation)
        {
            Indentation = indentation;
            return this;
        }

        public StateDumpContext WithRetainedTrailingSeparator()
        {
            IsRetainingTrailingSeparator = true;
            return this;
        }

        internal StateDumpContext WithAttachedStateDumps(IEnumerable<AttachedStateDumpHandler> handlers) 
        {
            AttachedStateDumpsHandlers = handlers.ToArray();
            return this;
        }

        // /// <summary>
        // ///   Specifies that restricted values (that will normally be dumped as redacted values) can be
        // ///   disclosed for specified logger if that logger is declaring a <see cref="LogLevel"/> that
        // ///   matches the restricted value's decorated <see cref="RestrictedValueAttribute.DisclosureLogLevels"/>. 
        // /// </summary>
        // /// <param name="logger">
        // ///   A targeted logger provider.
        // /// </param>
        // /// <returns>
        // ///   This object (fluent API).
        // /// </returns>
        // public StateDumpContext WithTargetLogger(ILogger logger)
        // {
        //     Logger = logger;
        //     return this;
        // }

        /// <summary>
        ///   Initializes the <see cref="StateDumpContext"/> with default options.
        /// </summary>
        /// <param name="caption">
        ///   
        /// </param>
        /// <param name="log">
        ///   (optional)<br/>
        ///   A target logger provider. Adding this to the options is necessary if you need to disclose
        ///   restricted values, based on the declared <see cref="LogLevel"/>. 
        /// </param>
        /// <seealso cref="RestrictedValueAttribute"/>
        public StateDumpContext(string? caption, ILog? log = null)
        {
            Caption = caption;
            Log = log;
            StringBuilder = new StringBuilder();
            Indentation = new Indentation(3);
        }
    }

     public static class StateDumpHelper
     {
         public static StateDumpContext Indent(this StateDumpContext context) => context.WithIndentation(context.Indentation.Push());
         
         public static StateDumpContext Outdent(this StateDumpContext context) => context.WithIndentation(context.Indentation.Pop());

     }
}