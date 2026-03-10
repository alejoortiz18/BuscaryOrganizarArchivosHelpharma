namespace ArchivosNas.Models.Dto
{
    public class ResultadoListadoDto
    {
        public List<ResultadoBusquedaDto> Encontrados { get; set; } = new();

        public List<string> NoEncontrados { get; set; } = new();
    }
}
