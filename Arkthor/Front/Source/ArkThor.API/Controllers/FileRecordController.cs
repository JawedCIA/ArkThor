namespace ArkThor.API.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Extensions.Configuration;
using ArkThor.API.Services;
using ArkThor.API.Helpers;
using Microsoft.EntityFrameworkCore;
using ArkThor.API.Models.Records;
using ArkThor.API.Entities;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class FileRecordController : ControllerBase
{
    private IFileRecordService _fileService;
    private IMapper _mapper;
    private readonly ILogger<FileRecordController> _logger;
    //_logger.LogInformation("File Submitted: {0} : {DT}", trustedFileNameForDisplay, DateTime.UtcNow.ToLongTimeString());
      //  _logger.LogError("File Upload Failed: {0} {DT}", trustedFileNameForDisplay, DateTime.UtcNow.ToLongTimeString());
    public FileRecordController(ILogger<FileRecordController> logger,
    IFileRecordService fileRecordService,
    IMapper mapper)
    {
        _fileService = fileRecordService;
        _mapper = mapper;
        _logger = logger;
    }


    [HttpPut]
    [Route("UpdateThreatType")]
    public IActionResult UpdateThreatType(string hash256, string threatType)
    {

        var files = _fileService.UpdateThreatType(hash256.ToUpper(),threatType);
        return Ok(files);
    }


    [HttpPut]
    [Route("UpdateCurrentStage")]
    public IActionResult UpdateCurrentStage(string hash256, string stage)
    {
        var files = _fileService.UpdateCurrentStage(hash256.ToUpper(), stage);
        return Ok(files);
    }

    [HttpPut]
    [Route("UpdateStatus")]
    public IActionResult UpdateStatus(string hash256, string status)
    {
        _logger.LogInformation("Status Update {0} Request for hash: {1}", status, hash256);
        var files = _fileService.UpdateStatus(hash256.ToUpper(), status);
        return Ok(files);
    }

   
    [HttpPut]
    [Route("UpdateAnalyzedDate")]
    public IActionResult UpdateAnalyzedDate(string hash256, DateTime AnalyzedDate)
    {
        var files = _fileService.UpdateAnalyzedDate(hash256.ToUpper(), AnalyzedDate);
        return Ok(files);
    }

    [HttpPut]
    [Route("UpdateSeverity")]
    public IActionResult UpdateSeverity(string hash256, string severity)
    {
        var files = _fileService.UpdateSeverity(hash256.ToUpper(), severity);
        return Ok(files);
    }

    [HttpGet]
    [Route("GetAllFileRecord")]
    public IActionResult GetAllFileRecord()
    {
        var files = _fileService.GetAll();
        return Ok(files);
    }
    //Get Count For Dashboard
    [HttpGet]
    [Route("GetDashboardCount")]
    public IActionResult GetDashboardCount()
    {
        var files = _fileService.GetVariousCount();
        return Ok(files);
    }

  
  
    [HttpGet]
    [Route("GetFileRecordByUploadedDate")]
    public IActionResult GetFileRecordByUploadedDate(DateTime FromUploadedDate, DateTime ToUploadedDate)
    {
        var files = _fileService.GetFileRecordByUploadedDate(FromUploadedDate, ToUploadedDate);
        return Ok(files);
    }

    // Task<IEnumerable<DistributionCount>> GetFilesDistributionBasedOnAnalyzedDate(DateTime FromDate, DateTime ToDate);

    [HttpGet]
    [Route("GetFilesDistributionBasedOnAnalyzedDate")]
    public IActionResult GetFilesDistributionBasedOnAnalyzedDate(DateTime FromUploadedDate, DateTime ToUploadedDate)
    {
        var files = _fileService.GetFilesDistributionBasedOnAnalyzedDate(FromUploadedDate, ToUploadedDate);
        return Ok(files);
    }

    [HttpGet]
    [Route("GetC2CountriesDistributionBasedOnUploadedDate")]
    public IActionResult GetC2CountriesDistributionBasedOnUploadedDate(DateTime FromUploadedDate, DateTime ToUploadedDate)
    {
        var files = _fileService.GetC2CountriesDistributionBasedOnUploadedDate(FromUploadedDate, ToUploadedDate);
        return Ok(files);
    }

    [HttpGet]
    [Route("GetC2InfectedCountriesDistributionBasedOnUploadedDate")]
    public IActionResult GetC2InfectedCountriesDistributionBasedOnUploadedDate(DateTime FromUploadedDate, DateTime ToUploadedDate)
    {
        var files = _fileService.GetC2InfectedCountriesDistributionBasedOnUploadedDate(FromUploadedDate, ToUploadedDate);
        return Ok(files);
    }
    
    [HttpGet]
    [Route("GetFilesDistributionBasedOnUploadedDate")]
    public IActionResult GetFilesDistributionBasedOnUploadedDate(DateTime FromUploadedDate, DateTime ToUploadedDate)
    {
        var files = _fileService.GetFilesDistributionBasedOnUploadedDate(FromUploadedDate, ToUploadedDate);
        return Ok(files);
    }

    [HttpGet]
    [Route("GetThreatDistributionBasedOnUploadedDate")]
    public IActionResult GetThreatDistributionBasedOnUploadedDate(DateTime FromUploadedDate, DateTime ToUploadedDate)
    {
        var files = _fileService.GetThreatDistributionBasedOnUploadedDate(FromUploadedDate, ToUploadedDate);
        return Ok(files);
    }

    [HttpGet]
    [Route("GetStatusDistributionBasedOnUploadedDate")]
    public IActionResult GetStatusDistributionBasedOnUploadedDate(DateTime FromUploadedDate, DateTime ToUploadedDate)
    {
        var files = _fileService.GetStatusDistributionBasedOnUploadedDate(FromUploadedDate, ToUploadedDate);
        return Ok(files);
    }
    //GetSimilarThreatRecords
    [HttpGet]
    [Route("GetSimilarThreatRecords")]
    public IActionResult GetSimilarThreatRecords(string threatType)
    {
        var files = _fileService.GetSimilarThreatRecords(threatType);
        return Ok(files);
    }

    [HttpGet]
    [Route("GetTOPFileRecord")]
    public IActionResult GetTOPFileRecord(int numberOfRecordsToFetch)
    {
        var files = _fileService.GetTOPFileRecord(numberOfRecordsToFetch);
        return Ok(files);
    }

    [HttpGet]
    [Route("GetByHash")]
    public IActionResult GetByHash(string hash256)
    {
        var files = _fileService.GetByHash(hash256);
        return Ok(files);
    }


    [HttpPost]
    [Route("CreateFileRecord")]
    public IActionResult CreateFileRecord(CreateFileRecord model)
    {
        _logger.LogInformation("File Record Creation for file Name with hash  {0} ", model.FileName);
        //Save File on disk
        _fileService.SaveFileOnDisk(model);
        //Save Record in Database
        _fileService.Create(model);

        return Ok(new { message = "File created" });
    }
   

    //[HttpPut("{id}")]
    //public IActionResult Update(int id, UpdateRequest model)
    //{
    //    _fileService.Update(id, model);
    //    return Ok(new { message = "User updated" });
    //}

    //[HttpDelete("{id}")]
    //public IActionResult Delete(int id)
    //{
    //    _userService.Delete(id);
    //    return Ok(new { message = "User deleted" });
    //}
}