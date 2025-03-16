// See https://aka.ms/new-console-template for more information
using System.ComponentModel.DataAnnotations;
using DuckDuckGoSearchDotNet;

var d=new DuckDuckGoSearch();
// var results= d.Chat("Iran");
// Console.Write(results);
var search=await d.News("Iran");
foreach (var item in search)
{
    var a=item["title"];
    Console.WriteLine(a);
}