namespace ArchivosNas.Models.Dto
{
    public class SoporteResponseDto
    {
        public string IdConvenio { get; set; }
        public string NombreConvenio { get; set; }

        public DateTime Fecha { get; set; }

        public string IdBodega { get; set; }
        public string NombreSede { get; set; }

        public string NombreActividad { get; set; }

        public string TipoEntrega { get; set; }
        public string TipoPlan { get; set; }

        public string IdCartera { get; set; }

        public string NombrePaciente { get; set; }
        public string IdTipoId { get; set; }

        public int IdPaciente { get; set; }

        public string Celular { get; set; }

        public string Telefono { get; set; }

        public string Direccion { get; set; }
        public string Complemento { get; set; }
        public string Observacion { get; set; }
        public string ValorCM { get; set; }

        public List<MedicamentoDto> medicamentos { get; set; }
    }

    public class MedicamentoDto
    {
        public string ordenes { get; set; }
        public string producto { get; set; }
        public string nombre { get; set; }
        public int cantidad { get; set; }
    }
}
