using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CpuInfoClient
{
    public class Entities { }
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
        //Campos a Revisar
        public string connectionID { get; set; }
        public string pais { get; set; }
    }


    public class VersionChatBE
    {
        public string version { get; set; }
    }

    public class SitesIISBE
    {
        public string machineName { get; set; }
        public string pais { get; set; }
        public long siteID { get; set; }
        public string siteName { get; set; }
        public string siteBinding { get; set; }
        public string siteState { get; set; }
        public string sitePath { get; set; }
    }


    public class ProcessBE
    {
        public string machineName { get; set; }
        public string pais { get; set; }
        public string processID { get; set; }
        public string processName { get; set; }
        public string instanceName { get; set; }
        public string commandLine { get; set; }
    }
}
