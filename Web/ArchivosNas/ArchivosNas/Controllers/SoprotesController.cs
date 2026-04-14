using ArchivosNas.Models.Dto;
using ArchivosNas.Services;
using Microsoft.AspNetCore.Mvc;

public class SoportesController : Controller
{
    private readonly SoporteApiService _soporteApi;
    private readonly SoporteFisicoApiService _soporteFisicoApi;
    private readonly ILogger<SoportesController> _logger;

    public SoportesController(
        SoporteApiService soporteApi,
        SoporteFisicoApiService soporteFisicoApi,
        ILogger<SoportesController> logger)
    {
        _soporteApi = soporteApi;
        _soporteFisicoApi = soporteFisicoApi;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Procesar(ProcesarManualDto model)
    {
        var resultados = new List<string>();

        foreach (var item in model.Items)
        {
            if (string.IsNullOrEmpty(item.Soporte) || item.Archivo == null)
            {
                resultados.Add("Fila inválida");
                continue;
            }

            var rutaTemp = Path.Combine(Path.GetTempPath(), item.Archivo.FileName);

            try
            {
                using (var stream = new FileStream(rutaTemp, FileMode.Create))
                {
                    await item.Archivo.CopyToAsync(stream);
                }

                var respuesta = await _soporteApi.EnviarSoporteAsync(item.Soporte);

                if (respuesta == null)
                {
                    resultados.Add($"{item.Soporte} → Error LEER DATOS");
                    continue;
                }

                var enviado = await _soporteFisicoApi.EnviarSoporteFisicoAsync(
                    item.Soporte,
                    rutaTemp,
                    respuesta
                );

                if (!enviado)
                {
                    resultados.Add($"{item.Soporte} → Error ENVIANDO ARCHIVO");
                    continue;
                }

                resultados.Add($"{item.Soporte} → OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error manual");
                resultados.Add($"{item.Soporte} → Error interno");
            }
            finally
            {
                if (System.IO.File.Exists(rutaTemp))
                    System.IO.File.Delete(rutaTemp);
            }
        }

        ViewBag.Resultados = resultados;
        return View("Index");
    }
}