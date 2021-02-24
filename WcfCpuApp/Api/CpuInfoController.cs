using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using WcfCpuApp.Hubs;
using WcfCpuApp.Models;

namespace WcfCpuApp.Api
{
    [RoutePrefix("Api/cpuInfo")]
    public class CpuInfoController : ApiController
    {
        private static List<CpuInfoPostData> wListData = null;
        [HttpPost]
        [Route("Post")]
        public void Post()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<CpuInfo>();
            var body = new StreamReader(HttpContext.Current.Request.InputStream, Encoding.GetEncoding(1252)).ReadToEnd();
            CpuInfoPostData cpuInfo = JsonConvert.DeserializeObject<CpuInfoPostData>(body);

            if (wListData == null)
                wListData = new List<CpuInfoPostData>();
            if(wListData!= null)
            {
                wListData.Add(cpuInfo);
            }

            context.Clients.All.cpuInfoMessage(cpuInfo.MachineName, cpuInfo.Processor, cpuInfo.MemUsage, cpuInfo.TotalMemory, cpuInfo.Services, cpuInfo.AddressIp, cpuInfo.Disk, cpuInfo.Sysos, cpuInfo.Processador, cpuInfo.filesVersion, cpuInfo.pais, cpuInfo.iisSites, cpuInfo.processNode, cpuInfo.descriptionServer);
        }

        [HttpGet]
        [Route("getDataAPI")]
        public HttpResponseMessage getDataAPI()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<CpuInfo>();
            var json = JsonConvert.SerializeObject(wListData);

            var resultResponse = JsonConvert.SerializeObject(wListData,
                            Formatting.Indented,
                            new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                            });

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(resultResponse, System.Text.Encoding.UTF8, "application/json"),
            };

            //return Request.CreateResponse(HttpStatusCode.Created, wListData);
            return response;
            //return string.Empty;
        }

        [HttpGet]
        [Route("getData2")]
        public string getData2()
        {
            var a = 1;
            return "data";
        }
    }
}
