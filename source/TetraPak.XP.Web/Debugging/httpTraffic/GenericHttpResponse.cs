namespace TetraPak.AspNet.Debugging
{
    /// <summary>
    ///   An abstract representation of a HTTP request (note: the class itself is not <c>abstract</c>).
    /// </summary>
    public class GenericHttpResponse : AbstractHttpMessage
    {
        /// <summary>
        ///   Gets or sets the response status code.
        /// </summary>
        public int StatusCode { get; set; }
    }
}