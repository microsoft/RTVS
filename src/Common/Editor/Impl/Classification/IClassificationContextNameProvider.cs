namespace Microsoft.Languages.Editor.Classification
{
    public interface IClassificationContextNameProvider<T>
    {
        string GetClassificationContextName(T t);
    }
}
