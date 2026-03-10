using System;
using System.Collections.Generic;

namespace ArchivosNas.Models.Entities;

public partial class ArchivosIndexadosStaging
{
    public string RutaCompleta { get; set; } = null!;

    public string NombreArchivo { get; set; } = null!;

    public string Extension { get; set; } = null!;

    public string? Prefijo { get; set; }

    public string? NumeroFactura { get; set; }
}
