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

namespace ArkThor.Dashboard.Controllers
{
    public class HomeController : Controller
    {
       private readonly ILogger<HomeController> _logger;    
      
      
        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions;
        private readonly string _webAPIBaseAddress;
        private readonly string _checkUploadFileSignatur;
        private readonly string _WebAPIBaseAddressForJS;
        public HomeController(IConfiguration config, ILogger<HomeController> logger)
        {
            //this.context = context;
            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");           
            _permittedExtensions = config.GetValue<string>("PermittedExtensions").Split(';');          
            _webAPIBaseAddress = config.GetValue<string>("WebAPIBaseAddress");
            _checkUploadFileSignatur = config.GetValue<string>("checkUploadFileSignatur");
            _WebAPIBaseAddressForJS = config.GetValue<string>("WebAPIBaseAddressForJS");
            _logger = logger;
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

        //Get the Base API URL
        [HttpGet]
        [Route("GetBaseAPIUrl")]
        public string GetBaseAPIUrl()
        {
            return _WebAPIBaseAddressForJS;
        }

        [HttpPost]
        [Route("UploadFiles")]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files)
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
                                                var uploaderRandomName = RandomUploaderName.GetRandomName();
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
                                                using (var dataStream = new MemoryStream())
                                                {
                                                    await formFile.CopyToAsync(dataStream);
                                                    fileModel.Data = dataStream.ToArray();
                                                }

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

                                                   return StatusCode(422, new { message = "File Upload Failed: " + responseTaskUploadReleaseFile.Result.ReasonPhrase + " : " + responseTaskUploadReleaseFile.Result.StatusCode });

                                                }
                                            }
                                            else
                                            {
                                                return StatusCode(422, new { message = "File Upload Failed: Unable to get Hash-256 of file, Kinldy try again with different file" });
                                                //return BadRequest("File Upload Failed: Unable to get Hash-256 of file, Kinldy try again with different file");
                                            }


                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        // return Ok(new { count = files.Count, size, ex.Message });                    
                                       
                                        _logger.LogInformation("Error: {0} : {1}: {DT}", trustedFileNameForDisplay, ex.Message, DateTime.UtcNow.ToLongTimeString());
                                       // return BadRequest("File Upload Failed EX: " + ex.Message);
                                        return StatusCode(422, new { message = "File Upload Failed: " + ex.Message });
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
                        return BadRequest("Chosen file size :" + fileSizeInMB + " MB is above permitted size :" + megabyteSizeLimit + " MB, Kindly choose file size small than permitted size!");
                        //string.Format("Chosen file size : {size} is above permitted size : {permittedSize} MB, Kindly choose file size small than permitted size!", size, megabyteSizeLimit);
                    }                  

                }
                catch (System.Exception ex)
                {
                    // Handle the upload error here
                   // return BadRequest("An error occurred while uploading the file: " + ex.Message);
                    return StatusCode(422, new { message = "An error occurred while uploading the file: " + ex.Message });
                }
            }

            // Return a success response
            return Ok("Files uploaded successfully.");
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
    }
}
