using Indexador.Data;
using Indexador.Models;
using Indexador.Services;

var rutaBase = @"\\192.168.0.69\Informes";

var repo = new SqlRepository();

Console.WriteLine("=== INICIANDO INDEXACIÓN DESDE NAS ===");

// 🔥 NUEVO FLUJO
var ultimaEjecucion = await repo.ObtenerUltimaEjecucionAsync();

var rutas = FileScannerService.EscanearArchivos(rutaBase, ultimaEjecucion);
var archivos = FileParserService.ParsearArchivos(rutas);

var lote = new List<ArchivoModel>(20000);

long contador = 0;
int contadorLotes = 0;

foreach (var archivo in archivos)
{
    lote.Add(archivo);
    contador++;

    if (lote.Count >= 20000)
    {
        await repo.BulkInsertAsync(lote);
        lote.Clear();

        contadorLotes++;

        if (contadorLotes % 5 == 0)
        {
            Console.WriteLine("🔄 Ejecutando merge SP...");
            await repo.EjecutarMergeSPAsync();
        }

        Console.WriteLine($"Procesados: {contador}");
    }
}

// Último lote
if (lote.Count > 0)
{
    await repo.BulkInsertAsync(lote);
}

// Merge final
Console.WriteLine("🔄 Ejecutando merge final...");
await repo.EjecutarMergeSPAsync();

Console.WriteLine($"=== INDEXACIÓN TERMINADA ===");
Console.WriteLine($"TOTAL: {contador}");