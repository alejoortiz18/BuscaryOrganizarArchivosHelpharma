namespace ArchivosNas.Models.Dto
{
    public class BusquedaDto
    {
        public string? NombreArchivo { get; set; }

        public string? Prefijo { get; set; }

        public string? NumeroFactura { get; set; }

        public int Pagina { get; set; } = 1;
    }
}
