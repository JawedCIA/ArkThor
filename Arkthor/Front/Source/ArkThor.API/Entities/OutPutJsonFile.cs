namespace ArkThor.API.Entities;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

[Keyless]
public class OutPutJsonFile
{
    public string SHA256 { get; set; }
    public string? rule_name { get; set; }
    public long? analyzed_time { get; set; }
    public int? severity { get; set; }
    public string[]? MITRE { get; set; }
    public string[]? c2_countries { get; set; }
    public string[]? infected_countries { get; set; }
    
        public string[]? C2Communication { get; set; }
    public byte[]? JsonData { get; set; }
    public string? Status { get; set; }

}
