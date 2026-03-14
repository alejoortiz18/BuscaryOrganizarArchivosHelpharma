using Indexador.Data;
using Indexador.Services;
using Indexador.Models;
using Indexador.Parsers;

var rutaTxt = @"C:\Users\alejandro.ortiz\Documents\helpharma\Documentos\indexado masivo\Consolidados\consolidados.txt";

var repo = new SqlRepository();

Console.WriteLine("=== INDEXACIÓN BASE ===");

var lineas = TxtReaderService.LeerLineas(rutaTxt);
var archivos = ConsolidadosParser.Parsear(lineas);

var lote = new List<ArchivoModel>(5000);
long contador = 0;

foreach (var archivo in archivos)
{
    lote.Add(archivo);
    contador++;

    if (lote.Count >= 5000)
    {
        await repo.BulkInsertAsync(lote);
        await repo.EjecutarMergeAsync();

        lote.Clear();

        Console.WriteLine($"Insertados base: {contador}");
    }
}

if (lote.Count > 0)
{
    await repo.BulkInsertAsync(lote);
    await repo.EjecutarMergeAsync();
}

Console.WriteLine("=== INDEXACIÓN BASE TERMINADA ===");


Console.WriteLine("=== INICIANDO PROCESAMIENTO DE COMPRIMIDOS ===");

var compressedRepo = new CompressedRepository();
var processor = new CompressedProcessorService();

var comprimidos = compressedRepo.ObtenerComprimidos();

Console.WriteLine($"ZIP encontrados: {comprimidos.Count}");

if (comprimidos.Count == 0)
{
    Console.WriteLine("⚠️ No se encontraron comprimidos en la base.");
    return;
}

var loteZip = new List<ArchivoModel>(5000);
long totalProcesados = 0;

foreach (var zip in comprimidos)
{
    var internos = processor.Procesar(zip);

    Console.WriteLine($"ZIP {zip} contiene {internos.Count()} archivos internos");

    foreach (var archivo in internos)
    {
        loteZip.Add(archivo);

        if (loteZip.Count >= 5000)
        {
            await repo.BulkInsertAsync(loteZip);
            await repo.EjecutarMergeAsync();

            totalProcesados += loteZip.Count;
            loteZip.Clear();

            Console.WriteLine($"[PROGRESO] Archivos internos insertados: {totalProcesados}");
        }
    }
}

if (loteZip.Count > 0)
{
    await repo.BulkInsertAsync(loteZip);
    await repo.EjecutarMergeAsync();
}

Console.WriteLine($"=== FINALIZADO: {totalProcesados} archivos internos indexados ===");