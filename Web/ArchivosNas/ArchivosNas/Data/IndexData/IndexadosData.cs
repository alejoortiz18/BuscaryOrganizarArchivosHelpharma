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
    }
}
