using System.Text.RegularExpressions;

namespace StickyBusinessAccess;

internal interface ISpeechBackend : IDisposable
{
    bool Refresh();
    bool Speak(string text, bool interrupt);
}

// Independent of Unity and reader APIs so speech behavior can be regression tested.
internal sealed class SpeechService : IDisposable
{
    private readonly ISpeechBackend backend;
    private readonly Action<string> trace, warn;
    private readonly Func<long> clock;
    private string last = "Sticky Business Access loaded. F1 gives contextual help. F2 repeats.";
    private string recent = "";
    private long recentAt;
    private bool wasRunning, reportedFailure, disposed;
    internal SpeechService(ISpeechBackend backend, Action<string> trace, Action<string> warn, Func<long>? clock=null)
    { this.backend=backend; this.trace=trace; this.warn=warn; this.clock=clock ?? (() => Environment.TickCount64); }
    internal static string Clean(string text) => Regex.Replace(Regex.Replace(text ?? "", "<[^>]*>", " "), @"\s+", " ").Trim();
    internal bool CheckConnected()
    {
        if(disposed) return false;
        try
        {
            bool now=backend.Refresh(), connected=now&&!wasRunning;
            wasRunning=now;
            return connected;
        }
        catch(Exception e) { Failure(e); wasRunning=false; return false; }
    }
    internal void Say(string text, bool force=false, bool interrupt=true)
    {
        if(disposed) return;
        text=Clean(text);
        if(text.Length==0) return;
        long now=clock();
        if(!force && text==recent && now-recentAt<500) return;
        last=recent=text; recentAt=now;
        trace("[speech] "+text);
        try { if(backend.Speak(text,interrupt)) reportedFailure=false; }
        catch(Exception e) { Failure(e); wasRunning=false; }
    }
    private void Failure(Exception e)
    { if(!reportedFailure) { reportedFailure=true; warn("Prism speech unavailable: "+e.Message); } }
    internal void Repeat() => Say(last,true);
    public void Dispose()
    {
        if(disposed) return;
        disposed=true;
        try { backend.Dispose(); } catch(Exception e) { Failure(e); }
    }
}
