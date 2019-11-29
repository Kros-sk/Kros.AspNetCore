using System.ComponentModel.DataAnnotations;

namespace Kros.AspNetCore.Configuration
{
    /// <summary>
    /// Base class for settings, where properties can be annotated using data annotations attributes and
    /// and the state of the object is validated against them.
    /// </summary>
    public abstract class AnnotatedSettingsBase : IValidatable
    {
        /// <summary>
        /// Validates the object using data annotations validator (<see cref="Validator"/>).
        /// </summary>
        public virtual void Validate()
            => Validator.ValidateObject(this, new ValidationContext(this), validateAllProperties: true);
    }
}
