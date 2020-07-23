using System.Web.Http;
using Microsoft.AspNet.SignalR;
using WcfCpuApp.Hubs;
using WcfCpuApp.Models;

namespace WcfCpuApp.Api
{
    public class ServiceInfoController : ApiController
    {
        public void Post(ServiceInfoPostData serviceInfo)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ServiceInfo>();
            context.Clients.All.serviceInfoMessage(serviceInfo.MachineName, serviceInfo.serviceName, serviceInfo.serviceDisplayName, serviceInfo.serviceType, serviceInfo.status, serviceInfo.startType, serviceInfo.path,serviceInfo.version);
        }
    }
}
