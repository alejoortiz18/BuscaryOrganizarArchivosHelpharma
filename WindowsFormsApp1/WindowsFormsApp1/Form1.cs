using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BuscarArchivos
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void btnOrigen_Click_1(object sender, EventArgs e)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();

            if (f.ShowDialog() == DialogResult.OK)
                txtOrigen.Text = f.SelectedPath;
            var split = txtOrigen.Text.Split('\\');
            txtOrigen.Text = ($"\\\\192.168.0.69\\Informes\\{split[1]}");

        }

        private void btnDestino_Click_1(object sender, EventArgs e)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();

            if (f.ShowDialog() == DialogResult.OK)
                txtDestino.Text = f.SelectedPath;
            var split = txtDestino.Text.Split('\\');
            txtDestino.Text = ($"\\\\192.168.0.69\\Informes\\{split[1]}");
        }

        private void btnArchivo_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "Archivos txt|*.txt";

            if (o.ShowDialog() == DialogResult.OK)
                txtArchivo.Text = o.FileName;
        }

        private async void btnProcesar_Click_1(object sender, EventArgs e)
        {
            listLog.Items.Clear();

            string RUTA_ORIGEN = txtOrigen.Text;
            string RUTA_DESTINO = txtDestino.Text;
            string ARCHIVO_LISTA = txtArchivo.Text;

            Directory.CreateDirectory(RUTA_DESTINO);

            listLog.Items.Add("🚀 Iniciando proceso...");

            var documentos = File.ReadAllLines(ARCHIVO_LISTA)
                                 .Where(x => !string.IsNullOrWhiteSpace(x))
                                 .ToList();

            progressBar1.Maximum = documentos.Count;
            progressBar1.Value = 0;

            lblContador.Text = $"Documentos procesados: 0 / {documentos.Count}";

            await Task.Run(() =>
            {
                //---------------------------------
                // INDEXAR SERVIDOR
                //---------------------------------

                this.Invoke(new Action(() =>
                {
                    listLog.Items.Add("🔎 Indexando archivos del servidor...");
                }));

                Dictionary<string, List<string>> indiceArchivos =
                    new Dictionary<string, List<string>>();

                var archivos = Directory.GetFiles(RUTA_ORIGEN, "*.*", SearchOption.AllDirectories);

                int contadorIndex = 0;

                foreach (var archivo in archivos)
                {
                    string nombre = Path.GetFileName(archivo).ToLower();

                    if (!indiceArchivos.ContainsKey(nombre))
                        indiceArchivos[nombre] = new List<string>();

                    indiceArchivos[nombre].Add(archivo);

                    //---------------------------------
                    // INDEXAR ZIP
                    //---------------------------------

                    if (nombre.EndsWith(".zip"))
                    {
                        try
                        {
                            using (ZipArchive zip = ZipFile.OpenRead(archivo))
                            {
                                foreach (var entry in zip.Entries)
                                {
                                    string nombreZip = entry.Name.ToLower();

                                    if (!indiceArchivos.ContainsKey(nombreZip))
                                        indiceArchivos[nombreZip] = new List<string>();

                                    indiceArchivos[nombreZip].Add(archivo + "|" + entry.FullName);
                                }
                            }
                        }
                        catch { }
                    }

                    contadorIndex++;

                    if (contadorIndex % 5000 == 0)
                    {
                        this.Invoke(new Action(() =>
                        {
                            listLog.Items.Add($"Indexados: {contadorIndex}");
                        }));
                    }
                }

                this.Invoke(new Action(() =>
                {
                    listLog.Items.Add($"📚 Total archivos indexados: {contadorIndex}");
                }));

                //---------------------------------
                // PROCESAR DOCUMENTOS
                //---------------------------------

                int procesados = 0;
                int encontrados = 0;
                int noEncontrados = 0;
                List<string> listaNoEncontrados = new List<string>();

                foreach (var documento in documentos)
                {
                    this.Invoke(new Action(() =>
                    {
                        listLog.Items.Add($"📂 Buscando: {documento}");
                    }));

                    var coincidencias = indiceArchivos
                        .Where(x => x.Key.Contains(documento.ToLower()))
                        .SelectMany(x => x.Value)
                        .ToList();

                    if (coincidencias.Count == 0)
                    {
                        noEncontrados++;
                        listaNoEncontrados.Add(documento);
                        procesados++;

                        this.Invoke(new Action(() =>
                        {
                            listLog.Items.Add("❌ No encontrado");
                            listLog.Items.Add("--------------------------------");

                            progressBar1.Value = procesados;
                            lblContador.Text = $"Documentos procesados: {procesados} / {documentos.Count}";
                        }));

                        continue;
                    }

                    string jsonFile = null;
                    string cuvFile = null;
                    string pdfFile = null;
                    string zipFile = null;

                    foreach (var ruta in coincidencias)
                    {
                        string nombre;

                        if (ruta.Contains("|"))
                            nombre = ruta.Split('|')[1].ToLower();
                        else
                            nombre = Path.GetFileName(ruta).ToLower();

                        if (nombre.EndsWith(".json"))
                            jsonFile = ruta;

                        else if (nombre.Contains("cuv"))
                            cuvFile = ruta;

                        else if (nombre.EndsWith(".pdf"))
                            pdfFile = ruta;

                        else if (nombre.EndsWith(".zip"))
                            zipFile = ruta;
                    }

                    //---------------------------------
                    // CREAR CARPETA JSON
                    //---------------------------------

                    if (jsonFile != null)
                    {
                        string nombreJson =
                            jsonFile.Contains("|")
                            ? Path.GetFileNameWithoutExtension(jsonFile.Split('|')[1])
                            : Path.GetFileNameWithoutExtension(jsonFile);

                        string carpetaDestino = Path.Combine(RUTA_DESTINO, nombreJson);

                        Directory.CreateDirectory(carpetaDestino);

                        CopiarArchivo(jsonFile, carpetaDestino);
                        CopiarArchivo(cuvFile, carpetaDestino);
                    }

                    //---------------------------------
                    // COPIAR PDF / ZIP SUELTOS
                    //---------------------------------

                    CopiarArchivo(pdfFile, RUTA_DESTINO);
                    CopiarArchivo(zipFile, RUTA_DESTINO);

                    encontrados++;
                    procesados++;

                    this.Invoke(new Action(() =>
                    {
                        progressBar1.Value = procesados;
                        lblContador.Text = $"Documentos procesados: {procesados} / {documentos.Count}";

                        listLog.Items.Add($"✅ Procesado: {documento}");
                        listLog.Items.Add("--------------------------------");
                    }));
                }

                //---------------------------------
                // RESUMEN FINAL
                //---------------------------------

                this.Invoke(new Action(() =>
                {
                    listLog.Items.Add("");
                    listLog.Items.Add("========= RESUMEN =========");
                    listLog.Items.Add($"Documentos encontrados: {encontrados}");
                    listLog.Items.Add($"Documentos NO encontrados: {noEncontrados}");
                    listLog.Items.Add($"Total procesados: {procesados}");
                }));

                //---------------------------------
                // EXPORTAR NO ENCONTRADOS
                //---------------------------------

                if (listaNoEncontrados.Count > 0)
                {
                    try
                    {
                        string archivoSalida = Path.Combine(
                            RUTA_DESTINO,
                            $"NoEncontrados_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                        );

                        File.WriteAllLines(archivoSalida, listaNoEncontrados);

                        this.Invoke(new Action(() =>
                        {
                            listLog.Items.Add("");
                            listLog.Items.Add($"📄 Archivo de no encontrados generado:");
                            listLog.Items.Add(archivoSalida);
                        }));
                    }
                    catch { }
                }

            });

            listLog.Items.Add("🎉 Proceso terminado");
        }

        void CopiarArchivo(string ruta, string destinoCarpeta)
        {
            if (ruta == null) return;

            try
            {
                if (ruta.Contains("|"))
                {
                    var partes = ruta.Split('|');
                    string zipPath = partes[0];
                    string entryName = partes[1];

                    using (ZipArchive zip = ZipFile.OpenRead(zipPath))
                    {
                        var entry = zip.GetEntry(entryName);

                        if (entry != null)
                        {
                            string destino = Path.Combine(destinoCarpeta, entry.Name);
                            entry.ExtractToFile(destino, true);
                        }
                    }
                }
                else
                {
                    string destino = Path.Combine(destinoCarpeta, Path.GetFileName(ruta));
                    File.Copy(ruta, destino, true);
                }
            }
            catch { }
        }
    }
}
