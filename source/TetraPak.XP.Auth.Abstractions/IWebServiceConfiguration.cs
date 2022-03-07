namespace TetraPak.XP.Auth.Abstractions
{
    public interface IWebServiceConfiguration : IAuthConfiguration
    {
        /// <summary>
        ///   Gets the service base path (including schema and host elements).
        ///   This can be inherited from parent configuration.  
        /// </summary>
        string BaseAddress { get; }

        /// <summary>
        ///   Sets the service endpoint path. 
        /// </summary>
        string ServicePath { get; }
    }
}