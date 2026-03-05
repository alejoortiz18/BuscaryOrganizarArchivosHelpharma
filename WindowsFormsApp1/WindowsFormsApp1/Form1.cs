using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
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
            txtOrigen.Text = txtOrigen.Text.Replace("I:\\", "\\\\192.168.0.69\\Informes\\");

        }

        private void btnDestino_Click_1(object sender, EventArgs e)
        {
            FolderBrowserDialog f = new FolderBrowserDialog();

            if (f.ShowDialog() == DialogResult.OK)
                txtDestino.Text = f.SelectedPath;
            txtDestino.Text = txtDestino.Text.Replace("I:\\", "\\\\192.168.0.69\\Informes\\");
        }

        private void btnArchivo_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "Archivos txt|*.txt";

            if (o.ShowDialog() == DialogResult.OK)
                txtArchivo.Text = o.FileName;
        }

        private void btnProcesar_Click_1(object sender, EventArgs e)
        {
            listLog.Items.Clear();
            string RUTA_ORIGEN = txtOrigen.Text;
            string RUTA_DESTINO = txtDestino.Text;
            string ARCHIVO_LISTA = txtArchivo.Text;
            try
            {
                Directory.CreateDirectory(RUTA_DESTINO);

                listLog.Items.Add("🚀 Iniciando proceso...");

                var documentos = File.ReadAllLines(ARCHIVO_LISTA)
                                     .Where(x => !string.IsNullOrWhiteSpace(x))
                                     .ToList();

                listLog.Items.Add($"Documentos a buscar: {documentos.Count}");

                listLog.Items.Add("🔎 Indexando servidor...");

                List<string> archivosServidor =
                    Directory.GetFiles(RUTA_ORIGEN, "*.*", SearchOption.AllDirectories).ToList();

                listLog.Items.Add($"Total archivos encontrados: {archivosServidor.Count}");

                foreach (var documento in documentos)
                {
                    listLog.Items.Add($"Procesando: {documento}");

                    var coincidencias = archivosServidor
                        .Where(x => Path.GetFileName(x).Contains(documento))
                        .ToList();

                    if (coincidencias.Count == 0)
                    {
                        listLog.Items.Add("   ❌ No encontrado");
                        continue;
                    }

                    string jsonFile = null;
                    string cuvFile = null;
                    List<string> otros = new List<string>();

                    foreach (var ruta in coincidencias)
                    {
                        var nombre = Path.GetFileName(ruta).ToLower();

                        if (nombre.EndsWith(".json"))
                            jsonFile = ruta;
                        else if (nombre.Contains("cuv"))
                            cuvFile = ruta;
                        else
                            otros.Add(ruta);
                    }

                    if (jsonFile == null)
                    {
                        listLog.Items.Add("   ⚠️ No se encontró JSON");
                        continue;
                    }

                    string nombreCarpeta = Path.GetFileNameWithoutExtension(jsonFile);
                    string carpetaDestino = Path.Combine(RUTA_DESTINO, nombreCarpeta);

                    Directory.CreateDirectory(carpetaDestino);

                    foreach (var ruta in new[] { jsonFile, cuvFile })
                    {
                        if (ruta != null)
                        {
                            string destino = Path.Combine(carpetaDestino, Path.GetFileName(ruta));
                            File.Copy(ruta, destino, true);
                        }
                    }

                    foreach (var ruta in otros)
                    {
                        string nombre = Path.GetFileName(ruta).ToLower();

                        if (nombre.EndsWith(".pdf") || nombre.EndsWith(".zip"))
                        {
                            string destino = Path.Combine(RUTA_DESTINO, Path.GetFileName(ruta));
                            File.Copy(ruta, destino, true);
                        }
                    }
                }

                listLog.Items.Add("🎉 Proceso terminado");
            }
            catch (Exception ex)
            {
                listLog.Items.Add($"⚠️ {ex.Message}");
                
            }
            
        }
    }
}
