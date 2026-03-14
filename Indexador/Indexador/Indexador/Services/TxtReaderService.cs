using System;
using System.Collections.Generic;
using System.Text;

namespace Indexador.Services
{
    public static class TxtReaderService
    {
        public static IEnumerable<string> LeerLineas(string ruta)
        {
            foreach (var linea in File.ReadLines(ruta))
            {
                if (!string.IsNullOrWhiteSpace(linea))
                    yield return linea.Trim();
            }
        }
    }
}
