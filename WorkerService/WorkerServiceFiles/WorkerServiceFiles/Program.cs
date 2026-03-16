using WorkerServiceFiles;
using WorkerServiceFiles.Data;
using WorkerServiceFiles.Models;
using WorkerServiceFiles.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "IndexadorArchivosNAS";
});

builder.Services.Configure<NasSettings>(
    builder.Configuration.GetSection("NasSettings"));

builder.Services.Configure<IndexerSettings>(
    builder.Configuration.GetSection("IndexerSettings"));

builder.Services.AddScoped<SqlRepository>();

builder.Services.AddScoped<FileIndexerService>();

builder.Services.AddSingleton<FileWatcherService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();