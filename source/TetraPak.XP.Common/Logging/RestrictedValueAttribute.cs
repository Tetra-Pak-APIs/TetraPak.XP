using System;
using TetraPak.XP.Logging;

namespace TetraPak.XP
{
    /// <summary>
    ///   Decorating a property with this attribute indicates the property should not be disclosed
    ///   in log, traces or similar output when the declaring object's state is presented. 
    /// </summary>
    /// <seealso cref="StateDump"/>
    /// <seealso cref="WebLoggerHelper.GetStateDump"/>
    [AttributeUsage(AttributeTargets.Property)]
    public class RestrictedValueAttribute : Attribute
    {
        /// <summary>
        ///   (default=<see cref="LogLevel.None"/>)<br/>
        ///   Specifies the lowest <see cref="LogLevel"/> at which the restricted value can be disclosed.
        /// </summary>
        public LogRank DisclosureLogLevel { get; set; } = LogRank.None;
    }
}