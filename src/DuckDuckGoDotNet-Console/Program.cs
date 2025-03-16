// See https://aka.ms/new-console-template for more information

using DuckDuckGoDotNet;
using DuckDuckGoDotNet.AI;

var d=new DuckDuckGoSearch();
var results= d.Chat("Tell me about Iran's history",Model.Llama3370b);
Console.Write(results);
// var search=await d.NewsAsync("Iran");
// foreach (var item in search)
// {
//     var a=item["title"];
//     Console.WriteLine(a);
// }