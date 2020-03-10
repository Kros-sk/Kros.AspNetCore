namespace Kros.AspNetCore.Authorization
{
    /// <summary>
    /// JWT authorization options for api services. Contains all authorization schemes.
    /// </summary>
    public class ApiJwtAuthorizationOptions
    {
        /// <summary>
        /// List of schemes.
        /// </summary>
        public ApiJwtAuthorizationScheme[] Schemes { get; set; }
    }
}
