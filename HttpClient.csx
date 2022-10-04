#!/usr/bin/env dotnet-script
#r "nuget: Core.HttpClient, 3.1.9.12"
#r "nuget: HtmlAgilityPack, 1.11.46"

using System.Net.Http;
using static System.Console;
using HtmlAgilityPack;
static readonly HttpClient client = new HttpClient();
var uri = "https://wdfw.wa.gov/fishing/reports/creel/puget";
try {
    WriteLine($"There are {Args.Count} args: {string.Join(", ", Args.Select(arg => $"{arg}"))}");
    var count = 1;
    if ( Args.Count >= 1 )
        Int32.TryParse(Args[0], out count);
    string html = await client.GetStringAsync(uri);
    var htmlDoc = new HtmlDocument();
    htmlDoc.LoadHtml(html);
    var htmlTables = htmlDoc.DocumentNode.SelectNodes("//body/div/div/div/div/div[5]/div/main/section/div/section/article/div/div/div/div/div/section[2]/div/div/div[2]/table");
    WriteLine($"count={htmlTables.Count}");
    count = Math.Min(htmlTables.Count, count);
    var htmlTable = htmlDoc.DocumentNode.SelectSingleNode($"//body/div/div/div/div/div[5]/div/main/section/div/section/article/div/div/div/div/div/section[2]/div/div/div[2]/table[{count}]");
    WriteLine($"{htmlTable.OuterHtml}");
}
catch(HttpRequestException e){
    WriteLine("Exception Caught!");   
    WriteLine("Message :{0} ",e.Message);
}
