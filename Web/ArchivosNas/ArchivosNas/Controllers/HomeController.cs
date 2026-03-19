using ArchivosNas.Data.IndexData;
using ArchivosNas.Models;
using ArchivosNas.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO.Compression;

namespace ArchivosNas.Controllers
{
    public class HomeController : Controller
    {
        private readonly IndexadosData _indexadosData;

        private static Dictionary<string, int> progresoPorJob = new();
        private static Dictionary<string, string> zipPorJob = new();


        public HomeController(IndexadosData indexadosData)
        {
            _indexadosData = indexadosData;
        }

        public async Task<IActionResult> Index()
        {
            var dashboard = await _indexadosData.ObtenerDashboard();

            return View(dashboard);
        }


        [HttpPost]
        public async Task<IActionResult> Buscar(BusquedaDto filtro)
        {
            var resultado = await _indexadosData.Buscar(filtro);

            ViewBag.Total = resultado.total;
            ViewBag.Pagina = filtro.Pagina;
            ViewBag.PageSize = 20;

            // 🔥 ESTO ES LO QUE TE FALTABA
            ViewBag.NombreArchivo = filtro.NombreArchivo;
            ViewBag.Prefijo = filtro.Prefijo;
            ViewBag.NumeroFactura = filtro.NumeroFactura;

            return View("Resultados", resultado.resultados);
        }

      

        [HttpPost]
        public async Task<IActionResult> ProcesarListado(ProcesarListadoDto model, int pagina = 1, List<string> Facturas = null)
        {
            List<string> facturas;

            // 🔥 SI VIENE ARCHIVO (primera vez)
            if (model.Archivo != null && model.Archivo.Length > 0)
            {
                facturas = new List<string>();

                using (var reader = new StreamReader(model.Archivo.OpenReadStream()))
                {
                    while (!reader.EndOfStream)
                    {
                        var linea = await reader.ReadLineAsync();

                        if (!string.IsNullOrWhiteSpace(linea))
                            facturas.Add(linea.Trim());
                    }
                }
            }
            else
            {
                // 🔥 SI VIENE DE PAGINACIÓN
                if (Facturas == null || !Facturas.Any())
                    return BadRequest("No hay datos para procesar");

                facturas = Facturas;
            }

            var encontrados = await _indexadosData.BuscarPorListado(facturas);
            HttpContext.Session.SetString(
                "EncontradosCache",
                System.Text.Json.JsonSerializer.Serialize(encontrados)
            );
            var encontradosSet = encontrados
                .Select(x => x.NumeroFactura)
                .ToHashSet();

            var noEncontrados = facturas
                .Where(x => !encontradosSet.Contains(x))
                .ToList();

            // 🔥 PAGINACIÓN
            int pageSize = 20;
            var total = encontrados.Count;

            var encontradosPaginados = encontrados
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var resultado = new ResultadoListadoDto
            {
                Encontrados = encontradosPaginados,
                NoEncontrados = noEncontrados
            };
            ViewBag.TodosEncontrados = encontrados;
            // 🔥 VIEWBAG
            ViewBag.Total = total;
            ViewBag.Pagina = pagina;
            ViewBag.PageSize = pageSize;
            ViewBag.Facturas = facturas;
            ViewBag.RutaDestino = model.RutaDestino;

            return View("ResultadoListado", resultado);
        }

        [HttpPost]
        public async Task<IActionResult> IniciarZip()
        {
            var jobId = Guid.NewGuid().ToString();

            progresoPorJob[jobId] = 0;

            var encontradosJson = HttpContext.Session.GetString("EncontradosCache");

            var encontrados = System.Text.Json.JsonSerializer
                .Deserialize<List<ResultadoBusquedaDto>>(encontradosJson);

            var ids = encontrados.Select(x => x.Id).ToList();

            var archivos = await _indexadosData.ObtenerPorIds(ids);

            var zipPath = Path.Combine(Path.GetTempPath(), $"archivos_{jobId}.zip");
            zipPorJob[jobId] = zipPath;

            _ = Task.Run(() =>
            {
                using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    int total = archivos.Count;
                    int actual = 0;

                    foreach (var archivo in archivos)
                    {
                        AgregarArchivoAlZip(zip, archivo.RutaCompleta, archivo.NombreArchivo);

                        actual++;
                        progresoPorJob[jobId] = (int)((actual * 100.0) / total);
                    }
                }

                progresoPorJob[jobId] = 100;
            });

            return Json(new { jobId });
        }


        [HttpGet]
        public IActionResult ProgresoZip(string jobId)
        {
            if (!progresoPorJob.ContainsKey(jobId))
                return Json(new { porcentaje = 0 });

            return Json(new { porcentaje = progresoPorJob[jobId] });
        }


        [HttpGet]
        public async Task<IActionResult> DescargarZipFinal(string jobId)
        {
            if (!zipPorJob.ContainsKey(jobId))
                return BadRequest();

            var path = zipPorJob[jobId];

            var bytes = await System.IO.File.ReadAllBytesAsync(path);

            return File(bytes, "application/zip", "archivos.zip");
        }


        [HttpPost]
        public async Task<IActionResult> GuardarArchivos(string rutaDestino, List<long> ids)
        {
            var archivos = await _indexadosData.ObtenerPorIds(ids);

            await _indexadosData.CopiarArchivos(archivos, rutaDestino);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DescargarZip()
        {
            var encontradosJson = HttpContext.Session.GetString("EncontradosCache");

            if (string.IsNullOrEmpty(encontradosJson))
                return BadRequest("No hay datos en sesión");

            var encontrados = System.Text.Json.JsonSerializer
                .Deserialize<List<ResultadoBusquedaDto>>(encontradosJson);

            var ids = encontrados.Select(x => x.Id).ToList();

            var archivos = await _indexadosData.ObtenerPorIds(ids);

            var zipPath = Path.Combine(Path.GetTempPath(), $"archivos_{Guid.NewGuid()}.zip");

            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var archivo in archivos)
                {
                    AgregarArchivoAlZip(zip, archivo.RutaCompleta, archivo.NombreArchivo);
                }
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(zipPath);

            return File(bytes, "application/zip", "archivos.zip");
        }

        [HttpPost]
        public async Task<IActionResult> DescargarZip(BusquedaDto filtro)
        {
            var archivos = await _indexadosData.BuscarTodos(filtro);

            var zipPath = Path.Combine(Path.GetTempPath(), $"archivos_{Guid.NewGuid()}.zip");

            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var archivo in archivos)
                {
                    AgregarArchivoAlZip(zip, archivo.RutaCompleta, archivo.NombreArchivo);
                }
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(zipPath);

            return File(bytes, "application/zip", "archivos.zip");
        }

        public async Task<IActionResult> AbrirArchivo(string ruta, string nombre)
        {
            if (string.IsNullOrEmpty(ruta))
                return BadRequest();
            string splitPath;
            if (ruta.Contains("|"))
            {
                string[] split = ruta.Split("|");
                ruta = split[0];
                split = nombre.Split(".");
                nombre = ($"{split[0]}.zip");
            }

            if (!System.IO.File.Exists(ruta))
                return NotFound("Archivo no encontrado");

            var bytes = await System.IO.File.ReadAllBytesAsync(ruta);

            return File(bytes, "application/octet-stream", nombre);
        }

        [HttpPost]
        public async Task<IActionResult> OrganizarRadicacion(ProcesarListadoDto model)
        {
            if (model.Archivo == null || model.Archivo.Length == 0)
                return BadRequest("Debe subir archivo");

            var facturas = new List<string>();

            using (var reader = new StreamReader(model.Archivo.OpenReadStream()))
            {
                while (!reader.EndOfStream)
                {
                    var linea = await reader.ReadLineAsync();

                    if (!string.IsNullOrWhiteSpace(linea))
                        facturas.Add(linea.Trim());
                }
            }

            var archivos = await _indexadosData.BuscarPorFacturas(facturas);

            var zipPath = Path.Combine(Path.GetTempPath(), $"radicacion_{Guid.NewGuid()}.zip");

            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var factura in facturas)
                {
                    var archivosFactura = archivos
                        .Where(x => x.NumeroFactura == factura)
                        .ToList();

                    var json = archivosFactura
                        .FirstOrDefault(x => x.Extension.ToLower() == ".json");

                    string? nombreCarpeta = null;

                    if (json != null)
                    {
                        nombreCarpeta = Path.GetFileNameWithoutExtension(json.NombreArchivo);
                    }

                    foreach (var archivo in archivosFactura)
                    {
                        var nombre = archivo.NombreArchivo.ToLower();
                        var ext = archivo.Extension.ToLower();

                        bool esJson = ext == ".json";
                        bool esCuv = nombre.Contains("-cuv");

                        // 🔥 SI VA EN CARPETA
                        if (nombreCarpeta != null && (esJson || esCuv))
                        {
                            var rutaZip = $"{nombreCarpeta}/{archivo.NombreArchivo}";
                            AgregarArchivoAlZip(zip, archivo.RutaCompleta, rutaZip);
                        }
                        else
                        {
                            // 🔥 NORMAL (sin carpeta)
                            AgregarArchivoAlZip(zip, archivo.RutaCompleta, archivo.NombreArchivo);
                        }
                    }
                }
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(zipPath);

            return File(bytes, "application/zip", "radicacion.zip");
        }


        private void AgregarArchivoAlZip(ZipArchive zipDestino, string rutaCompleta, string nombreDestino)
        {
            try
            {
                if (rutaCompleta.Contains("|"))
                {
                    var partes = rutaCompleta.Split('|');

                    var rutaZip = partes[0];
                    var rutaInterna = partes[1].Replace("\\", "/");

                    if (!System.IO.File.Exists(rutaZip))
                        return;

                    using (var zipOrigen = ZipFile.OpenRead(rutaZip))
                    {
                        var entry = zipOrigen.Entries
                            .FirstOrDefault(e => e.FullName.Replace("\\", "/") == rutaInterna);

                        if (entry == null)
                            return;

                        var nuevaEntrada = zipDestino.CreateEntry(nombreDestino);

                        using (var streamOrigen = entry.Open())
                        using (var streamDestino = nuevaEntrada.Open())
                        {
                            streamOrigen.CopyTo(streamDestino);
                        }
                    }
                }
                else
                {
                    if (!System.IO.File.Exists(rutaCompleta))
                        return;

                    zipDestino.CreateEntryFromFile(rutaCompleta, nombreDestino);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando archivo: {rutaCompleta} - {ex.Message}");
            }
        }
    }
}
