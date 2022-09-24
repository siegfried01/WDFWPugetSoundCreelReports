#!/usr/bin/env dotnet-script
#nullable enable
#pragma warning disable CS7022
//https://www.elliotdenolf.com/posts/csharp-scripting-using-dotnet-script/
//  dotnet tool install -g dotnet-script
//https://www.nuget.org/packages/Core.HttpClient/
#r "nuget: Core.HttpClient, 3.1.9.12"
#r "nuget: HtmlAgilityPack, 1.11.46"
#r "nuget: Microsoft.Azure.Storage.Blob, 11.2.3"
#r "nuget: WindowsAzure.Storage, 9.3.3"
#r "nuget: Azure.Security.KeyVault.Secrets, 4.4.0"
#r "nuget: Azure.Identity, 1.7.0"
    
using System.Net.Http;
using HtmlAgilityPack;
using Microsoft.WindowsAzure.Storage;
using static System.Environment;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;


public class ScrapeCreelReportsMainProgram
{
    private static string seperator = ",";
    private static string replaceComma = ",";
    public static async Task UploadBlob(string storageAccountConnection, string storageAccountContainer, string blobName, string text)
    {
        // https://www.c-sharpcorner.com/article/using-azure-blob-storage-in-c-sharp/
        // https://stackoverflow.com/questions/56220364/is-there-any-way-of-writing-file-to-azure-blob-storage-directly-from-c-sharp-app
        var storageAccount = CloudStorageAccount.Parse(storageAccountConnection);
        var blobClient = storageAccount.CreateCloudBlobClient();
        var container = blobClient.GetContainerReference(storageAccountContainer);
        var blob = container.GetBlockBlobReference(blobName);
        blob.Properties.ContentType = "text/csv";
        await blob.UploadTextAsync(text);
    }

    static void DisplayChildNodes(HtmlNode nodeElement)
    {
        HtmlNodeCollection childNodes = nodeElement.ChildNodes;

        if (childNodes.Count == 0)
        {
            WriteLine(nodeElement.Name + " has no children");
        }
        else
        {
            var ii = 0;
            foreach (var node in childNodes)
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    WriteLine($"node {ii}={node.OuterHtml}");
                }
                ii++;
            }
        }
    }
    public static System.Text.RegularExpressions.Regex patComma = new System.Text.RegularExpressions.Regex(",");
    static List<List<string>> ExtractDataFromHtmlTableRows(HtmlNode nodeElement)
    {
        HtmlNodeCollection childNodes = nodeElement.ChildNodes;
        List<List<string>> tableData = new();

        if (childNodes.Count == 0)
        {
            WriteLine(nodeElement.Name + " has no children");
        }
        else
        {
            var ii = 0;
            foreach (var trNode in childNodes)
            {
                if (trNode.NodeType == HtmlNodeType.Element)
                {

                    var jj = 0;
                    List<string> row = new();
                    foreach (var tdNode in trNode.ChildNodes)
                    {
                        if (tdNode.NodeType == HtmlNodeType.Element)
                        {
                            row.Add(patComma.Replace(tdNode.InnerHtml.Trim(), replaceComma));
                        }
                        jj++;
                    }
                    tableData.Add(row);
                }
                ii++;
            }
        }
        return tableData;
    }


    private static HttpClient client = new HttpClient();
    public static async Task Main(string[] args)
    {
        var count = 1;
        if ( args.Count() >= 1 )
            Int32.TryParse(args[0], out count);
        var uri = GetEnvironmentVariable("PSCR_URI");// "https://wdfw.wa.gov/fishing/reports/creel/puget";
        System.IO.StringWriter csvCreelReport = new();
        var txtDate = "";

        // Call asynchronous network methods in a try/catch block to handle exceptions.
        try
        {
            string html = await client.GetStringAsync(uri);
            var htmlDoc2 = new HtmlDocument();
            htmlDoc2.LoadHtml(html);
            var htmlTHeadTr = htmlDoc2.DocumentNode.SelectSingleNode($"//body/div/div/div[1]/div[5]/div/main/section/div/section[2]/article/div/div/div/div/div/section[2]/div/div/div[2]/table[{count}]/thead/tr");
            List<string> columnHeaders = new() { "\"Date\"" };
            foreach (var node in htmlTHeadTr.ChildNodes)
            {
                var columnHeader = node.InnerHtml.Trim();
                if (!string.IsNullOrEmpty(columnHeader))
                    columnHeaders.Add('"' + columnHeader.Trim() + '"');
            }
            var headers = $"{string.Join(seperator, columnHeaders)}";
            csvCreelReport.WriteLine(headers);
            WriteLine(headers);
            txtDate = htmlDoc2.DocumentNode.SelectSingleNode($"//body/div/div/div[1]/div[5]/div/main/section/div/section[2]/article/div/div/div/div/div/section[2]/div/div/div[2]/table[{count}]/caption/text()").InnerHtml.Trim();
            txtDate = patComma.Replace(txtDate, replaceComma);
            var htmlTableRows = htmlDoc2.DocumentNode.SelectSingleNode($"//body/div/div/div[1]/div[5]/div/main/section/div/section[2]/article/div/div/div/div/div/section[2]/div/div/div[2]/table[{count}]/tbody");
            var rows = ExtractDataFromHtmlTableRows(htmlTableRows);
            foreach (var row in rows)
            {
                row.Insert(0, '"' + txtDate + '"');
                row[2] = '"' + row[2] + '"';
                row[1] = '"' + row[1] + '"';
                var rowString = string.Join(seperator, row).Trim();
                csvCreelReport.WriteLine(rowString);
                WriteLine(rowString);
            }
        }
        catch (HttpRequestException e)
        {
            WriteLine("\nException Caught!");
            WriteLine("Message :{0} ", e.Message);
        }
        //string? storageAccountConnection = GetEnvironmentVariable("PSCR_STORAGE_ACCOUNT_CONNECTION");
        string? storageAccountContainer = "pzveowxpswgja-siegblobcontainer";//GetEnvironmentVariable("PSCR_CONTAINER_NAME");
        //var blobName = "PSCR_" + DateTime.Parse(txtDate).ToString("yyMMdd_HHmmssddd")+".csv";
        var blobName = "PSCR_" + DateTime.Now.ToString("yyMMdd_HHmmssddd")+".csv";
        var vaultUrl = "https://kv-aadaccessazuresqlperm.vault.azure.net/";
        var secretClient = new SecretClient(vaultUri: new Uri(vaultUrl), credential: new DefaultAzureCredential());
        // Retrieve a secret using the secret client.
        string? storageAccountConnection = secretClient.GetSecret("pzveowxpswgjastgacc-connection").Value.Value;
        
        if (!string.IsNullOrEmpty(storageAccountConnection) && !string.IsNullOrEmpty(storageAccountContainer))
            await UploadBlob(storageAccountConnection, storageAccountContainer, blobName, csvCreelReport.ToString());
        else
            WriteLine("mismatch");
    }
}
if(Args.Count > 0)
    await ScrapeCreelReportsMainProgram.Main(Args.ToArray());
else
    await ScrapeCreelReportsMainProgram.Main(new string[]{});
