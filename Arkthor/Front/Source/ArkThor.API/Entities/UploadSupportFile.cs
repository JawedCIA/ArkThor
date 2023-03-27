namespace ArkThor.API.Entities;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


public class UploadSupportFile
{

    public string? FileName { get; set;}
    [Key]
    public int Id { get; set;}
   
    public string HashValue { get; set; }

    public string? UploadedBy { get; set; }

    public DateTime? UploadedDate { get; set; }
    public string? Extension { get; set; }
  
    public string? ContentType { get; set; }
   
    public Int64? Size { get; set; }
   
    public byte[]? Data { get; set; }
    public int? Isold { get; set; }
   

}
