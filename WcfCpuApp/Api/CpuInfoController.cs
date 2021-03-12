using Common.Entidades;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using WcfCpuApp.Hubs;
using WcfCpuApp.Models;

namespace WcfCpuApp.Api
{
    [RoutePrefix("Api/cpuInfo")]
    public class CpuInfoController : ApiController
    {
        #region Variables
        private static List<CpuInfoPostData> wListData = null;
        private static HubConnection hubConnection;
        private static IHubProxy hubProxy;
        private static List<PCDataBE> wListaDataServidores = null;
        private static string wJSON;
        #endregion

        #region Conexion SignalR
        private static void Conexion()
        {

            var serverUrlTest = new Uri("http://localhost:3005/signalr/hubs");
            hubConnection = new HubConnection(serverUrlTest.ToString());
            hubProxy = hubConnection.CreateHubProxy("CpuInfo");
            try
            {
                hubConnection.Start().Wait();
            }
            catch (Exception ex)
            {
                var msj = ex.Message;
            }

        }
        #endregion

        #region Captura de Datos
        [HttpPost]
        [Route("Post")]
        public void Post()
        {
            if (hubConnection == null)
                Conexion();
            var context = GlobalHost.ConnectionManager.GetHubContext<CpuInfo>();
            var body = new StreamReader(HttpContext.Current.Request.InputStream, Encoding.GetEncoding(1252)).ReadToEnd();
            CpuInfoPostData cpuInfo = JsonConvert.DeserializeObject<CpuInfoPostData>(body);

            if (wListData == null)
                wListData = new List<CpuInfoPostData>();
            if (wListData != null)
            {
                wListData.Add(cpuInfo);
            }

            context.Clients.All.cpuInfoMessage(cpuInfo.MachineName, cpuInfo.Processor, cpuInfo.MemUsage, cpuInfo.TotalMemory, cpuInfo.Services, cpuInfo.AddressIp, cpuInfo.Disk, cpuInfo.Sysos, cpuInfo.Processador, cpuInfo.filesVersion, cpuInfo.pais, cpuInfo.iisSites, cpuInfo.processNode, cpuInfo.descriptionServer);
        }

        [HttpPost]
        [Route("PostJSON")]
        public void PostJSON()
        {
            if (hubConnection == null)
                Conexion();
            var context = GlobalHost.ConnectionManager.GetHubContext<CpuInfo>();
            var body = new StreamReader(HttpContext.Current.Request.InputStream, Encoding.GetEncoding(1252)).ReadToEnd();
            var obj = JsonConvert.DeserializeObject<PCDataBE>(body);
            if (wListaDataServidores == null || wListaDataServidores.Count == 0)
            {
                wListaDataServidores = new List<PCDataBE>();
                wListaDataServidores.Add(obj);
            }
            else
            {
                wListaDataServidores.RemoveAll(x => x.MachineName == obj.MachineName);
                wListaDataServidores.Add(obj);
            }



            wJSON = body;
        }
        #endregion


        #region Eventos WebAPI
        #region Obtencion de Daots
        [HttpGet]
        [Route("getDataAPI")]
        public HttpResponseMessage getDataAPI()
        {
            if (hubConnection == null)
                Conexion();
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

        [HttpGet, HttpOptions, HttpPost]
        [Route("getData2")]
        public HttpResponseMessage getData2()
        {
            if (hubConnection == null)
                Conexion();
            if (Request.Method == HttpMethod.Options)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                };
            }

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(wJSON, System.Text.Encoding.UTF8, "application/json"),
            };
            return response;
        }

        [HttpGet, HttpOptions, HttpPost]
        [Route("getDataServidores")]
        public HttpResponseMessage getDataServidores()
        {
            if (hubConnection == null)
                Conexion();
            if (Request.Method == HttpMethod.Options)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                };
            }


            var json = JsonConvert.SerializeObject(wListaDataServidores);

            var resultResponse = JsonConvert.SerializeObject(wListaDataServidores,
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

            return response;
        }

        [HttpGet, HttpOptions, HttpPost]
        [Route("getDataServicios")]
        public HttpResponseMessage getDataServicios()
        {
            if (hubConnection == null)
                Conexion();
            if (Request.Method == HttpMethod.Options)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                };
            }
            List<ServiceBE> wListServices = new List<ServiceBE>();
            var list = wListaDataServidores;
            if (list != null)
            {
                foreach (var servidor in list)
                {
                    if (servidor.Services != null && servidor.Services.Count > 0)
                    {
                        foreach (var srv in servidor.Services)
                        {
                            wListServices.Add(srv);
                        }
                    }
                }
                var json = JsonConvert.SerializeObject(wListServices);

                var resultResponse = JsonConvert.SerializeObject(wListServices,
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

                return response;
            }
            else
            {
                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                };
                return response;
            }
        }

        [HttpOptions, HttpPost]
        [Route("getServiceDetail")]
        public HttpResponseMessage getServiceDetail()
        {
            if (hubConnection == null)
                Conexion();
            if (Request.Method == HttpMethod.Options)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                };
            }

            var body = new StreamReader(HttpContext.Current.Request.InputStream, Encoding.GetEncoding(1252)).ReadToEnd();
            ServiceBE service = new ServiceBE();
            service = JsonConvert.DeserializeObject<ServiceBE>(body);

            if (service == null)
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                };

            List<ServiceBE> wListServices = new List<ServiceBE>();
            var list = wListaDataServidores;
            if (list == null || list.Count == 0)
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                };

            var servicio = new ServiceBE();
            foreach (var servidor in list)
            {
                if (servidor.MachineName == service.machineName)
                {
                    if (servidor.Services != null && servidor.Services.Count > 0)
                    {
                        foreach (var srv in servidor.Services)
                        {
                            if (srv.serviceDisplayName == service.serviceDisplayName)
                            {
                                servicio = srv;
                            }
                        }
                    }
                }
            }


            var json = JsonConvert.SerializeObject(servicio);

            var resultResponse = JsonConvert.SerializeObject(servicio,
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

            return response;
        }
        #endregion

        #region Envio de Datos a Servidores
        [HttpOptions, HttpPost]
        [Route("sendEventService")]
        public HttpResponseMessage sendEventService()
        {
            if (hubConnection == null || hubConnection.State == ConnectionState.Disconnected)
                Conexion();
            if (Request.Method == HttpMethod.Options)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                };
            }

            var body = new StreamReader(HttpContext.Current.Request.InputStream, Encoding.GetEncoding(1252)).ReadToEnd();
            ServiceBE service = new ServiceBE();
            service = JsonConvert.DeserializeObject<ServiceBE>(body);

            if (service == null)
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                };

            List<ServiceBE> wListServices = new List<ServiceBE>();
            var list = wListaDataServidores;
            if (list == null || list.Count == 0)
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                };

            ServiceBE servicio = null;
            foreach (var servidor in list)
            {
                if (servidor.MachineName == service.machineName)
                {
                    if (servidor.Services != null && servidor.Services.Count > 0)
                    {
                        foreach (var srv in servidor.Services)
                        {
                            if (srv.serviceName == service.serviceName)
                            {
                                servicio = new ServiceBE();
                                servicio = srv;
                                break;
                            }
                        }
                    }
                }
            }

            if (servicio != null)
            {
                if (service.actions == "START")
                    hubProxy.Invoke("SendMessageStart", servicio.machineName, servicio.serviceName);
                if (service.actions == "STOP")
                    hubProxy.Invoke("SendMessageStop", servicio.machineName, servicio.serviceName);
                if (service.actions == "RESTART")
                    hubProxy.Invoke("SendMessageRestart", servicio.machineName, servicio.serviceName);
            }


            var json = JsonConvert.SerializeObject(servicio);

            var resultResponse = JsonConvert.SerializeObject(servicio,
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

            return response;
        }

        //Para HACER
        [HttpOptions, HttpPost]
        [Route("sendEventIIS")]
        public HttpResponseMessage sendEventIIS()
        {
            if (hubConnection == null || hubConnection.State == ConnectionState.Disconnected)
                Conexion();
            if (Request.Method == HttpMethod.Options)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                };
            }

            var body = new StreamReader(HttpContext.Current.Request.InputStream, Encoding.GetEncoding(1252)).ReadToEnd();
            ServiceBE service = new ServiceBE();
            service = JsonConvert.DeserializeObject<ServiceBE>(body);

            if (service == null)
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                };

            List<ServiceBE> wListServices = new List<ServiceBE>();
            var list = wListaDataServidores;
            if (list == null || list.Count == 0)
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                };

            ServiceBE servicio = null;
            foreach (var servidor in list)
            {
                if (servidor.MachineName == service.machineName)
                {
                    if (servidor.Services != null && servidor.Services.Count > 0)
                    {
                        foreach (var srv in servidor.Services)
                        {
                            if (srv.serviceName == service.serviceName)
                            {
                                servicio = new ServiceBE();
                                servicio = srv;
                                break;
                            }
                        }
                    }
                }
            }

            if (servicio != null)
            {
                if (service.actions == "START")
                    hubProxy.Invoke("SendMessageStart", servicio.machineName, servicio.serviceName);
                if (service.actions == "STOP")
                    hubProxy.Invoke("SendMessageStop", servicio.machineName, servicio.serviceName);
                if (service.actions == "RESTART")
                    hubProxy.Invoke("SendMessageRestart", servicio.machineName, servicio.serviceName);
            }


            var json = JsonConvert.SerializeObject(servicio);

            var resultResponse = JsonConvert.SerializeObject(servicio,
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

            return response;
        }
        #endregion
        #endregion






    }
}
