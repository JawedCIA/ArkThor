namespace ArkThor.API.Helpers;

using Microsoft.EntityFrameworkCore;
using System.Data;
using ArkThor.API.Entities;
using Dapper;
using Microsoft.Data.Sqlite;
public class DataContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public DataContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // connect to sqlite database
        options.UseSqlite(Configuration.GetConnectionString("WebApiDatabase"));
    }


    public IDbConnection CreateConnection()
    {
        return new SqliteConnection(Configuration.GetConnectionString("WebApiDatabase"));
    }

    public async Task Init()
    {
        // create database tables if they don't exist
        using var connection = CreateConnection();
        await _initUsers();
        await _initFilesRecord();
        async Task _initUsers()
        {
            var sql = """
                CREATE TABLE IF NOT EXISTS 
                Users (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    Title TEXT,
                    FirstName TEXT,
                    LastName TEXT,
                    Email TEXT,
                    Role INTEGER,
                    PasswordHash TEXT
                );
            """;
            await connection.ExecuteAsync(sql);
        }
        async Task _initFilesRecord()
        {
            var sql = """
              CREATE TABLE IF NOT EXISTS 
             FilesRecord (
               
               
                HashValue          TEXT    NOT NULL
                                           PRIMARY KEY
                                           UNIQUE,
                FileName           TEXT,
                UploadedBy         TEXT,
                UploadedDate       NUMERIC,
                ThreatType         TEXT,
                Status             TEXT,
                Isold              INTEGER,
                Extension          TEXT,
                ContentType        TEXT,
                Size               TEXT,
                AnalyzedDate       NUMERIC,
                Data               BLOB,
                Severity           INTEGER,
                C2Countries          TEXT,
                C2Communication    TEXT,
                CurrentStage       TEXT,
                Extractor          TEXT,
                Validator          TEXT,
                Parser             TEXT,
                JsonData           BLOB,
                MITRE              TEXT,
                infected_countries TEXT
            );
            
            
            """;
            await connection.ExecuteAsync(sql);
        }
    }
    public DbSet<User> Users { get; set; }
    public DbSet<FileRecord> FilesRecord { get; set; }
    public DbSet<DashboardCount> DashboardCounts { get; set; }
    public DbSet<DistributionCount> DistributionCounts { get; set; }

}