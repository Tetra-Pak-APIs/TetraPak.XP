using System;

namespace TetraPak.XP.Mobile
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class TetraPakRuntimeEnvironmentAttribute : Attribute
    {
        public RuntimeEnvironment RuntimeEnvironment { get; set; }
        
        public TetraPakRuntimeEnvironmentAttribute(MobileRuntimeEnvironment runtimeEnvironment)
        {
            if (runtimeEnvironment == MobileRuntimeEnvironment.Unknown)
                throw new ArgumentOutOfRangeException(
                    nameof(runtimeEnvironment), 
                    $"Assembly runtime environment cannot be '{runtimeEnvironment}'");
                
            RuntimeEnvironment = (RuntimeEnvironment) runtimeEnvironment;
        }
    }

    /// <summary>
    ///   This enum value can be used to identity a runtime environment.
    /// </summary>
    public enum MobileRuntimeEnvironment
    {
        /// <summary>
        ///   Runtime environment is unknown/unresolved.
        /// </summary>
        Unknown,
        
        /// <summary>
        ///   Represents a runtime environment used for very early development and/or proof of concept.
        ///   This type of environment must be completely isolated from a production environment.
        /// </summary>
        Sandbox,
        
        /// <summary>
        ///   Represents a runtime environment that changes very frequently, to be used for development
        ///   purposes only. 
        ///   This type of environment must be completely isolated from a production environment.
        /// </summary>
        Development,
        
        // "'"migration" tends to have a lot of names ...
        /// <summary>
        ///   Represents a runtime environment that emulates a <see cref="Production"/> environment very closely,
        ///   to test quality and ensure a solution's function before being deployed for production use.
        /// </summary>
        Migration,
        
        /// <summary>
        ///   Equivalent to <see cref="Migration"/>. 
        /// </summary>
        Test = Migration,
        
        /// <summary>
        ///   Equivalent to <see cref="Migration"/>. 
        /// </summary>
        Testing = Migration,

        /// <summary>
        ///   Equivalent to <see cref="Migration"/>. 
        /// </summary>
        Staging = Migration,
        
        /// <summary>
        ///   Represents a runtime environment that is fully operational, with access to production level
        ///   services and data. Solutions running in this runtime environment should have been carefully
        ///   tested to ensure stability and functionality.
        /// </summary>
        Production
    }
}