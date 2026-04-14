using ArchivosNas.Data.IndexData;
using ArchivosNas.Models.Entities;
using ArchivosNas.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IndexadosData>();
builder.Services.AddScoped<SoporteApiService>();
builder.Services.AddScoped<SoporteFisicoApiService>();
builder.Services.AddScoped<HttpClient>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

builder.Services.AddSession();
var app = builder.Build();
app.UseSession(); 


try
{
    var psi = new ProcessStartInfo
    {
        FileName = "cmd.exe",
        Arguments = "/c net use \\\\192.168.0.69\\Informes /user:ServiciosRelease\\radicacion h3lph@rm@,+",
        CreateNoWindow = true,
        UseShellExecute = false
    };

    Process.Start(psi);
}
catch (Exception ex)
{
    Console.WriteLine("Error conectando a NAS: " + ex.Message);
}

 //Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Index");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseExceptionHandler("/Home/Index");
app.UseStatusCodePagesWithReExecute("/Home/Index");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
//dotnet ef dbcontext scaffold "Server=ServiciosReleas\SQLEXPRESS;Database=FilesNas;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --output-dir Models/Entities --context AppDbContext --force --project "C:\Users\serviciosrelease\Documents\Reportes\GitCodigo\Web\ArchivosNas\ArchivosNas\ArchivosNas.csproj"


