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
    }
}
