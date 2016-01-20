namespace Microsoft.VisualStudio.R.Package.Repl {
    public interface IRInteractiveSessionProvider {
        IRInteractiveSession GetOrCreate();
    }
}