using Microsoft.AspNet.SignalR;

namespace WcfCpuApp.Hubs
{
    public class CpuInfo : Hub
    {
        public void SendCpuInfo(string machineName, double processor, int memUsage, int totalMemory, string services, string addressIP, string disk, string sysos, string processador, string filesVersion)
        {
            this.Clients.All.cpuInfoMessage(machineName, processor, memUsage, totalMemory, services, addressIP, disk, sysos, processador, filesVersion);
        }


    }
}