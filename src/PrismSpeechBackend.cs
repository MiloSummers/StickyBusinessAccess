using System.Runtime.InteropServices;

namespace StickyBusinessAccess;

// Bindings verified against ethindp/prism v0.18.3 include/prism.h.
// Prism uses cdecl, UTF-8 text, size_t indexes, uint64_t IDs and one-byte C bool.
internal sealed class PrismSpeechBackend : ISpeechBackend
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate IntPtr Init(IntPtr config);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void Release(IntPtr handle);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate UIntPtr Count(IntPtr context);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate ulong IdAt(IntPtr context, UIntPtr index);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate IntPtr Name(IntPtr context, ulong id);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int Priority(IntPtr context, ulong id);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate IntPtr Create(IntPtr context, ulong id);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate ulong Features(IntPtr backend);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int Operation(IntPtr backend);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int SpeakText(IntPtr backend,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string text, [MarshalAs(UnmanagedType.I1)] bool interrupt);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate IntPtr VersionString();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate IntPtr ErrorString(int error);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void VoidOperation();
    private const ulong RuntimeSupported=1UL, SupportsSpeak=1UL<<2, SupportsStop=1UL<<7;
    private IntPtr library, context, backend;
    private Release shutdown=null!, free=null!;
    private Create create=null!;
    private Features features=null!;
    private Operation initialize=null!, stop=null!;
    private SpeakText speak=null!;
    private ErrorString errorString=null!;
    private VoidOperation? logShutdown;
    private readonly Action<string> log;
    private readonly List<(ulong id,string name)> readers=new();
    private readonly HashSet<string> reportedIssues=new(StringComparer.Ordinal);
    private bool disposed;
    internal string? SelectedReader { get; private set; }

    internal PrismSpeechBackend(string directory, Action<string> log)
    {
        this.log=log;
        try
        {
            library=NativeLibrary.Load(Path.Combine(directory,"prism.dll"));
            T Bind<T>(string symbol) where T:Delegate => Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(library,symbol));
            var version=Marshal.PtrToStringUTF8(Bind<VersionString>("prism_version_string")());
            if(version!="0.18.3") throw new InvalidOperationException("Expected packaged Prism 0.18.3, found "+version);
            shutdown=Bind<Release>("prism_shutdown"); free=Bind<Release>("prism_backend_free");
            logShutdown=Bind<VoidOperation>("prism_log_shutdown");
            create=Bind<Create>("prism_registry_create"); features=Bind<Features>("prism_backend_get_features");
            initialize=Bind<Operation>("prism_backend_initialize"); stop=Bind<Operation>("prism_backend_stop");
            speak=Bind<SpeakText>("prism_backend_speak"); errorString=Bind<ErrorString>("prism_error_string");
            var count=Bind<Count>("prism_registry_count"); var idAt=Bind<IdAt>("prism_registry_id_at");
            var name=Bind<Name>("prism_registry_name"); var priority=Bind<Priority>("prism_registry_priority");
            context=Bind<Init>("prism_init")(IntPtr.Zero);
            if(context==IntPtr.Zero) throw new InvalidOperationException("prism_init returned null");
            // Enumerate every compiled reader. Exclude synthesis engines: absent readers remain silent,
            // matching the original mod rather than unexpectedly using a system TTS voice.
            var synthesis=new HashSet<string>(StringComparer.Ordinal) {"SAPI","OneCore","AVSpeech","SpeechDispatcher","Spiel","AndroidTextToSpeech","WebSpeechSynthesis"};
            for(ulong index=0;index<count(context).ToUInt64();index++)
            {
                ulong id=idAt(context,new UIntPtr(index));
                string label=Marshal.PtrToStringUTF8(name(context,id))??"";
                if(!synthesis.Contains(label)) readers.Add((id,label));
            }
            readers.Sort((a,b)=>priority(context,b.id).CompareTo(priority(context,a.id)));
            log("Prism 0.18.3 loaded; registered readers: "+string.Join(", ",readers.Select(r=>r.name))+". Availability does not establish audible testing.");
        }
        catch(Exception e)
        {
            Issue("Prism initialization failed: "+e.Message+". Re-extract the complete player package; the game will continue without mod speech.");
            try { Dispose(); } catch(Exception cleanup) { Issue("Prism cleanup failed: "+cleanup.Message); }
        }
    }
    private void Issue(string message) { if(reportedIssues.Add(message)) log(message); }
    private bool Available(IntPtr handle) => (features(handle)&(RuntimeSupported|SupportsSpeak))==(RuntimeSupported|SupportsSpeak);
    private void ReleaseBackend()
    {
        if(backend==IntPtr.Zero) return;
        var old=backend; backend=IntPtr.Zero; SelectedReader=null;
        try { if((features(old)&SupportsStop)!=0) _=stop(old); }
        finally { free(old); }
    }
    public bool Refresh()
    {
        if(disposed||context==IntPtr.Zero) return false;
        if(backend!=IntPtr.Zero && Available(backend)) return true;
        ReleaseBackend();
        foreach(var reader in readers)
        {
            IntPtr candidate=create(context,reader.id);
            if(candidate==IntPtr.Zero) continue;
            bool retain=false;
            try
            {
                if(!Available(candidate)) continue;
                int result=initialize(candidate);
                if(result!=0) { Issue("Prism "+reader.name+" initialization: "+Error(result)); continue; }
                backend=candidate; SelectedReader=reader.name; retain=true;
                log("Prism connected through "+reader.name+".");
                return true;
            }
            finally { if(!retain) free(candidate); }
        }
        Issue("Prism: no supported reader is available. Start a supported reader on the same desktop; connection will be checked again automatically.");
        return false;
    }
    private string Error(int result) => Marshal.PtrToStringUTF8(errorString(result))??result.ToString();
    public bool Speak(string text, bool interrupt)
    {
        if(disposed||backend==IntPtr.Zero) return false;
        // Recheck the selected reader only; full enumeration remains on the existing one-second poll.
        if(!Available(backend)) { ReleaseBackend(); return false; }
        int result=speak(backend,text,interrupt);
        if(result==0) { reportedIssues.Clear(); return true; }
        Issue("Prism speech failed: "+Error(result)+". Will reconnect on the next availability check.");
        ReleaseBackend();
        // Do not retry this text through another reader: delivery may have partially succeeded.
        return false;
    }
    public void Dispose()
    {
        if(disposed) return;
        disposed=true;
        try { ReleaseBackend(); }
        finally
        {
            try { if(context!=IntPtr.Zero) shutdown(context); }
            finally
            {
                context=IntPtr.Zero;
                // Required before unloading if Prism's PRISM_LOG environment option started its logger.
                try { logShutdown?.Invoke(); }
                finally { if(library!=IntPtr.Zero) NativeLibrary.Free(library); library=IntPtr.Zero; }
            }
        }
    }
}
