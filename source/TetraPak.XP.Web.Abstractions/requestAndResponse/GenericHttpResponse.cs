namespace TetraPak.XP.Web.Abstractions
{
    /// <summary>
    ///   An abstract representation of a HTTP request (note: the class itself is not <c>abstract</c>).
    /// </summary>
    public sealed class GenericHttpResponse : AbstractHttpMessage
    {
        /// <summary>
        ///   Gets or sets the response status code.
        /// </summary>
        public int StatusCode { get; set; }
    }
}