﻿
namespace WcfCpuApp.Models
{
    public class CpuInfoPostData
    {
        public string MachineName { get; set; }
        public double Processor { get; set; }
        public ulong MemUsage { get; set; }
        public ulong TotalMemory { get; set; }
        public string Services { get; set; }
        public string AddressIp { get; set; }
        public string Disk { get; set; }
        public string Sysos { get; set; }
        public string Processador { get; set; }
        public string filesVersion { get; set; }
    }
}