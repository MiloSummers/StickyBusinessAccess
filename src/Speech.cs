namespace StickyBusinessAccess;

// All existing accessibility handlers retain their announcement text and call semantics.
internal static class Speech
{
    private static SpeechService? service;
    internal static string Clean(string text) => SpeechService.Clean(text);
    internal static void Initialize(string directory)
    {
        Shutdown();
        service = new SpeechService(new PrismSpeechBackend(directory, message => {
            if(message.StartsWith("Prism 0.18.3 loaded;") || message.StartsWith("Prism connected through ")) Plugin.Logger.LogInfo(message);
            else Plugin.Logger.LogWarning(message);
        }),
            message => Plugin.Logger.LogDebug(message), message => Plugin.Logger.LogWarning(message));
    }
    internal static bool CheckConnected() => service?.CheckConnected() ?? false;
    internal static void Say(string text, bool force=false, bool interrupt=true) => service?.Say(text, force, interrupt);
    internal static void Repeat() => service?.Repeat();
    internal static void Shutdown() { service?.Dispose(); service = null; }
}
