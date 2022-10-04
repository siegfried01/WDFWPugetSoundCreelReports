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
    private static bool upload = true;
    private static bool runInGitHub = true;
    public static async Task UploadBlob(string storageAccountConnection, string storageAccountContainer, string blobName, string text)
    {
        if (upload)
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
        else
        {
            File.WriteAllText(System.Environment.ExpandEnvironmentVariables("%DN%\\"+ blobName), text);
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
    public static string GenerateExeclXml(List<List<string>> catchData)
    {
        var rowCount = catchData.Count;
        var xml1 = 
              "<?xml version=\"1.0\"?>\n"
            + "<?mso-application progid=\"Excel.Sheet\"?>\n"
            + "<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"\n"
            + " xmlns:o=\"urn:schemas-microsoft-com:office:office\"\n"
            + " xmlns:x=\"urn:schemas-microsoft-com:office:excel\"\n"
            + " xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"\n"
            + " xmlns:html=\"http://www.w3.org/TR/REC-html40\">\n"
            + " <DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">\n"
            + "  <Author>Siegfried Heintze</Author>\n"
            + "  <LastAuthor>Siegfried Heintze</LastAuthor>\n"
            + "  <Created>2022-09-27T16:45:28Z</Created>\n"
            + "  <Version>16.00</Version>\n"
            + " </DocumentProperties>\n"
            + " <OfficeDocumentSettings xmlns=\"urn:schemas-microsoft-com:office:office\">\n"
            + "  <AllowPNG/>\n"
            + " </OfficeDocumentSettings>\n"
            + " <ExcelWorkbook xmlns=\"urn:schemas-microsoft-com:office:excel\">\n"
            + "  <WindowHeight>28380</WindowHeight>\n"
            + "  <WindowWidth>32767</WindowWidth>\n"
            + "  <WindowTopX>32767</WindowTopX>\n"
            + "  <WindowTopY>32767</WindowTopY>\n"
            + "  <ProtectStructure>False</ProtectStructure>\n"
            + "  <ProtectWindows>False</ProtectWindows>\n"
            + " </ExcelWorkbook>\n"
            + " <Styles>\n"
            + "  <Style ss:ID=\"Default\" ss:Name=\"Normal\">\n"
            + "   <Alignment ss:Vertical=\"Bottom\"/>\n"
            + "   <Borders/>\n"
            + "   <Font ss:FontName=\"Calibri\" x:Family=\"Swiss\" ss:Size=\"8\" ss:Color=\"#000000\"/>\n"
            + "   <Interior/>\n"
            + "   <NumberFormat/>\n"
            + "   <Protection/>\n"
            + "  </Style>\n"
            + "  <Style ss:ID=\"s62\">\n"
            + "   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\" ss:Rotate=\"90\"/>\n"
            + "  </Style>\n"
            + "  <Style ss:ID=\"s63\">\n"
            + "   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>\n"
            + "  </Style>\n"
            + "  <Style ss:ID=\"s65\">\n"
            + "   <NumberFormat ss:Format=\"Fixed\"/>\n"
            + "  </Style>\n"
            + "  <Style ss:ID=\"s66\">\n"
            + "    <NumberFormat ss:Format=\"ddd\\ d\\-mmm\\-yy\"/>\n"
            + "  </Style>            \n"
            + " </Styles>\n"
            + " <Worksheet ss:Name=\"PSCR_220927_094042Tue\">\n"
            + "  <Names>\n"
            + $"<NamedRange ss:Name=\"_FilterDatabase\" ss:RefersTo=\"=PSCR_220927_094042Tue!R1C1:R{rowCount}C15\" ss:Hidden=\"1\"/></Names>"
            + $"<Table ss:ExpandedColumnCount=\"15\" ss:ExpandedRowCount=\"{rowCount+1}\" x:FullColumns=\"1\" x:FullRows=\"1\" ss:DefaultColumnWidth=\"42\" ss:DefaultRowHeight=\"11.25\">"
            + "<Column ss:Width=\"49.5\"/>\n"
            + "<Column ss:Width=\"189\"/>\n"
            + "<Column ss:AutoFitWidth=\"0\" ss:Width=\"12.75\"/>\n"
            + "<Column ss:Width=\"198.75\"/>\n"
            + "<Column ss:Width=\"16.5\"/>\n"
            + "<Column ss:Width=\"18.75\"/>\n"
            + "<Column ss:Width=\"24\"/>\n"
            + "<Column ss:Width=\"16.5\"/>\n"
            + "<Column ss:Width=\"21.75\"/>\n"
            + "<Column ss:Width=\"16.5\" ss:Span=\"5\"/>\n"
            + "<Row ss:AutoFitHeight=\"0\" ss:Height=\"103.5\">\n"
            + "    <Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">Date                        </Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n"
            + "    <Cell ss:StyleID=\"s63\"><Data ss:Type=\"String\">Ramp/site                   </Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n"
            + "    <Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">Marine Area                 </Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n"
            + "    <Cell ss:StyleID=\"s63\"><Data ss:Type=\"String\">Catch area                  </Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n"
            + "    <Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\"># Interviews (Boat or Shore)</Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n"
            + "    <Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">Anglers                     </Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n"
            + "    <Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">Chinook (per angler)        </Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n"
            + "    <Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">Chinook                     </Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n"
            + "    <Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">Coho per Angular            </Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n"
            + "    <Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">Coho                        </Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n"
            + "    <Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">Chum                        </Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n"
            + "    <Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">Pink                        </Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n"
            + "    <Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">Sockeye                     </Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n"
            + "    <Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">Lingcod                     </Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n"
            + "    <Cell ss:StyleID=\"s62\"><Data ss:Type=\"String\">Halibut                     </Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n"
            + "</Row>\n";
        string xml2 = MakeDataRows(catchData);
        var xml3 = 
                "</Table>\n"
              + "<WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">\n"
              + " <PageSetup>\n"
              + "  <Header x:Margin=\"0.3\"/>\n"
              + "  <Footer x:Margin=\"0.3\"/>\n"
              + "  <PageMargins x:Bottom=\"0.75\" x:Left=\"0.7\" x:Right=\"0.7\" x:Top=\"0.75\"/>\n"
              + " </PageSetup>\n"
              + " <Unsynced/>\n"
              + " <Print>\n"
              + "  <ValidPrinterInfo/>\n"
              + "  <HorizontalResolution>600</HorizontalResolution>\n"
              + "  <VerticalResolution>600</VerticalResolution>\n"
              + " </Print>\n"
              + " <Selected/>\n"
              + " <Panes>\n"
              + "  <Pane>\n"
              + "   <Number>3</Number>\n"
              + "   <ActiveRow>2</ActiveRow>\n"
              + "   <ActiveCol>2</ActiveCol>\n"
              + "  </Pane>\n"
              + " </Panes>\n"
              + " <ProtectObjects>False</ProtectObjects>\n"
              + " <ProtectScenarios>False</ProtectScenarios>\n"
              + "</WorksheetOptions>\n"
              + "<AutoFilter x:Range=\"R1C1:R25C15\"\n"
              + " xmlns=\"urn:schemas-microsoft-com:office:excel\">\n"
              + "</AutoFilter>\n"
              + "</Worksheet>\n"
              + "</Workbook> \n";

        return xml1 + xml2 + xml3;
    }

    private static string MakeDataRows(List<List<string>> catchData)
    {
        var xml = "";
        foreach (var row in catchData)
        {
            xml += "  <Row ss:AutoFitHeight=\"0\">\n"
                + "      <Cell ss:StyleID=\"s66\">                             <Data ss:Type=\"DateTime\">" + row[0] + "</Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n" // date
                + "      <Cell>                                                <Data ss:Type=\"String\">" + row[1] + "</Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n" // ramp
                + "      <Cell>                                                <Data ss:Type=\"Number\">" + "0" + "</Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n" // marine area
                + "      <Cell>                                                <Data ss:Type=\"String\">" + row[2] + "</Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n" // catch area
                + "      <Cell>                                                <Data ss:Type=\"Number\">" + row[3] + "</Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n" // # interviews
                + "      <Cell>                                                <Data ss:Type=\"Number\">" + row[4] + "</Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n" // anglers
                + "      <Cell ss:StyleID=\"s65\">                             <Data ss:Type=\"Number\">" + row[5] + "</Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n" // chinook per angler
                + "      <Cell>                                                <Data ss:Type=\"Number\">" + row[6] + "</Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n" // chinook
                + "      <Cell ss:StyleID=\"s65\" ss:Formula=\"=RC[1]/RC[-3]\"><Data ss:Type=\"Number\">"+ double.Parse(row[7]) / double.Parse(row[4]) +"</Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n" // coho per anguler
                + "      <Cell>                                                <Data ss:Type=\"Number\">" + row[7] + "</Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n" // coho
                + "      <Cell>                                                <Data ss:Type=\"Number\">" + row[8] + "</Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n" // chum
                + "      <Cell>                                                <Data ss:Type=\"Number\">" + row[9] + "</Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n" // pink
                + "      <Cell>                                                <Data ss:Type=\"Number\">" + row[10] + "</Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n" // sockeye
                + "      <Cell>                                                <Data ss:Type=\"Number\">" + row[11] + "</Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n" // lingcod
                + "      <Cell>                                                <Data ss:Type=\"Number\">" + row[12] + "</Data><NamedCell ss:Name=\"_FilterDatabase\"/></Cell>\n" // Halibut
                + "    </Row>\n";
        }

        return xml;
    }
    private static HttpClient client = new HttpClient();
    public static async Task Main(string[] args)
    {
        var uri = GetEnvironmentVariable("PSCR_URI")?? "https://wdfw.wa.gov/fishing/reports/creel/puget";
        System.IO.StringWriter csvCreelReport = new();
        List<List<string>> rows = new();
        string txtDate = "";

        // Call asynchronous network methods in a try/catch block to handle exceptions.
        try
        {
            var xpath = "/html/body/div/div/div/div/div[5]/div/main/section/div/section/article/div/div/div/div/div/section[2]/div/div/div[2]"; // Tue Oct 04 08:58 2022
            string html = await client.GetStringAsync(uri);
            var htmlDoc2 = new HtmlDocument();
            htmlDoc2.LoadHtml(html);
            var htmlTHeadTr = htmlDoc2.DocumentNode.SelectSingleNode(
                    $"{xpath}/table/thead/tr" // Tue Oct 04 08:39 2022
                   //"//body/div/div/div[1]/div[5]/div/main/section/div/section[2]/article/div/div/div/div/div/section[2]/div/div/div[2]/table/thead/tr" 
                );
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
            txtDate = htmlDoc2.DocumentNode.SelectSingleNode(
                    $"{xpath}/table/caption/text()" // Tue Oct 04 08:40 2022
                   //"//body/div/div/div[1]/div[5]/div/main/section/div/section[2]/article/div/div/div/div/div/section[2]/div/div/div[2]/table/caption/text()" 
                ).InnerHtml.Trim();
            HtmlNode htmlTableRows = htmlDoc2.DocumentNode.SelectSingleNode(
                    $"{xpath}/table/tbody" // Tue Oct 04 08:40 2022
                   //"//body/div/div/div[1]/div[5]/div/main/section/div/section[2]/article/div/div/div/div/div/section[2]/div/div/div[2]/table/tbody"
                );
            rows = ExtractDataFromHtmlTableRows(htmlTableRows);
            foreach (var row in rows)
            {
                row.Insert(0, '"' + txtDate + '"');
                row[2] = '"' + row[2] + '"';
                row[1] = '"' + row[1] + '"';
                var rowString = string.Join(seperator, row).Trim();
                csvCreelReport.WriteLine(rowString);
                WriteLine(rowString);
            }
            //string? storageAccountConnection = GetEnvironmentVariable("PSCR_STORAGE_ACCOUNT_CONNECTION");
            string? storageAccountContainer = "pzveowxpswgja-siegblobcontainer";//GetEnvironmentVariable("PSCR_CONTAINER_NAME");
            var blobName = runInGitHub ? "PSCR_" + DateTime.Parse(txtDate).ToString("yyMMdd_HHmmssddd")+".csv" :  "PSCR_" + DateTime.Now.ToString("yyMMdd_"+(upload?"HHmmss":"")+"ddd") + ".csv";
            var vaultUrl = "https://kv-aadaccessazuresqlperm.vault.azure.net/";
            var secretClient = upload ? new SecretClient(vaultUri: new Uri(vaultUrl), credential: new DefaultAzureCredential()) : null;
            // Retrieve a secret using the secret client.
            string storageAccountConnection = upload &&  secretClient is not null ? secretClient.GetSecret("pzveowxpswgjastgacc-connection").Value.Value : "";
        
            if (!upload || !string.IsNullOrEmpty(storageAccountConnection) && !string.IsNullOrEmpty(storageAccountContainer))
                await UploadBlob(storageAccountConnection, storageAccountContainer, blobName, csvCreelReport.ToString());
            else
                WriteLine("mismatch1");
            blobName =  "PSCR_" + (runInGitHub ? DateTime.Parse(txtDate).ToString("yyMMdd_HHmmssddd") :  "PSCR_" + DateTime.Now.ToString("yyMMdd_"+(upload?"HHmmss":"")+"ddd")) + ".xml";
            txtDate = DateTime.Parse(txtDate).ToString("yyyy-MM-ddTHH:mm:ss.fff"); // convert to XML date format
            csvCreelReport = new();
            if (htmlTableRows is not null)
            {
                rows = ExtractDataFromHtmlTableRows(htmlTableRows);
                foreach (var row in rows)
                {
                    row.Insert(0, txtDate);
                    var rowString = string.Join(seperator, row).Trim();
                    csvCreelReport.WriteLine(rowString);                
                }

                if (!upload || !string.IsNullOrEmpty(storageAccountConnection) && !string.IsNullOrEmpty(storageAccountContainer))
                    await UploadBlob(storageAccountConnection, storageAccountContainer, blobName, GenerateExeclXml(rows));
                else
                    WriteLine("mismatch2");
            }
        }
        catch (HttpRequestException e)
        {
            WriteLine("\nException Caught!");
            WriteLine("Message :{0} ", e.Message);
        }
        WriteLine($"All done");
    }
}
if(Args.Count > 0)
    await ScrapeCreelReportsMainProgram.Main(Args.ToArray());
else
    await ScrapeCreelReportsMainProgram.Main(new string[]{});
