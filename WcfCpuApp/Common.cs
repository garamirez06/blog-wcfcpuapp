using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfCpuApp
{
    public class Common
    {
    }

    public class UserDetail
    {
        public string ConnectionId { get; set; }
        public string MachineName { get; set; }
    }

    public class MessageDetail
    {

        public string UserName { get; set; }

        public string Message { get; set; }

    }

    public class CurrentStatus {
        public string MachineName { get; set; }
        public double Processor { get; set; }
        public int MemUsage { get; set; }
        public int TotalMemory { get; set; }
        public string Services { get; set; }
        public string AddressIP { get; set; }
        public string Disk { get; set; }
        public string Sysos { get; set; }
        public string Processador { get; set; }
        public string FilesVersion { get; set; }
        public string Pais { get; set; }
        public string IisSites { get; set; }
        public string ProcessNode { get; set; }
        public string DescriptionServer { get; set; }
    }


}