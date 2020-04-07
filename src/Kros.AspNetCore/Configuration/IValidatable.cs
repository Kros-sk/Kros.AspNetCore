namespace Kros.AspNetCore.Configuration
{
    /// <summary>
    /// Interface for objects that can validate their state.
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// Validates the object. If the object state is not valid, method must throw exception (any exception is OK).
        /// </summary>
        void Validate();
    }
}
