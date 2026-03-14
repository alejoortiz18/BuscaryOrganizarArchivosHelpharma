using System;
using System.Collections.Generic;
using System.Text;

namespace Indexador.Models
{
    public class ArchivoModel
    {
        public string RutaCompleta { get; set; } = "";
        public string NombreArchivo { get; set; } = "";
        public string Extension { get; set; } = "";
        public string? Prefijo { get; set; }
        public string? NumeroFactura { get; set; }
    }
}
