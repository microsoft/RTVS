namespace Microsoft.VisualStudio.R.Package.Repl {
    public interface IRInteractiveProvider {
        IRInteractive GetOrCreate();
    }
}