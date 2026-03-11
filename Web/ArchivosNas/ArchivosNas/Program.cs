using ArchivosNas.Data.IndexData;
using ArchivosNas.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IndexadosData>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));
var app = builder.Build();


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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
//dotnet ef dbcontext scaffold "Server=(localdb)\MSSQLLocalDB;Database=FilesNas;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --output-dir Models/Entities --context AppDbContext --force --project "C:\Users\alejandro.ortiz\Documents\helpharma\Desarrollos\BuscarYOrganizarSoportes\BuscaryOrganizarArchivosHelpharma\Web\ArchivosNas\ArchivosNas\ArchivosNas.csproj"