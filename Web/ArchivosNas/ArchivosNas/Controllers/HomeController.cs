using ArchivosNas.Data.IndexData;
using ArchivosNas.Models;
using ArchivosNas.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ArchivosNas.Controllers
{
    public class HomeController : Controller
    {
        private readonly IndexadosData _indexadosData;

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

            return View("Resultados", resultado.resultados);
        }

        [HttpPost]
        public async Task<IActionResult> ProcesarListado(ProcesarListadoDto model)
        {
            if (model.Archivo == null || model.Archivo.Length == 0)
                return BadRequest("Debe subir un archivo");

            if (string.IsNullOrEmpty(model.RutaDestino))
                return BadRequest("Debe indicar ruta destino");

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

            var encontrados = await _indexadosData.BuscarPorListado(facturas);

            var encontradosSet = encontrados
                .Select(x => x.NumeroFactura)
                .ToHashSet();

            var noEncontrados = facturas
                .Where(x => !encontradosSet.Contains(x))
                .ToList();

            var resultado = new ResultadoListadoDto
            {
                Encontrados = encontrados,
                NoEncontrados = noEncontrados
            };

            ViewBag.RutaDestino = model.RutaDestino;

            return View("ResultadoListado", resultado);
        }
    }
}
