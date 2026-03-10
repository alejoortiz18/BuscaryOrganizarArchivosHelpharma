namespace ArchivosNas.Models.Dto
{
    public class ResultadoBusquedaDto
    {
        public long Id { get; set; }

        public string RutaCompleta { get; set; }

        public string NombreArchivo { get; set; }

        public string Extension { get; set; }

        public string? Prefijo { get; set; }

        public string? NumeroFactura { get; set; }
    }
}
