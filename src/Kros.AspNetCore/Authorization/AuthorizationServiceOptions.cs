namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// Authorization service definition.
    /// </summary>
    public class AuthorizationServiceOptions
    {
        /// <summary>
        /// Authorization service name.
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Path name.
        /// </summary>
        public string PathName { get; set; }
    }
}
