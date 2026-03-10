using ArchivosNas.Models.Dto;
using ArchivosNas.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArchivosNas.Data.IndexData
{
    public class IndexadosData
    {
        private readonly AppDbContext _context;

        public IndexadosData(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> ObtenerDashboard()
        {
            var totalArchivos = await _context.ArchivosIndexados.CountAsync();

            var duplicados = await _context.ArchivosIndexados
                .GroupBy(x => x.NombreArchivo)
                .Where(g => g.Count() > 1)
                .CountAsync();

            return new DashboardDto
            {
                TotalArchivos = totalArchivos,
                TotalDuplicadosNombre = duplicados
            };
        }

        public async Task<(List<ResultadoBusquedaDto> resultados, int total)> Buscar(BusquedaDto filtro)
        {
            const int pageSize = 20;

            var query = _context.ArchivosIndexados.AsQueryable();

            if (!string.IsNullOrEmpty(filtro.NombreArchivo))
            {
                query = query.Where(x => x.NombreArchivo.Contains(filtro.NombreArchivo));
            }

            if (!string.IsNullOrEmpty(filtro.Prefijo))
            {
                query = query.Where(x => x.Prefijo == filtro.Prefijo);
            }

            if (!string.IsNullOrEmpty(filtro.NumeroFactura))
            {
                query = query.Where(x => x.NumeroFactura == filtro.NumeroFactura);
            }

            var total = await query.CountAsync();

            var resultados = await query
                .OrderBy(x => x.Id)
                .Skip((filtro.Pagina - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ResultadoBusquedaDto
                {
                    Id = x.Id,
                    RutaCompleta = x.RutaCompleta,
                    NombreArchivo = x.NombreArchivo,
                    Extension = x.Extension,
                    Prefijo = x.Prefijo,
                    NumeroFactura = x.NumeroFactura
                })
                .ToListAsync();

            return (resultados, total);
        }

        public async Task<List<ResultadoBusquedaDto>> BuscarPorListado(List<string> facturas)
        {
            return await _context.ArchivosIndexados
                .Where(x => facturas.Contains(x.NumeroFactura))
                .Select(x => new ResultadoBusquedaDto
                {
                    Id = x.Id,
                    RutaCompleta = x.RutaCompleta,
                    NombreArchivo = x.NombreArchivo,
                    Extension = x.Extension,
                    Prefijo = x.Prefijo,
                    NumeroFactura = x.NumeroFactura
                })
                .ToListAsync();
        }
    }
}
