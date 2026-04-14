using Indexador.Data;
using Indexador.Models;
using Indexador.Services;

var rutaBase = @"\\192.168.0.69\Informes";

var repo = new SqlRepository();

Console.WriteLine("=== INICIANDO INDEXACIÓN ===");

// ⏱️ DEFINIR HORA LÍMITE (5 AM)
var horaLimite = DateTime.Today.AddHours(5);

if (DateTime.Now > horaLimite)
{
    horaLimite = horaLimite.AddDays(1);
}

// 🔥 OBTENER ESTADO
var ultimaEjecucion = await repo.ObtenerUltimaEjecucionAsync();
var ultimaRuta = await repo.ObtenerUltimaRutaAsync();

Console.WriteLine($"Última ejecución: {ultimaEjecucion}");
Console.WriteLine($"Última ruta: {ultimaRuta}");

// 🔥 SCAN CON PROGRESO
var rutas = FileScannerService.EscanearArchivos(rutaBase, ultimaRuta, ultimaEjecucion);
var archivos = FileParserService.ParsearArchivos(rutas);

var lote = new List<ArchivoModel>(20000);

long contador = 0;
int contadorLotes = 0;

string? ultimaRutaProcesada = ultimaRuta;

foreach (var archivo in archivos)
{
    // ⏱️ CORTE AUTOMÁTICO A LAS 5 AM
    if (DateTime.Now >= horaLimite)
    {
        Console.WriteLine("⏰ Hora límite alcanzada, deteniendo proceso...");
        break;
    }

    lote.Add(archivo);
    contador++;

    // 💾 Guardar progreso en memoria
    ultimaRutaProcesada = archivo.RutaCompleta;

    if (lote.Count >= 20000)
    {
        await repo.BulkInsertAsync(lote);
        lote.Clear();

        contadorLotes++;

        // 💾 Guardar progreso en BD
        if (!string.IsNullOrEmpty(ultimaRutaProcesada))
        {
            await repo.GuardarUltimaRutaAsync(ultimaRutaProcesada);
        }

        // 🔄 Merge cada 5 lotes
        if (contadorLotes % 5 == 0)
        {
            Console.WriteLine("🔄 Ejecutando merge...");
            await repo.EjecutarMergeSPAsync();
        }

        Console.WriteLine($"Procesados: {contador}");
    }
}

// 🔚 Insertar último lote si quedó algo
if (lote.Count > 0)
{
    await repo.BulkInsertAsync(lote);
}

// 💾 Guardar progreso final
if (!string.IsNullOrEmpty(ultimaRutaProcesada))
{
    await repo.GuardarUltimaRutaAsync(ultimaRutaProcesada);
}

// 🔄 Merge final
Console.WriteLine("🔄 Merge final...");
await repo.EjecutarMergeSPAsync();

// 🕒 Actualizar última ejecución
await repo.ActualizarUltimaEjecucionAsync(DateTime.Now);

Console.WriteLine($"=== TERMINADO === {contador}");