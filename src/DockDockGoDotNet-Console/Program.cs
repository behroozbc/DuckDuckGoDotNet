// See https://aka.ms/new-console-template for more information
using System.ComponentModel.DataAnnotations;
using DockDockGoDotNet;
using DockDockGoDotNet.AI;

var d=new DuckDuckGoSearch();
var results= d.Chat("Pls answer the question in the following and put the answer in start and end with '|||', The question: write an esay about, what is your opinion about iran.",Model.Llama3370b);
Console.Write(results);
// var search=await d.NewsAsync("Iran");
// foreach (var item in search)
// {
//     var a=item["title"];
//     Console.WriteLine(a);
// }