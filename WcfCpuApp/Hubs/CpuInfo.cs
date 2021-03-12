using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace WcfCpuApp.Hubs
{
    public class CpuInfo : Hub
    {
        #region Data Members
        static List<UserDetail> ConnectedUsers = new List<UserDetail>();
        static List<MessageDetail> CurrentMessage = new List<MessageDetail>();
        static List<CurrentStatus> CurrentData = new List<CurrentStatus>();
        #endregion

        #region Recepcion de Informacion
        public void RetrieveDataStatic()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<CpuInfo>();

            if (CurrentData.Count > 0)
            {
                foreach (var cpuInfo in CurrentData)
                {
                    context.Clients.All.cpuInfoMessage(cpuInfo.MachineName, cpuInfo.Processor, cpuInfo.MemUsage, cpuInfo.TotalMemory, cpuInfo.Services, cpuInfo.AddressIP, cpuInfo.Disk, cpuInfo.Sysos, cpuInfo.Processador, cpuInfo.FilesVersion, cpuInfo.Pais, cpuInfo.IisSites, cpuInfo.ProcessNode, cpuInfo.DescriptionServer);
                }
            }
        }
        public int GetDataStatic(string machineName, double processor, int memUsage, int totalMemory, string services, string addressIP, string disk, string sysos, string processador, string filesVersion, string pais, string iisSites, string processNode, string descriptionServer)
        {
            //this.Clients.All.cpuInfoMessage(machineName, processor, memUsage, totalMemory, services, addressIP, disk, sysos, processador, filesVersion, pais, iisSites, processNode, descriptionServer);
            //vemos si funca
            var item = CurrentData.FirstOrDefault(x => x.MachineName == machineName);
            if (item != null)
            {
                CurrentData.Remove(item);
                CurrentData.Add(new CurrentStatus
                {
                    MachineName = machineName,
                    Processor = processor,
                    MemUsage = memUsage,
                    TotalMemory = totalMemory,
                    Services = services,
                    AddressIP = addressIP,
                    Disk = disk,
                    Sysos = sysos,
                    Processador = processador,
                    FilesVersion = filesVersion,
                    Pais = pais,
                    IisSites = iisSites,
                    ProcessNode = processNode,
                    DescriptionServer = descriptionServer
                });
            }
            else
            {
                CurrentData.Add(new CurrentStatus
                {
                    MachineName = machineName,
                    Processor = processor,
                    MemUsage = memUsage,
                    TotalMemory = totalMemory,
                    Services = services,
                    AddressIP = addressIP,
                    Disk = disk,
                    Sysos = sysos,
                    Processador = processador,
                    FilesVersion = filesVersion,
                    Pais = pais,
                    IisSites = iisSites,
                    ProcessNode = processNode,
                    DescriptionServer = descriptionServer
                });
            }
            return CurrentData.Count;
        }
        #endregion

        #region Eventos Basicos
        public override Task OnConnected()
        {
            var id = Context.ConnectionId;
            var name = string.Empty;
            if (Context.Request.Headers.GetValues("HostClient") != null)
            {
                name = Context.Request.Headers.GetValues("HostClient").First().ToString();
            }

            if ((!string.IsNullOrEmpty(name)) && ConnectedUsers.Count(x => x.ConnectionId == id) == 0)
            {
                ConnectedUsers.Add(new UserDetail { ConnectionId = id, MachineName = name });

                //send to caller
                Clients.Caller.onConnected(id, name, ConnectedUsers);

                // send to all except caller client
                Clients.AllExcept(id).onNewUserConnected(id, name);

            }
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var item = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null)
            {
                ConnectedUsers.Remove(item);

                var id = Context.ConnectionId;
                this.Clients.All.onUserDisconnected(id, item.MachineName);

                var item2 = CurrentData.FirstOrDefault(x => x.MachineName == item.MachineName);
                if (item2 != null)
                {
                    CurrentData.Remove(item2);
                }
            }


            return base.OnDisconnected(stopCalled);
        }
        #endregion

        #region Eventos Conexion
        public void Connect()
        {
            var id = Context.ConnectionId;
            var name = string.Empty;
            if (Context.Request.Headers.GetValues("HostClient") != null)
            {
                name = Context.Request.Headers.GetValues("HostClient").ToString();
            }

            if ((!string.IsNullOrEmpty(name)) && ConnectedUsers.Count(x => x.ConnectionId == id) == 0)
            {
                ConnectedUsers.Add(new UserDetail { ConnectionId = id, MachineName = name });

                // send to caller
                //Clients.Caller.onConnected(id, userName, ConnectedUsers, CurrentMessage);

                // send to all except caller client
                //Clients.AllExcept(id).onNewUserConnected(id, userName);
                SendMessageWelcome(id, name);
            }

        }
        #endregion

        #region Envio de Mensajes
        public void SendMessageWelcome(string idConnection, string wName)
        {
            Clients.Client(idConnection).sendMessage("Bienvenido " + wName);
        }

        public void SendMessage(string idConnection)
        {
            Clients.Client(idConnection).sendMessage("Send Message " + DateTime.Now.ToString());
        }

        public void SendMessageStart(string pMachineName, String pServiceName)
        {
            String idConnection;
            var item = ConnectedUsers.FirstOrDefault(x => x.MachineName == pMachineName);
            if (item != null)
            {
                idConnection = item.ConnectionId;
                Clients.Client(idConnection).SendMessageStart(pServiceName);
            }

        }

        public void SendMessageStop(string pMachineName, String pServiceName)
        {
            String idConnection;
            var item = ConnectedUsers.FirstOrDefault(x => x.MachineName == pMachineName);
            if (item != null)
            {
                idConnection = item.ConnectionId;
                Clients.Client(idConnection).SendMessageStop(pServiceName);
            }

        }

        public void SendMessageRestart(string pMachineName, String pServiceName)
        {
            String idConnection;
            var item = ConnectedUsers.FirstOrDefault(x => x.MachineName == pMachineName);
            if (item != null)
            {
                idConnection = item.ConnectionId;
                Clients.Client(idConnection).SendMessageRestart(pServiceName);
            }

        }


        public string getUserConnected()
        {
            return "Hola MUNDO!";
        }
        #endregion
    }


}