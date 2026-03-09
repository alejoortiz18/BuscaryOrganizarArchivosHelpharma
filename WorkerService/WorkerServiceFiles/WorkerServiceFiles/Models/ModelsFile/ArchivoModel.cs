using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerServiceFiles.Models.ModelsFile
{
    public class ArchivoModel
    {
        public long Id { get; set; }

        public string RutaCompleta { get; set; } = string.Empty;

        public string NombreArchivo { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public string? Prefijo { get; set; }

        public string? NumeroFactura { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaModificacion { get; set; }
    }
}
