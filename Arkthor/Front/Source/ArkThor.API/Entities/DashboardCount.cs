namespace ArkThor.API.Entities;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

[Keyless]
public class DashboardCount
{
   
    public string? TotalCount { get; set; }
    public string? QueuedCount { get; set; }
    public string? AnalyzedCount { get; set; }
    public string? DifferentThreatType { get; set; }

}