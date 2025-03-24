// See https://aka.ms/new-console-template for more information

using DuckDuckGoDotNet;
using DuckDuckGoDotNet.AI;

var d = new DuckDuckGoSearch(timeout:100);
var oldchats=new List<ChatResponse>();
oldchats.Add(new ChatResponse{Content="تو یه ربات مترجم به زبان انگلیسی هستی هر متنی بهت دادن رو به بهترین شکل ترجمه کن.",Role=ChatRole.User});
oldchats.Add(new (){Content="متن خورجی ات رو ترجمه ات رو داخل ||| قرار بده",Role=ChatRole.User});
var results = d.Chat("متن زیر رو به انگلیسی ترجمه کن: " + File.ReadAllText("E:\\repos\\DockDockGoDotNet\\README.md"), Model.O3Mini, oldchats);
Console.Write(results);
await File.WriteAllTextAsync("./output.txt", results);
var search=await d.TextAsync("Iran");
foreach (var item in search)
{
    var a=item.Title;
    Console.WriteLine(a);
}