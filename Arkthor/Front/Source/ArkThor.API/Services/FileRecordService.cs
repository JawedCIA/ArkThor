namespace ArkThor.API.Services;

using AutoMapper;
using BCrypt.Net;
using ArkThor.API.Entities;
using ArkThor.API.Helpers;
using Dapper;
using ArkThor.API.Models.Records;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ArkThor.API.Models.Users;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

public interface IFileRecordService
{
    IEnumerable<FileRecord> GetAll();
    Task<IEnumerable<FileRecord>> GetFileRecordByUploadedDate(DateTime FromUploadedDate, DateTime ToUploadedDate);
    Task<IEnumerable<FileRecord>> GetTOPFileRecord(int numberOfRecordsToFetch);
    Task<IEnumerable<FileRecord>> GetByHash(string hash256);
    Task<IEnumerable<FileRecord>> GetSimilarThreatRecords(string threatType);
    Task<IEnumerable<DashboardCount>> GetVariousCount(); 
     Task<IEnumerable<DistributionCount>> GetFilesDistributionBasedOnUploadedDate(DateTime FromDate, DateTime ToDate);
    Task<IEnumerable<DistributionCount>> GetFilesDistributionBasedOnAnalyzedDate(DateTime FromDate, DateTime ToDate);
    Task<IEnumerable<DistributionCount>> GetThreatDistributionBasedOnUploadedDate(DateTime FromDate, DateTime ToDate);
    Task<IEnumerable<DistributionCount>> GetStatusDistributionBasedOnUploadedDate(DateTime FromDate, DateTime ToDate);
    Task<IEnumerable<DistributionCount>> GetC2CountriesDistributionBasedOnUploadedDate(DateTime FromDate, DateTime ToDate);
    Task<IEnumerable<DistributionCount>> GetC2InfectedCountriesDistributionBasedOnUploadedDate(DateTime FromDate, DateTime ToDate);
    Task<IEnumerable<FileRecord>> UpdateThreatType(string hash256, string threatType);
    Task<IEnumerable<FileRecord>> UpdateStatus(string hash256, string status);
    Task<IEnumerable<FileRecord>> UpdateSeverity(string hash256, string severity);
    Task<IEnumerable<FileRecord>> UpdateAnalyzedDate(string hash256, DateTime AnalyzedDate);
  
    void Create(CreateFileRecord model);
    void UpdateRecordWithOutPutJsonValue(FileRecord record);

    void SaveFileOnDisk(CreateFileRecord model);
    // void Update(int id, UpdateFileRecord model);
    //  void Delete(int id);
}

public class FileRecordService : IFileRecordService
{
    private DataContext _context;
    private readonly IMapper _mapper;
    private readonly string _localfilePathToStore;
    private readonly string _useCurrentPath;
    private readonly string _targetfolderNameToUploadFile;
    private readonly string _addMultipleFileWithSameHashValue;
    public FileRecordService(
        DataContext context,
        IMapper mapper, IConfiguration config)
    {
        _context = context;
        _mapper = mapper;
        _useCurrentPath = config.GetValue<string>("useCurrentPath");
        _targetfolderNameToUploadFile = config.GetValue<string>("targetfolderNameToUploadFile");
        _localfilePathToStore = config.GetValue<string>("localfilePathToStore");
        _addMultipleFileWithSameHashValue = config.GetValue<string>("addMultipleFileWithSameHashValue");
    }

    public IEnumerable<FileRecord> GetAll()
    {
        return _context.FilesRecord;
    }


    public async Task<IEnumerable<FileRecord>> GetFileRecordByUploadedDate(DateTime FromUploadedDate, DateTime ToUploadedDate)
    {
        using var connection = _context.CreateConnection();
        var sql = """
            SELECT rowid,
             FileName,
             HashValue,
             UploadedBy,
             UploadedDate,
             ThreatType,
             Status,
             Isold,
             Extension,
             ContentType,
             Size,
             AnalyzedDate,
             Severity,
             C2Countries,
             C2Communication,
             CurrentStage,
             Extractor,
             Validator,
             Parser,
             MITRE,
        infected_countries
           FROM FilesRecord 
        WHERE  DATE(UploadedDate) between DATE(@FromUploadedDate) AND DATE(@ToUploadedDate)
        order by UploadedDate desc
        """;
        return await connection.QueryAsync<FileRecord>(sql, new { FromUploadedDate, ToUploadedDate });
    }

    //Get Files Count By Uploaded Date
    public async Task<IEnumerable<DistributionCount>> GetFilesDistributionBasedOnUploadedDate(DateTime FromDate, DateTime ToDate)
    {
        using var connection = _context.CreateConnection();
        var sql = """
            select * from (SELECT DATE(a.UploadedDate) as Date, COUNT(DISTINCT a.rowid) as TotalCount
        FROM FilesRecord a
        LEFT JOIN FilesRecord b ON DATE(a.UploadedDate) = DATE(b.UploadedDate)
        GROUP BY DATE(a.UploadedDate))
        WHERE  DATE(Date) between DATE(@FromDate) AND DATE(@ToDate);      
        """;
        return await connection.QueryAsync<DistributionCount>(sql, new { FromDate, ToDate });
    }

    //Get Distribution Record for C2 Countries
    public async Task<IEnumerable<DistributionCount>> GetC2CountriesDistributionBasedOnUploadedDate(DateTime FromDate, DateTime ToDate)
    {
        using var connection = _context.CreateConnection();
        var sql = """
            select C2Countries as Countries from FilesRecord
        WHERE  DATE(UploadedDate) between DATE(@FromDate) AND DATE(@ToDate);      
        """;
        return await connection.QueryAsync<DistributionCount>(sql, new { FromDate, ToDate });
    }
    //
    //Get Distribution Record for C2 Infected Countries
    public async Task<IEnumerable<DistributionCount>> GetC2InfectedCountriesDistributionBasedOnUploadedDate(DateTime FromDate, DateTime ToDate)
    {
        using var connection = _context.CreateConnection();
        var sql = """
           select infected_countries as Countries from FilesRecord
        WHERE  DATE(UploadedDate) between DATE(@FromDate) AND DATE(@ToDate);       
        """;
        return await connection.QueryAsync<DistributionCount>(sql, new { FromDate, ToDate });
    }
    //Get Files Count By Analyzed Date
    public async Task<IEnumerable<DistributionCount>> GetFilesDistributionBasedOnAnalyzedDate(DateTime FromDate, DateTime ToDate)
    {
        using var connection = _context.CreateConnection();
        var sql = """
           select * from (SELECT DATE(a.AnalyzedDate) as Date, COUNT(DISTINCT a.rowid) as TotalCount
        FROM FilesRecord a
        LEFT JOIN FilesRecord b ON DATE(a.AnalyzedDate) = DATE(b.AnalyzedDate)
        GROUP BY DATE(a.AnalyzedDate))
        WHERE  DATE(Date) between DATE(@FromDate) AND DATE(@ToDate);      
        """;
        return await connection.QueryAsync<DistributionCount>(sql, new { FromDate, ToDate });
    }

    //Get Threat Type Distribution
    public async Task<IEnumerable<DistributionCount>> GetThreatDistributionBasedOnUploadedDate(DateTime FromDate, DateTime ToDate)
    {
        using var connection = _context.CreateConnection();
        var sql = """
           SELECT a.ThreatType as Type, COUNT(DISTINCT a.rowid) as TotalCount
        FROM FilesRecord a
        LEFT JOIN FilesRecord b ON a.ThreatType = b.ThreatType
        WHERE a.ThreatType IS NOT NULL AND a.ThreatType <> ''
        AND
        DATE(a.UploadedDate) between DATE(@FromDate) AND DATE(@ToDate)
        GROUP BY a.ThreatType;      
        """;
        return await connection.QueryAsync<DistributionCount>(sql, new { FromDate, ToDate });
    }

    //Get File Status  Distribution
    public async Task<IEnumerable<DistributionCount>> GetStatusDistributionBasedOnUploadedDate(DateTime FromDate, DateTime ToDate)
    {
        using var connection = _context.CreateConnection();
        var sql = """
           SELECT a.Status as Status, COUNT(DISTINCT a.rowid) as TotalCount
        FROM FilesRecord a
        LEFT JOIN FilesRecord b ON a.Status = b.Status
        WHERE a.Status IS NOT NULL AND a.Status <> ''
        AND
        DATE(a.UploadedDate) between DATE(@FromDate) AND DATE(@ToDate)
        GROUP BY a.Status;      
        """;
        return await connection.QueryAsync<DistributionCount>(sql, new { FromDate, ToDate });
    }

    //Get Records based on Threat Type
    public async Task<IEnumerable<FileRecord>> GetSimilarThreatRecords(string threatType)
    {
        using var connection = _context.CreateConnection();
        var sql = """
           SELECT rowid,
             FileName,
             HashValue,
             UploadedBy,
             UploadedDate,
             ThreatType,
             Status,
             Isold,
             Extension,
             ContentType,
             Size,
             AnalyzedDate,
             Severity,
             C2Countries,
             C2Communication,
             CurrentStage,
             Extractor,
             Validator,
             Parser,
             MITRE,
        infected_countries
        FROM FilesRecord
        WHERE  ThreatType like @threatType
        order by UploadedDate desc
        """;
        return await connection.QueryAsync<FileRecord>(sql, new { threatType });
    }

    public async Task<IEnumerable<FileRecord>> GetTOPFileRecord(int numberOfRecordsToFetch)
    {
        using var connection = _context.CreateConnection();
        var sql = """
           SELECT rowid,
             FileName,
             HashValue,
             UploadedBy,
             UploadedDate,
             ThreatType,
             Status,
             Isold,
             Extension,
             ContentType,
             Size,
             AnalyzedDate,
             Severity,
             C2Countries,
             C2Communication,
             CurrentStage,
             Extractor,
             Validator,
             Parser,
             MITRE,
        infected_countries
        FROM FilesRecord
        order by UploadedDate desc
        LIMIT @numberOfRecordsToFetch
        """;
        return await connection.QueryAsync<FileRecord>(sql, new { numberOfRecordsToFetch });
    }
    //Update File ThreatType
    public async Task<IEnumerable<FileRecord>> UpdateThreatType(string hash256, string threatType)
    {
        using var connection = _context.CreateConnection();
        var sql = """
           UPDATE FilesRecord
               SET ThreatType = @threatType
             WHERE HashValue = @hash256           
        """;
        return await connection.QueryAsync<FileRecord>(sql, new { hash256, threatType });
    }

    //Update File ThreatType
    public async Task<IEnumerable<FileRecord>> UpdateAnalyzedDate(string hash256, DateTime AnalyzedDate)
    {
        using var connection = _context.CreateConnection();
        var sql = """
           UPDATE FilesRecord
               SET AnalyzedDate = @AnalyzedDate
             WHERE HashValue = @hash256           
        """;
        return await connection.QueryAsync<FileRecord>(sql, new { hash256, AnalyzedDate });
    }
    //Update File Status
    public async Task<IEnumerable<FileRecord>> UpdateStatus(string hash256, string status)
    {
        using var connection = _context.CreateConnection();
        var sql = """
           UPDATE FilesRecord
               SET Status = @status
             WHERE HashValue = @hash256           
        """;
        return await connection.QueryAsync<FileRecord>(sql, new { hash256, status });
    }

    //Update Severity Type
    public async Task<IEnumerable<FileRecord>> UpdateSeverity(string hash256, string severity)
    {
        using var connection = _context.CreateConnection();
        var sql = """
           UPDATE FilesRecord
               SET Severity = @severity
             WHERE HashValue = @hash256           
        """;
        return await connection.QueryAsync<FileRecord>(sql, new { hash256, severity });
    }
    //Get Count Records

    public async Task<IEnumerable<DashboardCount>> GetVariousCount()
    {
        using var connection = _context.CreateConnection();
        var sql = """
           SELECT  (
            SELECT COUNT(*)
            FROM   FilesRecord
            ) AS TotalCount,
            (
            SELECT COUNT(*)
            FROM   FilesRecord
            where Status like 'Queued'
            ) AS QueuedCount,
            (
            SELECT COUNT(*)
            FROM   FilesRecord
             where ThreatType IS NOT NULL AND ThreatType != ""
            ) AS AnalyzedCount,
            (
            SELECT count(DISTINCT ThreatType)
            FROM   FilesRecord
            where ThreatType IS NOT NULL AND ThreatType != ""
            ) AS DifferentThreatType
        """;
        return await connection.QueryAsync<DashboardCount>(sql);
    }

    public async Task<IEnumerable<FileRecord>> GetByHash(string hash256)
    {
        using var connection = _context.CreateConnection();
        var sql = """
           SELECT 
             FileName,
             HashValue,
             UploadedBy,
             UploadedDate,
             ThreatType,
             Status,
             Isold,
             Extension,
             ContentType,
             Size,
             AnalyzedDate,
             Data,
             Severity,
             C2Countries,
             C2Communication,
             CurrentStage,
             Extractor,
             Validator,
             Parser,
             JsonData,
             MITRE,
        infected_countries
        FROM FilesRecord
            WHERE  HashValue=@hash256
        """;
        return await connection.QueryAsync<FileRecord>(sql, new { hash256 });
    }


    public void Create(CreateFileRecord model)
    {
        var existingRecord = _context.FilesRecord.SingleOrDefault(m => m.HashValue == model.HashValue);
        // validate
       
            if (existingRecord != null)
            {
                
                    // copy model to user and save
                    _mapper.Map(model, existingRecord);
                    _context.FilesRecord.Update(existingRecord);
                    _context.SaveChanges();
                   // throw new AppException("File with the same hash256 '" + model.HashValue + "' already exists"); 
                           
            }
            else
            {
                // map model to new file object
                var file = _mapper.Map<FileRecord>(model);
                // save file
                _context.FilesRecord.Add(file);
                _context.SaveChanges();
            }
       
       
    }


    // helper methods

    private FileRecord getFile(int id)
    {
        var file = _context.FilesRecord.Find(id);
        if (file == null) throw new KeyNotFoundException("File not found");
        return file;
    }

    //Save File On Server
    public void SaveFileOnDisk(CreateFileRecord model)
    {
        if(model == null) throw new ArgumentNullException("Provided File details are Null");
        if (model.Data !=null)
        {
            string currentPath = Directory.GetCurrentDirectory();
            string uploadFilePath = _localfilePathToStore;
            if (_useCurrentPath.ToUpper() == "TRUE")
            {
                uploadFilePath = Path.Join(currentPath, _targetfolderNameToUploadFile);

            }
            else
            {
                uploadFilePath = Path.Join(_localfilePathToStore, _targetfolderNameToUploadFile);
            }
            if (!Directory.Exists(uploadFilePath))
            {
                Directory.CreateDirectory(uploadFilePath);
            }
            var fileNameToBeCreated = model.HashValue + model.Extension;
            var targetFileFullPath = Path.Join(uploadFilePath, fileNameToBeCreated);

            using var stream = File.Create(targetFileFullPath);
            stream.Write(model.Data, 0, model.Data.Length);
        }
        else
        {
            throw new ArgumentNullException("Provided File data byte is Null");
        }
    }

    //Update  File Records with Final Json File
    public void UpdateRecordWithOutPutJsonValue(FileRecord record)
    {
        var existingMalware = _context.FilesRecord.SingleOrDefault(m => m.HashValue == record.HashValue);

        if (existingMalware != null)
        {
            if(record.UploadedDate ==null)
            {
                existingMalware.UploadedDate = DateTime.Now;
            }
            else
            {
                existingMalware.UploadedDate = record.UploadedDate;
            }
            existingMalware.ThreatType = record.ThreatType;
            existingMalware.AnalyzedDate = record.AnalyzedDate;
            existingMalware.Severity = record.Severity;
            existingMalware.Status = record.Status;
            existingMalware.MITRE = record.MITRE;
            existingMalware.C2Countries = record.C2Countries;
            existingMalware.infected_countries = record.infected_countries;
            existingMalware.JsonData = record.JsonData;
            existingMalware.C2Communication = record.C2Communication;
        }
        else
        {
            var file = _mapper.Map<FileRecord>(record);
            _context.FilesRecord.Add(record);
        }
        _context.SaveChanges();
    }
}