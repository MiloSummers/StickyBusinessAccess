using StickyBusinessAccess;

internal static class SpeechRegression
{
    internal static void Run(string? runtimeDirectory)
    {
        int checks=0;
        void Check(bool condition,string label) { if(!condition) throw new Exception(label); checks++; }
        long now=1000;
        var backend=new FakeSpeechBackend(); var warnings=new List<string>();
        var service=new SpeechService(backend,_=>{},warnings.Add,()=>now);
        service.Say(" <b>Menu</b>\n   selected ");
        Check(backend.Calls.Single()==("Menu selected",true),"Clean text and interrupt by default");
        service.Say("Menu selected");
        Check(backend.Calls.Count==1,"Suppress immediate duplicates");
        now+=499; service.Say("Menu selected");
        Check(backend.Calls.Count==1,"Suppress duplicates inside existing 500 ms window");
        now++; service.Say("Menu selected");
        Check(backend.Calls.Count==2,"Same text remains available after 500 ms");
        service.Say("Menu selected",true);
        Check(backend.Calls.Count==3,"Preserve intentional forced announcements");
        service.Repeat();
        Check(backend.Calls.Count==4 && backend.Calls.Last()==("Menu selected",true),"F2 repeats and interrupts");
        service.Say("Validation detail",interrupt:false);
        Check(backend.Calls.Last()==("Validation detail",false),"Preserve appended validation and screen text");
        service.Say("<b> </b>");
        Check(backend.Calls.Count==5,"Ignore empty cleaned text");
        backend.Calls.Clear();
        for(int i=0;i<100;i++) service.Say("Focus "+i);
        Check(backend.Calls.Count==100 && backend.Calls.All(c=>c.interrupt),"Rapid navigation always requests replacement");
        Check(backend.Calls.Last().text=="Focus 99","Latest navigation retains exact content");
        Check(service.CheckConnected() && !service.CheckConnected(),"One connection announcement per transition");
        backend.Connected=false;
        Check(!service.CheckConnected(),"Reader exit is safe");
        service.Say("Disconnected status");
        Check(backend.Calls.Count==100,"No reader means no speech delivery");
        backend.Connected=true;
        Check(service.CheckConnected(),"Reader can connect after launch or restart");
        service.Repeat();
        Check(backend.Calls.Last()==("Disconnected status",true),"Repeat retains status produced while disconnected");
        backend.Throw=true;
        Check(!service.CheckConnected(),"Availability failures do not escape");
        service.Say("Failure A"); service.Say("Failure B");
        Check(warnings.Count==1,"Repeated backend failures log once");
        backend.Throw=false; service.Say("Recovered"); backend.Throw=true; service.Say("Failure after recovery");
        Check(warnings.Count==2,"Recovery permits a new failure warning");
        backend.Throw=false; service.Dispose(); service.Dispose(); service.Say("After shutdown");
        Check(backend.Disposals==1 && backend.Calls.Last().text=="Recovered","Idempotent disposal and no calls after shutdown");
        using(var missing=new PrismSpeechBackend(Path.Combine(Path.GetTempPath(),Guid.NewGuid().ToString()),_=>{}))
        { Check(!missing.Refresh() && !missing.Speak("ignored",true),"Missing native DLL fails gracefully"); }
        if(runtimeDirectory!=null)
        {
            var nativeLog=new List<string>();
            using var native=new PrismSpeechBackend(runtimeDirectory,nativeLog.Add);
            Check(nativeLog.Any(l=>l.StartsWith("Prism 0.18.3 loaded;")),"Official native DLL loads and all bound symbols resolve");
            Check(nativeLog.Any(l=>l.Contains("NVDA")&&l.Contains("JAWS")&&l.Contains("UIA")),"Native registry contains reader backends");
            // Do not emit or stop speech from the user's reader in automated tests.
        }
        Console.WriteLine(checks+" speech checks passed (no audible screen-reader testing claimed).");
    }
    private sealed class FakeSpeechBackend : ISpeechBackend
    {
        internal bool Connected=true, Throw;
        internal int Disposals;
        internal readonly List<(string text,bool interrupt)> Calls=new();
        public bool Refresh() { if(Throw) throw new InvalidOperationException("fixture"); return Connected; }
        public bool Speak(string text,bool interrupt)
        { if(Throw) throw new InvalidOperationException("fixture"); if(!Connected) return false; Calls.Add((text,interrupt)); return true; }
        public void Dispose() { Disposals++; }
    }
}
