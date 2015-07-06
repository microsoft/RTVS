
namespace Microsoft.R.Editor.Completion.Definitions
{
    // This cannot be an enum because it's used as field on custom MEF attribute
    // (RCompletionProviderAttribute) and MEF uses that class while initializing.
    // If that class contains custom type attributes, it will load assembly containing
    // type definitions. This resulted in R editor assembly being loaded on VS startup.
    public static class CompletionTypes
    {
        public const string Keywords = "Keywords";
        public const string IntrinsicFunctions = "IntrinsicFunctions";
        public const string LibraryFunctions = "LibraryFunctions";
        public const string None = "None";
    }
}
