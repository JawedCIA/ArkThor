using System.Text.Json.Serialization;
using ArkThor.API.Helpers;
using ArkThor.API.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// add services to DI container
{
    var services = builder.Services;
    var env = builder.Environment;

    services.AddDbContext<DataContext>();
    services.AddCors(options =>
    {
        options.AddPolicy("AllowArkthorDomain", builder =>
        {
            builder.WithOrigins(
                     "http://arkthor.westeurope.cloudapp.azure.com",
                     "https://arkthor.com",
                     "http://arkthor.com"
                 )
                 .AllowAnyHeader()
                 .AllowAnyMethod();
        });
    });
    services.AddControllers().AddJsonOptions(x =>
    {
        // serialize enums as strings in api responses (e.g. Role)
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        // ignore omitted parameters on models to enable optional params (e.g. User update)
        x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
    services.Configure<FormOptions>(options =>
    {
        // Set the limit to 256 MB
        options.ValueLengthLimit = int.MaxValue;
        options.MultipartBodyLengthLimit = int.MaxValue; // if don't set default value is: 128 MB
        options.MultipartHeadersLengthLimit = int.MaxValue;

    });
    services.Configure<IISServerOptions>(options =>
    {
        options.MaxRequestBodySize = int.MaxValue;
    });

    services.Configure<KestrelServerOptions>(options =>
    {
        options.Limits.MaxRequestBodySize = int.MaxValue; // if don't set default value is: 30 MB
        options.AllowSynchronousIO = true; // Enable streaming
    });
    services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    // configure DI for application services

    services.AddScoped<IFileRecordService, FileRecordService>();
    services.AddScoped<ISupportFileService, SupportFileService>();
    services.AddScoped<IRabbitMQService, RabbitMQProducer>();
}
var app = builder.Build();

// ensure database and tables exist
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    await context.Init();
}

// Configure the HTTP request pipeline.
//app.UseSwagger();
//app.UseSwaggerUI();
// configure HTTP request pipeline
{
    // global cors policy
    app.UseCors("AllowArkthorDomain");

    // global error handler
    app.UseMiddleware<ErrorHandlerMiddleware>();

    app.MapControllers();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
