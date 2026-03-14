using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Indexador.Parsers
{
    public static class FacturaParser
    {
        static readonly string[] PrefijosValidos =
        {
            "FEAC","FEAH","FEAL","FEAN","FEBO","FEBQ","FEBU","FECA",
            "FECS","FEDI","FEDO","FEEN","FEFA","FEFB","FEFN","FEMA",
            "FEMF","FEMI","FEMN","FEMO","FEMS","FEPE","FEPQ","FEPR",
            "FERI","FEUC"
        };

        static readonly Regex RegexFactura =
            new(@"(FEAC|FEAH|FEAL|FEAN|FEBO|FEBQ|FEBU|FECA|FECS|FEDI|FEDO|FEEN|FEFA|FEFB|FEFN|FEMA|FEMF|FEMI|FEMN|FEMO|FEMS|FEPE|FEPQ|FEPR|FERI|FEUC)[\s\-_]?(\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static (string? prefijo, string? numero) Extraer(string nombreArchivo)
        {
            var match = RegexFactura.Match(nombreArchivo);

            if (!match.Success)
                return (null, null);

            var prefijo = match.Groups[1].Value.ToUpper();
            var numero = match.Groups[2].Value;

            return (prefijo, numero);
        }
    }
}
