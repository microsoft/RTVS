
namespace Microsoft.R.Editor.Validation.Definitions
{
    /// <summary>
    /// Inmplemented by a provider of AST node validator. 
    /// Exported via MEF for a particular content type.
    /// </summary>
    public interface IValidatorProvider
    {
        /// <summary>
        /// Creates HTML element validator
        /// </summary>
        /// <returns></returns>
        IValidator GetValidator();
    }
}
