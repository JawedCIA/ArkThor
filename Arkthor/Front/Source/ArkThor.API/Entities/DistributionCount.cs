namespace ArkThor.API.Entities;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

[Keyless]
public class DistributionCount
{
   
    public DateTime? Date { get; set; }
    public string? TotalCount { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; }
    public string? Countries { get; set; }

}