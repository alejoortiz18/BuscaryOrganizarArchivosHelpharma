using Indexador.Data;
using Indexador.Services;
using Indexador.Models;
using Indexador.Parsers;

var rutaTxt = @"C:\Users\serviciosrelease\Documents\Reportes\IndexacionNAS\Consolidados\consolidados.txt";

var repo = new SqlRepository();

Console.WriteLine("=== INICIANDO INDEXACIÓN ===");

var lineas = TxtReaderService.LeerLineas(rutaTxt);
var archivos = ConsolidadosParser.Parsear(lineas);

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

        // 🔥 MERGE cada 100k (5 * 20k)
        if (contadorLotes % 5 == 0)
        {
            Console.WriteLine("🔄 Ejecutando merge...");
            await repo.EjecutarMergeAsync();
        }

        Console.WriteLine($"Insertados: {contador}");
    }
}

// 🔚 último lote
if (lote.Count > 0)
{
    await repo.BulkInsertAsync(lote);
}

// 🔥 MERGE FINAL
Console.WriteLine("🔄 Ejecutando merge final...");
await repo.EjecutarMergeAsync();

Console.WriteLine($"=== INDEXACIÓN TERMINADA ===");
Console.WriteLine($"TOTAL: {contador}");