using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfCpuApp.Hubs
{
    public class ServiceInfo : Hub
    {
        public void SendServicesInfo(string machineName, string serviceName, string serviceDisplayName, string serviceType, string status, string startType, string path, string version)
        {
            this.Clients.All.serviceInfoMessage(machineName, serviceName, serviceDisplayName, serviceType, status, startType, path, version);
        }
    }
}