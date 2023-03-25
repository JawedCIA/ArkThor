namespace ArkThor.API.Models.Records;

using System.ComponentModel.DataAnnotations;
using ArkThor.API.Entities;

public class CreateFileRecord
{
    [Required]
    public string FileName { get; set; }
    [Required]
    public string HashValue { get; set; }

    public string? UploadedBy { get; set; }

    public DateTime? UploadedDate { get; set; }
    public string? Extension { get; set; }

    public string? ThreatType { get; set; }
    public string? ContentType { get; set; }
    public string? Status { get; set; }
    public Int64? Size { get; set; }
    public byte[]? Data { get; set; }
    public DateTime? AnalyzedDate { get; set; }
    public int? Isold { get; set; }
    public int? Severity { get; set; }
}