using System.IO;
using System.Text;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using WcfCpuApp.Hubs;
using WcfCpuApp.Models;

namespace WcfCpuApp.Api
{
    public class CpuInfoController : ApiController
    {
        public void Post()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<CpuInfo>();
            var body = new StreamReader(HttpContext.Current.Request.InputStream, Encoding.GetEncoding(1252)).ReadToEnd();
            CpuInfoPostData cpuInfo = JsonConvert.DeserializeObject<CpuInfoPostData>(body);



            context.Clients.All.cpuInfoMessage(cpuInfo.MachineName, cpuInfo.Processor, cpuInfo.MemUsage, cpuInfo.TotalMemory, cpuInfo.Services, cpuInfo.AddressIp, cpuInfo.Disk, cpuInfo.Sysos, cpuInfo.Processador, cpuInfo.filesVersion, cpuInfo.pais);
        }

        [HttpGet]
        public string getData()
        {
            var a = 1;
            return "data";
        }
    }
}
