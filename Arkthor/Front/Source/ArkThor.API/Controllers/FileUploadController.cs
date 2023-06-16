using ArkThor.API.Entities;
using ArkThor.API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ArkThor.API.Utilities;
using System.Security.Cryptography;
using ArkThor.API.Models.Records;
using System.Diagnostics.Eventing.Reader;

namespace ArkThor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly ILogger<FileUploadController> _logger;
        private IMapper _mapper;
        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions;
        private readonly string _checkUploadFileSignatur;
        private IFileRecordService _fileService;
        private ISupportFileService _supportFileService;
        private IRabbitMQService _rabbitMQService;
        public FileUploadController(ILogger<FileUploadController> logger, IFileRecordService fileRecordService, IMapper mapper, IConfiguration config, ISupportFileService supportFileService,IRabbitMQService rabbitMQService)
        {
            _logger = logger;
            _fileService = fileRecordService;
            _mapper = mapper;
            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");
            _permittedExtensions = config.GetValue<string>("PermittedExtensions").Split(';');
            _checkUploadFileSignatur = config.GetValue<string>("checkUploadFileSignatur");
            _supportFileService = supportFileService;
            _rabbitMQService = rabbitMQService;
        }

        [HttpPost]
        [Route("UploadFileOutPutJson")]
        public async Task<IActionResult> UploadFileOutPutJson(IFormFile file)
        {
            try
            {
                var trustedFileNameForDisplay = WebUtility.HtmlEncode(file.FileName);
                string? fileContent = null;
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    fileContent = reader.ReadToEnd();
                }
               // var result = JsonConvert.DeserializeObject<MyObject>(fileContent);

                var rules = JsonConvert.DeserializeObject<List<OutPutJsonFile>>(fileContent);
               
                if (rules != null)
                {
                    DateTime AnalyzedDateTime= DateTime.Now;
                    if (rules[0].analyzed_time != null)
                    {
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)rules[0].analyzed_time);
                        AnalyzedDateTime = dateTimeOffset.LocalDateTime;
                    }
                  //  string C2Communication = JsonSerializer.Serialize(rules[0].c2_countries);
                    //Uploading File to API for DB Store
                    var filerecord = new FileRecord
                    {
                        HashValue = rules[0].SHA256.ToUpper(),
                        ThreatType = rules[0].rule_name,
                        Severity = rules[0].severity,
                        Status = rules[0].Status,
                        AnalyzedDate= AnalyzedDateTime,
                        C2Countries = JsonConvert.SerializeObject(rules[0].c2_countries),
                        infected_countries = JsonConvert.SerializeObject(rules[0].infected_countries),
                        MITRE = JsonConvert.SerializeObject(rules[0].MITRE),
                        C2Communication = JsonConvert.SerializeObject(rules[0].C2Communication),
                        UploadedDate=  DateTime.Now

                };
                    using (var dataStream = new MemoryStream())
                    {
                        await file.CopyToAsync(dataStream);
                        filerecord.JsonData = dataStream.ToArray();
                    }
                    _fileService.UpdateRecordWithOutPutJsonValue(filerecord);
                    
                    return Ok("File Saved!");
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,"Muliple Rules Provided in Json file");
                }

               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 136314880)]
        [RequestSizeLimit(136314880)]
        [Route("UploadFileForAnalysis")]
        public async Task<IActionResult> UploadFileForAnalysis(IFormFile file)
        {

            long size = file.Length;
            var megabyteSizeLimit = _fileSizeLimit / 1048576;
            long fileSizeInMB = size / 1048576;
            // HttpClientHandler clientHandler = new HttpClientHandler();
            // clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            // using var client = new HttpClient(clientHandler);

            // client.BaseAddress = new Uri("");
            //Check for file size
            if (size < _fileSizeLimit)
            {
                // The file is too large ... discontinue processing the file

                var fileNameWithExtn = Path.GetFileName(file.FileName);
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var uploadedFileFullPath = Path.GetFullPath(file.FileName);
                // Don't trust the file name sent by the client. To display
                // the file name, HTML-encode the value.
                // Don't trust the file name sent by the client. To display
                // the file name, HTML-encode the value.
                var trustedFileNameForDisplay = WebUtility.HtmlEncode(file.FileName);
                // var fileName = Path.GetFileNameWithoutExtension(formFile.FileName);

                // var extension = Path.GetExtension(formFile.FileName);

                //check for FileExtension
                if (string.IsNullOrEmpty(extension) || !_permittedExtensions.Contains(extension))
                {
                    // The extension is invalid ... discontinue processing the file
                    return StatusCode(StatusCodes.Status500InternalServerError, "File with Extension: " + extension + " is not permitted, Please use only Permitted extension : " + string.Join(" | ", _permittedExtensions));

                }
                else
                {
                    //Check for File Signature 
                    bool IsValidateSignature = false;
                    if (_checkUploadFileSignatur.ToUpper() == "TRUE")
                    {
                        IsValidateSignature = Utilities.FileHelpers.IsValidFileSignature(file, extension);
                    }
                    else
                    {
                        IsValidateSignature = true; //ByPass valide Signature
                    }

                    if (IsValidateSignature)
                    {
                        try
                        {



                            if (file.Length > 0)
                            {

                                var filePath = Path.GetTempFileName();

                                using (var stream = System.IO.File.Create(filePath))
                                {
                                    await file.CopyToAsync(stream);
                                }

                                //Get SHA256 of uploaded file
                                var hashValue = GenereteHash256.SHA256file(filePath);

                                if (hashValue != null)
                                {
                                    var fileToBeUpload = new CreateFileRecord
                                    {


                                        UploadedDate = DateTime.UtcNow,
                                        UploadedBy = "Admin",
                                        ContentType = file.ContentType,
                                        Size = file.Length,
                                        Extension = extension,
                                        FileName = trustedFileNameForDisplay,
                                        HashValue = hashValue.ToUpper(),
                                        Status = "Queued"

                                    };
                                 //   if(fileSizeInMB <=50)
                                //    {
                                        using (var dataStream = new MemoryStream())
                                        {
                                            await file.CopyToAsync(dataStream);
                                            fileToBeUpload.Data = dataStream.ToArray();
                                        }

                                   // }
                                 //   else
                                 //   {
                                       // fileToBeUpload.Data = null;
                                //    }
                                   

                                   
                                    //Save File on disk
                                    _fileService.SaveFileOnDisk(fileToBeUpload);
                                    //Save Record in Database
                                    _fileService.Create(fileToBeUpload);

                                    return Ok(new { message = "File created" });

                                }
                                else
                                {
                                    return StatusCode(StatusCodes.Status500InternalServerError, "Unable to get hash of file");
                                }
                                //string fileData = JsonConvert.SerializeObject(fileToBeUpload);
                            }
                            else
                            {
                                return StatusCode(StatusCodes.Status500InternalServerError, "file length is 0");
                            }

                        }
                        catch (Exception ex)
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                        }


                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, "Chosen file signature does not matched with the extension of file: " + trustedFileNameForDisplay);
                    }
                }

            }
            else
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Chosen file size :" + fileSizeInMB + " MB is above permitted size :" + megabyteSizeLimit + " MB, Kindly choose file size small than permitted size!");
            }


        }


        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 136314880)]
        [RequestSizeLimit(136314880)]
        [Route("UploadSupportingFile")]
        public async Task<IActionResult> UploadSupportingFile(string sha256, IFormFile file)
        {

            long size = file.Length;
            var megabyteSizeLimit = _fileSizeLimit / 1048576;
            long fileSizeInMB = size / 1048576;
            //Check for file size
            if (size < _fileSizeLimit)
            {
                // The file is too large ... discontinue processing the file

                var fileNameWithExtn = Path.GetFileName(file.FileName);
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var uploadedFileFullPath = Path.GetFullPath(file.FileName);
                // Don't trust the file name sent by the client. To display
                // the file name, HTML-encode the value.
                // Don't trust the file name sent by the client. To display
                // the file name, HTML-encode the value.
                var trustedFileNameForDisplay = WebUtility.HtmlEncode(file.FileName);
                // var fileName = Path.GetFileNameWithoutExtension(formFile.FileName);

                // var extension = Path.GetExtension(formFile.FileName);

                //check for FileExtension
               // if (string.IsNullOrEmpty(extension) || !_permittedExtensions.Contains(extension))
                //{
                    // The extension is invalid ... discontinue processing the file
                  //  return StatusCode(StatusCodes.Status500InternalServerError, "File with Extension: " + extension + " is not permitted, Please use only Permitted extension : " + string.Join(" | ", _permittedExtensions));

                //}
                //else
                //{
                   
                        try
                        {
                            if (file.Length > 0)
                            {                             

                                var supportFile = new UploadSupportFile
                                {

                                    UploadedDate = DateTime.UtcNow,
                                    UploadedBy = "Admin",
                                    ContentType = file.ContentType,
                                    Size = file.Length,
                                    Extension = extension,
                                    FileName = trustedFileNameForDisplay,
                                    HashValue = sha256.ToUpper()

                                };
                                using (var dataStream = new MemoryStream())
                                {
                                    await file.CopyToAsync(dataStream);
                                supportFile.Data = dataStream.ToArray();
                                }

                                _supportFileService.Create(supportFile);
                                

                                return Ok(new { message = "File created" });

                                //string fileData = JsonConvert.SerializeObject(fileToBeUpload);
                            }
                            else
                            {
                                return StatusCode(StatusCodes.Status500InternalServerError, "file length is 0");
                            }

                        }
                        catch (Exception ex)
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                        }


              //  }

            }
            else
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "Chosen file size :" + fileSizeInMB + " MB is above permitted size :" + megabyteSizeLimit + " MB, Kindly choose file size small than permitted size!");
            }


        }


        [HttpGet]     
        [Route("GetSupportFiles")]
        public async Task<IActionResult> GetSupportFiles(string sha256)
        {
            var files = await _supportFileService.GetSupportFiles(sha256);
            return Ok(files);

        }
    }
}
