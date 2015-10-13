using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Web;
using System.Web.Http;
using RestSharp;

namespace SyncIDS.API.Controllers
{
    public class DocsController : ApiController
    {
        [HttpGet, Route("docs/generate")]
        public HttpResponseMessage Generate(string firmName, string clientNumber, string matterNumber, string filingIds = "no", string skipArt = "no")
        {
            Render(firmName, clientNumber, matterNumber, filingIds, skipArt);

            var name = $"{firmName}-{clientNumber}-{matterNumber}";
            var baseFolder = string.Format($"C:\\syncids\\{name}");
            var filesFolder = $"{baseFolder}\\files";

            var fileName = $"{name}-PendingIDS.Zip";
            var resultingFileName = $"{baseFolder}\\{fileName}";

            // Create directory
            if (Directory.Exists(baseFolder)) Directory.Delete(baseFolder, true);
            Directory.CreateDirectory(baseFolder);

            // download zip file
            var webClient = new WebClient();
            var downloadUrl = $"http://syncids2.com/{fileName}";
            var downloadTarget = $"{baseFolder}\\original.zip";
            webClient.DownloadFile(downloadUrl, downloadTarget);
            // extract zip file and delete it
            ZipFile.ExtractToDirectory(downloadTarget, filesFolder);
            File.Delete(downloadTarget);

            var blankForm = Directory.GetFiles(filesFolder, "Blank-*.pdf").First();

            var xmlFiles = Directory.GetFiles(filesFolder, "*.xml");
            foreach (var xmlFile in xmlFiles)
            {
                // for each xml file, create copy of blank form
                // populate data on the blank form copy
                var filedPdf = xmlFile.Replace(".xml", ".pdf");
                File.Copy(blankForm, filedPdf);
                using (var fs = new FileStream(filedPdf, FileMode.Open, FileAccess.ReadWrite))
                {
                    var pdfForm = new Aspose.Pdf.Facades.Form(fs);
                    using (var xmlInput = new FileStream(xmlFile, FileMode.Open))
                    {
                        pdfForm.ImportXml(xmlInput);
                    }
                    pdfForm.Save(fs);
                }
            }

            ZipFile.CreateFromDirectory(filesFolder, resultingFileName);
            Directory.Delete(filesFolder, true);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(new FileStream(resultingFileName, FileMode.Open, FileAccess.Read));
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileName
            };
            return response;
        }

        private void Render(string firmName, string clientNumber, string matterNumber, string filingIds, string skipArt)
        {
            var client = new RestClient("http://syncids.com/RenderIDS4.asp");
            var request = new RestRequest(Method.POST);
            request.AddParameter("FilingIDS", filingIds);
            request.AddParameter("SkipArt", skipArt);
            request.AddParameter("FirmName", firmName);
            request.AddParameter("ClientNumber", clientNumber);
            request.AddParameter("MatterNumber", matterNumber);
            client.Execute(request);
        }
    }
}