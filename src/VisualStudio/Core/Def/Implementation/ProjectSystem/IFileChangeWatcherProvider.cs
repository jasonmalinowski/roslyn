namespace Microsoft.VisualStudio.LanguageServices.Implementation.ProjectSystem
{
    internal interface IFileChangeWatcherProvider
    {
        FileChangeWatcher Watcher { get; }
    }
}