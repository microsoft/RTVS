
namespace Microsoft.Languages.Core.Text
{
    /// <summary>
    /// Text range that allows attaching of simple properties
    /// </summary>
    public interface ITextRange<T>: ITextRange
    {
        T Tag { get; set; }
    }
}
