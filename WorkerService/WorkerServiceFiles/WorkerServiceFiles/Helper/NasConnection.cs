using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WorkerServiceFiles.Helper
{
    public class NasConnection : IDisposable
    {
        private readonly string _networkName;

        public NasConnection(string networkName, NetworkCredential credentials)
        {
            _networkName = networkName;

            var netResource = new NetResource
            {
                lpRemoteName = networkName
            };

            // 🔥 CERRAR CONEXIÓN PREVIA (CLAVE)
            WNetCancelConnection2(networkName, 0, true);

            var result = WNetAddConnection2(
                netResource,
                credentials.Password,
                credentials.UserName,
                0);

            if (result != 0)
            {
                throw new Exception($"Error connecting to NAS: {result}");
            }
        }

        public void Dispose()
        {
            WNetCancelConnection2(_networkName, 0, true);
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(
            NetResource netResource,
            string password,
            string username,
            int flags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(
            string name,
            int flags,
            bool force);

        [StructLayout(LayoutKind.Sequential)]
        private class NetResource
        {
            public int dwScope = 2;
            public int dwType = 1;
            public int dwDisplayType = 3;
            public int dwUsage = 1;
            public string? lpLocalName = null;
            public string? lpRemoteName = null;
            public string? lpComment = null;
            public string? lpProvider = null;
        }

        private enum ResourceScope
        {
            GlobalNetwork
        }

        private enum ResourceType
        {
            Disk
        }

        private enum ResourceDisplaytype
        {
            Share
        }
    }
}
