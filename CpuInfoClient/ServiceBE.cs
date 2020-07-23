using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpuInfoClient
{
    public class ServiceBE
    {
        public string machineName { get; set; }
        public string serviceName { get; set; }
        public string serviceDisplayName { get; set; }
        public string serviceType { get; set; }
        public string status { get; set; }
        public string startType { get; set; }

        public string Path { get; set; }
        public string serviceVersion { get; set; }
        public string filesVersion { get; set; }
    }
}
