﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ArkThor.Dashboard.Models;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ArkThor.Dashboard.Utilities;
using System.Net.Mime;
using System.Net;
using System.Net.Http.Headers;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json;
using System.Net.Http;
using ArkThor.Models;
using ArkThor.UI.Utilities;
using System.Net.Http.Json;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ArkThor.Dashboard.Controllers
{

    public class HomeController : Controller
    {
       private readonly ILogger<HomeController> _logger;    
      
      
        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions;
        private readonly string _webAPIBaseAddress;
        private readonly string _checkUploadFileSignatur;
        private readonly string _ReadOnly;
        private readonly UploaderServiceName _uploaderServiceName;
        // private readonly string _WebAPIBaseAddressForJS;
        public HomeController(IConfiguration config, ILogger<HomeController> logger, UploaderServiceName uploaderServiceName)
        {
            //this.context = context;
            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");           
            _permittedExtensions = config.GetValue<string>("PermittedExtensions").Split(';');          
            _webAPIBaseAddress = config.GetValue<string>("WebAPIBaseAddress");
            _checkUploadFileSignatur = config.GetValue<string>("checkUploadFileSignatur");
            _ReadOnly= config.GetValue<string>("ReadOnly");
            // _WebAPIBaseAddressForJS = config.GetValue<string>("WebAPIBaseAddressForJS");
            _logger = logger;
            _uploaderServiceName= uploaderServiceName;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        //public IActionResult UploadFiless()
        //{
        //    return View();
        //}

        
        private string EnsureCorrectFilename(string filename)
        {
            if (filename.Contains("\\"))
                filename = filename.Substring(filename.LastIndexOf("\\") + 1);

            return filename;
        }

       

        [HttpPost]
        [Route("UploadFiles")]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files)
        {
            string uploaderName = _uploaderServiceName.GetUploaderName();

            if (_ReadOnly.ToUpper()=="FALSE")
            {
                // Upload each file
                foreach (var file in files)
                {
                    try
                    {
                        long size = files.Sum(f => f.Length);
                        var megabyteSizeLimit = _fileSizeLimit / 1048576;
                        long fileSizeInMB = size / 1048576;
                        HttpClientHandler clientHandler = new HttpClientHandler();
                        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
                        using var client = new HttpClient(clientHandler);

                        client.BaseAddress = new Uri(_webAPIBaseAddress);
                        //Check for file size
                        if (size < _fileSizeLimit)
                        {
                            // The file is too large ... discontinue processing the file           


                            foreach (var formFile in files)
                            {

                                string fileName = ContentDispositionHeaderValue.Parse(formFile.ContentDisposition).FileName.Trim('"');

                                fileName = this.EnsureCorrectFilename(fileName);
                                var fileNameWithExtn = Path.GetFileName(formFile.FileName);
                                var extension = Path.GetExtension(formFile.FileName).ToLowerInvariant();
                                var uploadedFileFullPath = Path.GetFullPath(formFile.FileName);
                                // Don't trust the file name sent by the client. To display
                                // the file name, HTML-encode the value.
                                // Don't trust the file name sent by the client. To display
                                // the file name, HTML-encode the value.
                                var trustedFileNameForDisplay = WebUtility.HtmlEncode(formFile.FileName);
                                // var fileName = Path.GetFileNameWithoutExtension(formFile.FileName);

                                // var extension = Path.GetExtension(formFile.FileName);
                                _logger.LogInformation("File Submitted: {0} : {DT}", trustedFileNameForDisplay, DateTime.UtcNow.ToLongTimeString());
                                //check for FileExtension
                                if (string.IsNullOrEmpty(extension) || !_permittedExtensions.Contains(extension))
                                {
                                    // The extension is invalid ... discontinue processing the file
                                    return StatusCode(422, new { message = "File with Extension: " + extension + " is not permitted, Please use only Permitted extension : " + string.Join(" | ", _permittedExtensions) });
                                    // return BadRequest("File with Extension: " + extension + " is not permitted, Please use only Permitted extension : " + string.Join(" | ", _permittedExtensions));
                                    // break;
                                }
                                else
                                {
                                    //Check for File Signature 
                                    bool IsValidateSignature = false;
                                    if (_checkUploadFileSignatur.ToUpper() == "TRUE")
                                    {
                                        IsValidateSignature = Utilities.FileHelpers.IsValidFileSignature(formFile, extension);
                                    }
                                    else
                                    {
                                        IsValidateSignature = true; //ByPass valide Signature
                                    }

                                    if (IsValidateSignature)
                                    {
                                        try
                                        {

                                            _logger.LogInformation("Processing File {0} {DT}", fileName, DateTime.UtcNow.ToLongTimeString());

                                            if (formFile.Length > 0)
                                            {
                                                var filePath = Path.GetTempFileName();

                                                using (var stream = System.IO.File.Create(filePath))
                                                {
                                                    await formFile.CopyToAsync(stream);
                                                }

                                                //Get SHA256 of uploaded file
                                                var hashValue = GenereteHash256.SHA256file(filePath);
                                                //Make sure hash is not emplty or null
                                                if (!string.IsNullOrEmpty(hashValue))
                                                {
                                                    //Get the random name for Uploader
                                                    var uploaderRandomName = uploaderName;// RandomUploaderName.GetRandomName();
                                                                                          //Uploading File to API for DB Store
                                                    var fileModel = new UploadedFileModel
                                                    {
                                                        UploadedDate = DateTime.UtcNow,
                                                        UploadedBy = uploaderRandomName,//"Admin",
                                                        ContentType = formFile.ContentType,
                                                        Size = formFile.Length,
                                                        Extension = extension,
                                                        FileName = trustedFileNameForDisplay,
                                                        HashValue = hashValue.ToUpper(),
                                                        Status = "Queued"
                                                    };

                                                    //if (fileSizeInMB <= 50)
                                                    //    {
                                                    using (var dataStream = new MemoryStream())
                                                    {
                                                        await file.CopyToAsync(dataStream);
                                                        fileModel.Data = dataStream.ToArray();
                                                    }

                                                    //    }
                                                    //    else
                                                    //    {
                                                    //         fileModel.Data = null;
                                                    //   }
                                                    string fileData = JsonConvert.SerializeObject(fileModel);
                                                    HttpContent httpContent = new StringContent(fileData, Encoding.UTF8, "application/json");

                                                    //HTTP GET
                                                    var responseTaskUploadReleaseFile = client.PostAsync("FileRecord/CreateFileRecord", httpContent);

                                                    var responseTaskUploadReleaseFileResult = responseTaskUploadReleaseFile.Result;

                                                    if (responseTaskUploadReleaseFileResult.IsSuccessStatusCode)
                                                    {

                                                        //ViewBag.Message = "SUCCESS: " + trustedFileNameForDisplay + " uploaded Successfully for Threat categorization Analysis based on C2 Communication!";
                                                        _logger.LogInformation("SUCCESS: File Upload Successful {0} : {DT}", trustedFileNameForDisplay, DateTime.UtcNow.ToLongTimeString());
                                                        _logger.LogInformation(" {0} : Hash256 Value {1}: {DT}", trustedFileNameForDisplay, hashValue, DateTime.UtcNow.ToLongTimeString());
                                                    }
                                                    else //web api sent error response 
                                                    {
                                                        _logger.LogError("Error:File Upload Failed {0} : {DT}", trustedFileNameForDisplay, DateTime.UtcNow.ToLongTimeString());
                                                        return StatusCode(422, new { message = "File Upload Failed: " + responseTaskUploadReleaseFile.Result.ReasonPhrase + " : " + responseTaskUploadReleaseFile.Result.StatusCode });

                                                    }
                                                }
                                                else
                                                {
                                                    _logger.LogError("File Upload Failed: {0} {DT}", trustedFileNameForDisplay, DateTime.UtcNow.ToLongTimeString());
                                                    return StatusCode(422, new { message = "File Upload Failed: Unable to get Hash-256 of file, Kinldy try again with different file" });
                                                    //return BadRequest("File Upload Failed: Unable to get Hash-256 of file, Kinldy try again with different file");
                                                }


                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            // return Ok(new { count = files.Count, size, ex.Message });                    

                                            _logger.LogError("Error: {0} : {1}: {DT}", trustedFileNameForDisplay, ex.Message, DateTime.UtcNow.ToLongTimeString());
                                            // return BadRequest("File Upload Failed EX: " + ex.Message);
                                            return StatusCode(422, new { message = "File Upload Failed: " + trustedFileNameForDisplay + ex.Message });
                                        }
                                    }
                                    else
                                    {
                                        //return BadRequest("Chosen file signature does not matched with the extension of file: " + trustedFileNameForDisplay);
                                        return StatusCode(422, new { message = "Chosen file signature does not matched with the extension of file: " + trustedFileNameForDisplay });
                                    }
                                }
                            }
                        }
                        else
                        {
                            return StatusCode(500, new { message = "Chosen file size :" + fileSizeInMB + " MB is above permitted size :" + megabyteSizeLimit + " MB, Kindly choose file size small than permitted size, or use ArkThor API to upload File!" });
                            //string.Format("Chosen file size : {size} is above permitted size : {permittedSize} MB, Kindly choose file size small than permitted size!", size, megabyteSizeLimit);
                        }

                    }
                    catch (System.Exception ex)
                    {
                        // Handle the upload error here
                        // return BadRequest("An error occurred while uploading the file: " + ex.Message);
                        _logger.LogError("Error:File - {0} : {DT}", ex.Message, DateTime.UtcNow.ToLongTimeString());
                        return StatusCode(422, new { message = "An error occurred while uploading the file: " + ex.Message });
                    }
                }

                // Return a success response
                return Ok("Files uploaded successfully.");
            }
            else
            {
                return Ok("Sorry for the inconvenience, ARKTHOR is unable to upload and process your file as it is running in ReadOnly/View Mode!");
            }
        }
    
        public ActionResult AnalysisInformation()
        {

            return View();
        }
        

        public ActionResult AnalysisRecords()
        {

            return View();
        } //UnderDevelopment

        public ActionResult UnderDevelopment()
        {

            return View();
        } //UnderDevelopment

        public ActionResult CoreConfig()
        {

            string configFilePath = "config.json"; // Replace with the actual file path
            string jsonContents = System.IO.File.ReadAllText(configFilePath);
            dynamic configData = JsonConvert.DeserializeObject(jsonContents);
            return View(configData);
        } //CoreConfig
        public ActionResult CoreComponentRefresh()
        {

            return View();
        }
        
        public ActionResult ThreatRecords()
        {

            return View();
        }
        public IActionResult Contacts()
        {
            return View();
        }

        public IActionResult Subscribe()
        {
            return View();
        }
        public IActionResult Statistics()
        {
            return View();
        }
        public IActionResult LiveAnalysisTracking()
        {
            return View();
        }
        
        public IActionResult WebAPI()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
       //[Route("SaveConfig")]
      
        public async Task<IActionResult> PostCoreConfig()
        {
            try
            {
                // Get the content of the config file from the request
                string configContent = Request.Form["configContent"];

                // Call the external API with the configContent
                using (HttpClient httpClient = new HttpClient())
                {
                    // Specify the URL of the external API
                    string externalApiUrl = _webAPIBaseAddress+ "CoreAdmin/UpdateCoreConfig";

                    // Create a JSON object with the configContent
                    JObject requestData = new JObject();
                    requestData["configContent"] = configContent;

                    // Convert the JSON object to a string
                    string jsonRequest = requestData.ToString();

                    // Create a StringContent object with the JSON string
                    StringContent content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    // Send the POST request to the external API
                    HttpResponseMessage response = await httpClient.PostAsync(externalApiUrl, content);

                    // Check if the API call was successful
                    if (!response.IsSuccessStatusCode)
                    {
                        // API call failed, handle the error
                        string errorMessage = $"Failed to call the external API. StatusCode: {response.StatusCode}";
                        return BadRequest(errorMessage);
                    }
                }

                // Specify the path to the config.json file
                string configFilePath = "config.json";

                // Write the configContent to the file
                await System.IO.File.WriteAllTextAsync(configFilePath, configContent);

                // Optionally, you can perform any additional logic or validation here

                return Ok(); // Return a success response
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during saving
                return BadRequest(ex.Message); // Return an error response
            }
        }

       


        [HttpGet]
        public async Task<IActionResult> ProxyToExternalEndpoint_GetDashboardCount()
        {

            string externalApiUrl = _webAPIBaseAddress + "FileRecord/GetDashboardCount";
            // Forward the request to the external WebAPI
            using (HttpClient client = new HttpClient())
            {
                // Create a HttpRequestMessage to send to the external WebAPI
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, externalApiUrl);

                // Send the request to the external WebAPI
                HttpResponseMessage response = await client.SendAsync(request);

                // Retrieve the response body from the external WebAPI
                string responseBody = await response.Content.ReadAsStringAsync();

                // Return the response to the JavaScript code
                return Content(responseBody, "application/json");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProxyToExternalEndpoint_GetByHash(string hash256)
        {
            string externalApiUrl = _webAPIBaseAddress+"FileRecord/GetByHash"+ "?hash256=" + hash256;
            //string externalApiUrl = _webAPIBaseAddress + "FileRecord/GetDashboardCount";
            // Forward the request to the external WebAPI
            using (HttpClient client = new HttpClient())
            {
                // Create a HttpRequestMessage to send to the external WebAPI
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, externalApiUrl);

                // Send the request to the external WebAPI
                HttpResponseMessage response = await client.SendAsync(request);

                // Retrieve the response body from the external WebAPI
                string responseBody = await response.Content.ReadAsStringAsync();

                // Return the response to the JavaScript code
                return Content(responseBody, "application/json");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProxyToExternalEndpoint_GetFileRecordByUploadedDate(string FromUploadedDate, string ToUploadedDate)
        {
            string externalApiUrl = _webAPIBaseAddress + "FileRecord/GetFileRecordByUploadedDate?FromUploadedDate=" + FromUploadedDate + "&ToUploadedDate=" + ToUploadedDate;     
            // Forward the request to the external WebAPI
            using (HttpClient client = new HttpClient())
            {
                // Create a HttpRequestMessage to send to the external WebAPI
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, externalApiUrl);
                // Send the request to the external WebAPI
                HttpResponseMessage response = await client.SendAsync(request);
                // Retrieve the response body from the external WebAPI
                string responseBody = await response.Content.ReadAsStringAsync();
                // Return the response to the JavaScript code
                return Content(responseBody, "application/json");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProxyToExternalEndpoint_GetTOPFileRecord(string numberOfRecordsToFetch)
        {
            string externalApiUrl = _webAPIBaseAddress + "FileRecord/GetTOPFileRecord?numberOfRecordsToFetch=" + numberOfRecordsToFetch;
            // Forward the request to the external WebAPI
            using (HttpClient client = new HttpClient())
            {
                // Create a HttpRequestMessage to send to the external WebAPI
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, externalApiUrl);
                // Send the request to the external WebAPI
                HttpResponseMessage response = await client.SendAsync(request);
                // Retrieve the response body from the external WebAPI
                string responseBody = await response.Content.ReadAsStringAsync();
                // Return the response to the JavaScript code
                return Content(responseBody, "application/json");
            }
        }
        //threatType
        [HttpGet]
        public async Task<IActionResult> ProxyToExternalEndpoint_GetSimilarThreatRecords(string threatType)
        {
            string externalApiUrl = _webAPIBaseAddress + "FileRecord/GetSimilarThreatRecords?threatType=" + threatType;
            // Forward the request to the external WebAPI
            using (HttpClient client = new HttpClient())
            {
                // Create a HttpRequestMessage to send to the external WebAPI
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, externalApiUrl);
                // Send the request to the external WebAPI
                HttpResponseMessage response = await client.SendAsync(request);
                // Retrieve the response body from the external WebAPI
                string responseBody = await response.Content.ReadAsStringAsync();
                // Return the response to the JavaScript code
                return Content(responseBody, "application/json");
            }
        }
        //GetSupportFiles
        [HttpGet]
        public async Task<IActionResult> ProxyToExternalEndpoint_GetSupportFiles(string sha256)
        {
            string externalApiUrl = _webAPIBaseAddress + "FileUpload/GetSupportFiles?sha256=" + sha256;
            // Forward the request to the external WebAPI
            using (HttpClient client = new HttpClient())
            {
                // Create a HttpRequestMessage to send to the external WebAPI
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, externalApiUrl);
                // Send the request to the external WebAPI
                HttpResponseMessage response = await client.SendAsync(request);
                // Retrieve the response body from the external WebAPI
                string responseBody = await response.Content.ReadAsStringAsync();
                // Return the response to the JavaScript code
                return Content(responseBody, "application/json");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProxyToExternalEndpoint_GetThreatDistributionBasedOnUploadedDate(string FromUploadedDate, string ToUploadedDate)
        {
            string externalApiUrl = _webAPIBaseAddress + "FileRecord/GetThreatDistributionBasedOnUploadedDate?FromUploadedDate=" + FromUploadedDate + "&ToUploadedDate=" + ToUploadedDate;
            // Forward the request to the external WebAPI
            using (HttpClient client = new HttpClient())
            {
                // Create a HttpRequestMessage to send to the external WebAPI
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, externalApiUrl);
                // Send the request to the external WebAPI
                HttpResponseMessage response = await client.SendAsync(request);
                // Retrieve the response body from the external WebAPI
                string responseBody = await response.Content.ReadAsStringAsync();
                // Return the response to the JavaScript code
                return Content(responseBody, "application/json");
            }
        }
        [HttpGet]
        public async Task<IActionResult> ProxyToExternalEndpoint_GetStatusDistributionBasedOnUploadedDate(string FromUploadedDate, string ToUploadedDate)
        {
            string externalApiUrl = _webAPIBaseAddress + "FileRecord/GetStatusDistributionBasedOnUploadedDate?FromUploadedDate=" + FromUploadedDate + "&ToUploadedDate=" + ToUploadedDate;
            // Forward the request to the external WebAPI
            using (HttpClient client = new HttpClient())
            {
                // Create a HttpRequestMessage to send to the external WebAPI
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, externalApiUrl);
                // Send the request to the external WebAPI
                HttpResponseMessage response = await client.SendAsync(request);
                // Retrieve the response body from the external WebAPI
                string responseBody = await response.Content.ReadAsStringAsync();
                // Return the response to the JavaScript code
                return Content(responseBody, "application/json");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProxyToExternalEndpoint_GetFilesDistributionBasedOnUploadedDate(string FromUploadedDate, string ToUploadedDate)
        {
            string externalApiUrl = _webAPIBaseAddress + "FileRecord/GetFilesDistributionBasedOnUploadedDate?FromUploadedDate=" + FromUploadedDate + "&ToUploadedDate=" + ToUploadedDate;
            // Forward the request to the external WebAPI
            using (HttpClient client = new HttpClient())
            {
                // Create a HttpRequestMessage to send to the external WebAPI
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, externalApiUrl);
                // Send the request to the external WebAPI
                HttpResponseMessage response = await client.SendAsync(request);
                // Retrieve the response body from the external WebAPI
                string responseBody = await response.Content.ReadAsStringAsync();
                // Return the response to the JavaScript code
                return Content(responseBody, "application/json");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProxyToExternalEndpoint_GetFilesDistributionBasedOnAnalyzedDate(string FromUploadedDate, string ToUploadedDate)
        {
            string externalApiUrl = _webAPIBaseAddress + "FileRecord/GetFilesDistributionBasedOnAnalyzedDate?FromUploadedDate=" + FromUploadedDate + "&ToUploadedDate=" + ToUploadedDate;
            // Forward the request to the external WebAPI
            using (HttpClient client = new HttpClient())
            {
                // Create a HttpRequestMessage to send to the external WebAPI
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, externalApiUrl);
                // Send the request to the external WebAPI
                HttpResponseMessage response = await client.SendAsync(request);
                // Retrieve the response body from the external WebAPI
                string responseBody = await response.Content.ReadAsStringAsync();
                // Return the response to the JavaScript code
                return Content(responseBody, "application/json");
            }
        }
        //GetC2CountriesDistributionBasedOnUploadedDate
        [HttpGet]
        public async Task<IActionResult> ProxyToExternalEndpoint_GetC2CountriesDistributionBasedOnUploadedDate(string FromUploadedDate, string ToUploadedDate)
        {
            string externalApiUrl = _webAPIBaseAddress + "FileRecord/GetC2CountriesDistributionBasedOnUploadedDate?FromUploadedDate=" + FromUploadedDate + "&ToUploadedDate=" + ToUploadedDate;
            // Forward the request to the external WebAPI
            using (HttpClient client = new HttpClient())
            {
                // Create a HttpRequestMessage to send to the external WebAPI
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, externalApiUrl);
                // Send the request to the external WebAPI
                HttpResponseMessage response = await client.SendAsync(request);
                // Retrieve the response body from the external WebAPI
                string responseBody = await response.Content.ReadAsStringAsync();
                // Return the response to the JavaScript code
                return Content(responseBody, "application/json");
            }
        }

        //GetC2InfectedCountriesDistributionBasedOnUploadedDate

        [HttpGet]
        public async Task<IActionResult> ProxyToExternalEndpoint_GetC2InfectedCountriesDistributionBasedOnUploadedDate(string FromUploadedDate, string ToUploadedDate)
        {
            string externalApiUrl = _webAPIBaseAddress + "FileRecord/GetC2InfectedCountriesDistributionBasedOnUploadedDate?FromUploadedDate=" + FromUploadedDate + "&ToUploadedDate=" + ToUploadedDate;
            // Forward the request to the external WebAPI
            using (HttpClient client = new HttpClient())
            {
                // Create a HttpRequestMessage to send to the external WebAPI
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, externalApiUrl);
                // Send the request to the external WebAPI
                HttpResponseMessage response = await client.SendAsync(request);
                // Retrieve the response body from the external WebAPI
                string responseBody = await response.Content.ReadAsStringAsync();
                // Return the response to the JavaScript code
                return Content(responseBody, "application/json");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProxyToExternalEndpoint_Updateip2asn()
        {
            string externalApiUrl = _webAPIBaseAddress + "CoreAdmin/Updateip2asn";
            // Forward the request to the external WebAPI
            using (HttpClient client = new HttpClient())
            {
                // Create a HttpRequestMessage to send to the external WebAPI
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, externalApiUrl);
                // Send the request to the external WebAPI
                HttpResponseMessage response = await client.SendAsync(request);
                // Retrieve the response body from the external WebAPI
                string responseBody = await response.Content.ReadAsStringAsync();
                // Return the response to the JavaScript code
                return Content(responseBody, "application/json");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProxyToExternalEndpoint_refreshThreatFoxRule()
        {
            string externalApiUrl = _webAPIBaseAddress + "CoreAdmin/refreshThreatFoxRule";
            // Forward the request to the external WebAPI
            using (HttpClient client = new HttpClient())
            {
                // Create a HttpRequestMessage to send to the external WebAPI
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, externalApiUrl);
                // Send the request to the external WebAPI
                HttpResponseMessage response = await client.SendAsync(request);
                // Retrieve the response body from the external WebAPI
                string responseBody = await response.Content.ReadAsStringAsync();
                // Return the response to the JavaScript code
                return Content(responseBody, "application/json");
            }
        }


    }
}
