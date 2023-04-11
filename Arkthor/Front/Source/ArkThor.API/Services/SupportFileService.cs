namespace ArkThor.API.Services;

using AutoMapper;
using BCrypt.Net;
using ArkThor.API.Entities;
using ArkThor.API.Helpers;
using Dapper;
using ArkThor.API.Models.Records;
using static System.Runtime.InteropServices.JavaScript.JSType;

using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

public interface ISupportFileService
{
    IEnumerable<UploadSupportFile> GetAll();
    Task<IEnumerable<UploadSupportFile>> GetSupportFiles(string sha256);

    void Create(UploadSupportFile model);
   
}

public class SupportFileService : ISupportFileService
{
    private DataContext _context;
    private readonly IMapper _mapper;

    public SupportFileService(
        DataContext context,
        IMapper mapper, IConfiguration config)
    {
        _context = context;
        _mapper = mapper;
        
    }

    public IEnumerable<UploadSupportFile> GetAll()
    {
        return _context.SupportFile;
    }
  
    public void Create(UploadSupportFile model)
    {
      //  var existingRecord = _context.SupportFile.SingleOrDefault(m => m.HashValue == model.HashValue);
        // validate
       
         //   if (existingRecord != null)
           // {
                
                    //// copy model to user and save
                    //_mapper.Map(model, existingRecord);
                    //_context.SupportFile.Update(existingRecord);
                    //_context.SaveChanges();
                   // throw new AppException("File with the same hash256 '" + model.HashValue + "' already exists"); 
                           
            //}
           // else
            //{
                // map model to new file object
                var file = _mapper.Map<UploadSupportFile>(model);
                // save file
                _context.SupportFile.Add(file);
                _context.SaveChanges();
            //}
       
       
    }


    // helper methods

    public async Task<IEnumerable<UploadSupportFile>> GetSupportFiles(string sha256)
    {
        using var connection = _context.CreateConnection();
        var sql = """
           SELECT *
        FROM SupportFile
        WHERE  HashValue=@sha256
        """;
        return await connection.QueryAsync<UploadSupportFile>(sql, new { sha256 });
    }


}