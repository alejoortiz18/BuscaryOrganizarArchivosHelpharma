namespace ArchivosNas.Models.Dto
{
    public class ProcesarManualDto
    {
        public List<ItemSoporteDto> Items { get; set; } = new();
    }

    public class ItemSoporteDto
    {
        public string Soporte { get; set; }
        public IFormFile Archivo { get; set; }
    }
}
