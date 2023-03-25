namespace ArkThor.API.Entities;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


public class FileRecord
{

    public string? FileName { get; set; }
    [Key]
    public string HashValue { get; set; }

    public string? UploadedBy { get; set; }

    public DateTime? UploadedDate { get; set; }
    public string? Extension { get; set; }
    public string? ThreatType { get; set; }
    public string? ContentType { get; set; }
    public string? Status { get; set; }
    public int? Severity { get; set; }
    public Int64? Size { get; set; }
    public DateTime? AnalyzedDate { get; set; }
    public byte[]? Data { get; set; }
    public int? Isold { get; set; }
    public byte[]? JsonData { get; set; }
   
    public string? CurrentStage { get; set; }
    public string? Extractor { get; set; }
    public string? Validator { get; set; }
    public string? Parser { get; set; }
   
    public string? MITRE { get; set; }   
    public string? C2Communication { get; set; }
    public string? infected_countries { get; set; }    
  
    public string? C2Countries { get; set; }

}

public class IndexValue
{
    [Key]
    public int index { get; set; }
    public string value { get; set; } = null!;
}

public class C2countrie
{
    [Key]
    public int index { get; set; }
    public string value { get; set; } = null!;
}