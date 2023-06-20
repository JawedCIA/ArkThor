namespace ArkThor.API.Helpers;

using Microsoft.EntityFrameworkCore;
using System.Data;
using ArkThor.API.Entities;
using Dapper;
using Microsoft.Data.Sqlite;
using Polly;
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
        // options.UseSqlite(Configuration.GetConnectionString("ArkThorDatabase"));
        var connectionStringBuilder = new SqliteConnectionStringBuilder(Configuration.GetConnectionString("ArkThorDatabase"))
        {
            DefaultTimeout = 30, // Increase the default timeout value
        };

        // Retry policy for SQLite locking errors
        var retryPolicy = Policy
            .Handle<SqliteException>(ex => ex.ErrorCode == 5) // Check for SQLite Error 5: 'database is locked'
            .Retry(3, (exception, retryCount) =>
            {
                // Optionally, log the retry attempts
                Console.WriteLine($"Retry #{retryCount} due to locking error: {exception.Message}");
            });

        // Connect to the SQLite database with retry policy
        retryPolicy.Execute(() => options.UseSqlite(connectionStringBuilder.ConnectionString));
    }


    public IDbConnection CreateConnection()
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder(Configuration.GetConnectionString("ArkThorDatabase"))
        {
            DefaultTimeout = 30, // Increase the default timeout value
        };

        IDbConnection connection = null;

        var retryPolicy = Policy
            .Handle<SqliteException>(ex => ex.ErrorCode == 5) // Check for SQLite Error 5: 'database is locked'
            .Retry(3, (exception, retryCount) =>
            {
                // Optionally, log the retry attempts
                Console.WriteLine($"Retry #{retryCount} due to locking error: {exception.Message}");
            });

        retryPolicy.Execute(() =>
        {
            connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            connection.Open();
        });

        return connection;
    }

    public async Task Init()
    {
        // create database tables if they don't exist
        using var connection = CreateConnection();
   
        await _initFilesRecord();
        await _initSupportFile();

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
        async Task _initSupportFile()
        {
            var sql = """
              CREATE TABLE IF NOT EXISTS 
             SupportFile (
                HashValue    TEXT    NOT NULL,
                FileName     TEXT,
                UploadedBy   TEXT,
                UploadedDate NUMERIC,
                Extension    TEXT,
                ContentType  TEXT,
                Size         TEXT,
                Data         BLOB,
                Isold        INTEGER,
                ID           INTEGER PRIMARY KEY
                                UNIQUE
                                NOT NULL
            );
            
            """;
            await connection.ExecuteAsync(sql);
        }
    }

    public DbSet<FileRecord> FilesRecord { get; set; }
    public DbSet<DashboardCount> DashboardCounts { get; set; }
    public DbSet<DistributionCount> DistributionCounts { get; set; }

    public DbSet<UploadSupportFile> SupportFile { get; set; }

}