using Indexador.Data;
using Indexador.Services;
using Indexador.Models;
using Indexador.Parsers;

var rutaTxt = @"C:\Users\alejandro.ortiz\Documents\helpharma\Documentos\indexado masivo\Consolidados\consolidados.txt";

var repo = new SqlRepository();

Console.WriteLine("=== INICIANDO INDEXACIÓN ===");

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

        Console.WriteLine($"Insertados: {contador}");
    }
}

if (lote.Count > 0)
{
    await repo.BulkInsertAsync(lote);
    await repo.EjecutarMergeAsync();
}

Console.WriteLine($"=== INDEXACIÓN TERMINADA ===");
Console.WriteLine($"TOTAL ARCHIVOS INDEXADOS: {contador}");