using StickyBusinessAccess;
void Check(bool condition,string message) {if(!condition)throw new Exception(message);}
string good="""{"schemaVersion":1,"locale":"en","entries":{"test-id":{"shortName":"Test name","description":"Reviewed detail","variants":{"test-variant":{"shortName":"Variant name"}}}}}""";
var catalogue=DescriptionCatalogue.Parse(good);
Check(catalogue.Resolve("test-id",null,"Game name").ShortName=="Test name","Override precedence");
Check(catalogue.Resolve("test-id","test-variant",null).ShortName=="Variant name","Variant precedence");
Check(catalogue.Resolve("unknown",null,"Game name").ShortName=="Game name","Localized fallback");
Check(catalogue.Resolve("unknown",null,null).Detail==null,"Do not invent a visual description");
Check(catalogue.Resolve("TEST-ID",null,null).ShortName=="Unlabelled sticker TEST-ID","IDs are case sensitive");
foreach(string bad in new[]{good.Replace("\"schemaVersion\":1","\"schemaVersion\":9"),good.Replace("\"Test name\"","\" \""),good.Replace("\"locale\":\"en\"","\"locale\":\"en\",\"locale\":\"en\""),"not json"})
{
    bool rejected=false;try{DescriptionCatalogue.Parse(bad);}catch{rejected=true;}
    Check(rejected,"Invalid catalogue must be rejected");
}
Console.WriteLine("9 catalogue checks passed.");
Check(NavigationRules.Move(0,3,-1)==2,"Reverse navigation wraps");
Check(NavigationRules.Move(2,3,1)==0,"Forward navigation wraps");
Check(NavigationRules.Move(-1,0,1)==-1,"Empty screen has no focus");
Check(NavigationRules.Move(-1,3,-1)==2,"Reverse navigation enters last control");
string longText=string.Join(" ",Enumerable.Repeat("Credits text",400));
var pages=NavigationRules.Pages(longText,650).ToArray();
Check(pages.All(p=>p.Length<=650),"Long credits are bounded");
Check(string.Join(" ",pages)==longText,"Credits paging preserves all text in order");
Check(string.Concat(NavigationRules.Pages(new string('x',1500),650))==new string('x',1500),"Unbroken long text is preserved");
Console.WriteLine("7 navigation and text paging checks passed.");

Check(TextEditFeedback.Deleted("cat","ca")=="t","Backspace last character");
Check(TextEditFeedback.Deleted("cat","ct")=="a","Middle deletion");
Check(TextEditFeedback.Deleted("red cat","cat")=="red ","Selected range deletion");
Check(TextEditFeedback.Deleted("a\U0001F431b","ab")=="\U0001F431","Emoji deletion remains complete");
Check(TextEditFeedback.Spoken(" ")=="space","Space is spoken");
Check(TextEditFeedback.Spoken(".")=="period","Punctuation has a spoken name");
Check(TextEditFeedback.At("cat",1)=="a","Cursor character review");
Check(TextEditFeedback.At("cat",3)=="End of text","End boundary review");
Console.WriteLine("8 editing feedback checks passed.");
Check(DayLengthRules.Cost(30,0)==30,"Normal preserves native action cost");
Check(DayLengthRules.Cost(30,1)==24,"1.25x day length");
Check(DayLengthRules.Cost(30,2)==20,"1.5x day length");
Check(DayLengthRules.Cost(30,3)==15,"2x day length");
Check(DayLengthRules.Cost(30,4)==0,"Unlimited action cost");
Check(DayLengthRules.Cost(0,2)==0,"Native limitless zero cost remains zero");
Check(DayLengthRules.Cost(1,3)==1,"Rounding cannot grant free finite-mode actions");
Check(DayLengthRules.Cost(15,3)==8,"Relaxed costs use their own base value");
Console.WriteLine("8 day length checks passed.");
SpeechRegression.Run(args.Length>0 ? args[0] : null);
