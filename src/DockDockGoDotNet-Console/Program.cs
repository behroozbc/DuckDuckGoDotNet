// See https://aka.ms/new-console-template for more information
using DuckDuckGoSearch;

var d=new DDGS();
var results= d.Text("Iran");
results.ForEach(c=>{
    var a= c["title"];
    Console.WriteLine(a);
});