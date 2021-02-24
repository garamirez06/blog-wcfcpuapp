using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CpuInfoClient.BE
{
    public class PCDataBE
    {
        string connectionID;
        string machineName;
        string pais;
        double processorUsage;
        ulong ramUsage;
        ulong totalMemoryRAM;
        List<ServiceBE> services;
        List<string> addressIp;
        List<HardDiskBE> disk;
        string sysos;
        string processador;
        List<SitesIISBE> iisSites;
        List<ProcessBE> processNode;
        string descriptionServer;

        public string MachineName { get => machineName; set => machineName = value; }
        public string Pais { get => pais; set => pais = value; }
        public double ProcessorUsage { get => processorUsage; set => processorUsage = value; }
        public ulong RamUsage { get => ramUsage; set => ramUsage = value; }
        public ulong TotalMemoryRAM { get => totalMemoryRAM; set => totalMemoryRAM = value; }
        public List<string> AddressIp { get => addressIp; set => addressIp = value; }
        public List<HardDiskBE> Disk { get => disk; set => disk = value; }
        public string Sysos { get => sysos; set => sysos = value; }
        public string Processador { get => processador; set => processador = value; }
        public List<SitesIISBE> IisSites { get => iisSites; set => iisSites = value; }
        public List<ProcessBE> ProcessNode { get => processNode; set => processNode = value; }
        public string DescriptionServer { get => descriptionServer; set => descriptionServer = value; }
        public List<ServiceBE> Services { get => services; set => services = value; }
        public string ConnectionID { get => connectionID; set => connectionID = value; }
    }
}
