using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WorkerServiceFiles.Helper
{
    public static class FileNameParser
    {
        private static readonly string[] PrefijosValidos =
        {
            "FEAC","FEAH","FEAL","FEAN","FEBO","FEBQ","FEBU",
            "FECA","FECS","FEDI","FEDO","FEEN","FEFA","FEFB",
            "FEFN","FEMA","FEMF","FEMI","FEMN","FEMO","FEMS",
            "FEPE","FEPQ","FEPR","FERI","FEUC"
        };

        public static (string? Prefijo, string? Numero) ExtraerFactura(string nombreArchivo)
        {
            var nombre = nombreArchivo.ToUpper();

            foreach (var prefijo in PrefijosValidos)
            {
                var regex = new Regex($@"{prefijo}[\s_]?(\d+)", RegexOptions.Compiled);

                var match = regex.Match(nombre);

                if (match.Success)
                {
                    return (prefijo, match.Groups[1].Value);
                }
            }

            return (null, null);
        }
    }
}
