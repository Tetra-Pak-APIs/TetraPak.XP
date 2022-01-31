using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace TetraPak.XP.Logging
{
    /// <summary>
    ///   Assists in creating a well formatted state dump of one or more objects.
    /// </summary>
    /// <seealso cref="RestrictedValueAttribute"/>
    public class StateDump
    {
        static readonly List<AttachedStateDumpHandler> s_attachedStateDumpHandlers = new();
        readonly StateDumpContext _context;

        StackTrace? _stackTrace;

        public const string NoCaption = "__NO_CAPTION__";
        
        /// <summary>
        ///   Adds an object (a "<paramref name="source"/>) to the state dump.
        /// </summary>
        /// <param name="source">
        ///   The object to be added.
        /// </param>
        /// <param name="name">
        ///   (optional; default = <see cref="Type"/> of <paramref name="source"/>)<br/>
        ///   A name for the source. 
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="source"/> was unassigned.
        /// </exception>
        /// <seealso cref="StateDumpContext"/>
        /// <seealso cref="RestrictedValueAttribute"/>
        public async Task AddAsync(object source, string? name = null)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            name ??= source.GetType().FullName;
            await addAsync(source, name!);
        }

        async Task addAsync(object source, string name)
        {
            _context.Append(_context.Indentation);
            _context.Append('"');
            _context.Append(name);
            _context.Append("\": ");
            await source.GetStateDumpAsync(_context);
        }
        
        /// <summary>
        ///   Invoking this method will have a <see cref="StateDump"/> include a stacktrace.   
        /// </summary>
        /// <param name="skipFrames">
        ///   (optional; default = 1)<br/>
        ///   Specifies how many stacktrace frames to be removed from the end of the stacktrace.
        ///   This is to avoid including frames representing calls to the state dump logic itself.
        /// </param>
        /// <returns>
        ///   This object (fluent API).
        /// </returns>
        public StateDump WithStackTrace(int skipFrames = 1)
        {
            _stackTrace = new StackTrace(skipFrames);
            return this;
        }

        /// <summary>
        ///   Overrides the base method to also add a prefix and suffix to the state dump.
        /// </summary>
        /// <returns>
        ///   A <see cref="string"/> presenting the state dump, with a prefix/suffix pair for easy recognition. 
        /// </returns>
        /// <see cref="BuildAsStringAsync"/>
        public override string ToString() => BuildAsStringAsync().Result;

        /// <summary>
        ///   Returns the <see cref="StateDump"/>'s textual representation.
        /// </summary>
        /// <returns>
        ///   A <see cref="string"/> presenting the state dump, with a prefix/suffix pair for easy recognition. 
        /// </returns>
        public async Task<string> BuildAsStringAsync()
        {
            var caption = string.IsNullOrWhiteSpace(_context.Caption) ? "STATE DUMP" : _context.Caption;
            var sb = new StringBuilder();
            if (caption != NoCaption)
            {
                sb.AppendLine($">===== {caption} BEGIN =====<");
            }
            
            sb.AppendLine(_context.ToString());
            if (_stackTrace is {})
            {
                await addStackTraceAsync();
            }

            if (caption != NoCaption)
            {
                sb.AppendLine($">====== {caption} END ======<");
            }
            return sb.ToString();
        }

        async Task addStackTraceAsync()
        {
            await addAsync(_stackTrace!, "StackTrace");
        }

        public static void Attach(AttachedStateDumpHandler handler)
        {
            lock (s_attachedStateDumpHandlers)
            {
                if (!s_attachedStateDumpHandlers.Contains(handler))
                    s_attachedStateDumpHandlers.Add(handler);
            }
        }

        /// <summary>
        ///   initializes the <see cref="StateDump"/>.
        /// </summary>
        /// <param name="context">
        ///   (optional)<br/>
        ///   A (custom) <see cref="StateDumpContext"/>, to be used instead of the one created automatically.
        /// </param>
        public StateDump(StateDumpContext? context = null)
        {
            _context = (context ?? new StateDumpContext(null!)).WithAttachedStateDumps(s_attachedStateDumpHandlers);
        }

        public StateDump(string caption, ILog? log)
        : this(new StateDumpContext(caption, log))
        {
        }
    }
}