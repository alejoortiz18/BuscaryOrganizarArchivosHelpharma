using System;
using System.Collections.Generic;

namespace ArchivosNas.Models.Entities;

public partial class ArchivosIndexado
{
    public long Id { get; set; }

    public string RutaCompleta { get; set; } = null!;

    public string NombreArchivo { get; set; } = null!;

    public string Extension { get; set; } = null!;

    public string? Prefijo { get; set; }

    public string? NumeroFactura { get; set; }

    public byte[]? RutaHash { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }
}
