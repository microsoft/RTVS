namespace Microsoft.R.Editor.Validation.Definitions
{
    /// <summary>
    /// Describes where validation error is located. Since validation is asynchronous, 
    /// actual item positions may change between the time node was validated and 
    /// the moment result reaches task list and the error marker is created.
    /// </summary>
    public enum ValidationErrorLocation
    {
        Node,
        BeforeNode,
        AfterNode,
    }
}
