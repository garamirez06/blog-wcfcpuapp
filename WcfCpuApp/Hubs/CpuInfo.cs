using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;

namespace WcfCpuApp.Hubs
{
    public class CpuInfo : Hub
    {
        #region Data Members
        static List<UserDetail> ConnectedUsers = new List<UserDetail>();
        static List<MessageDetail> CurrentMessage = new List<MessageDetail>();
        #endregion

        #region Recepcion de Informacion
        public void SendCpuInfo(string machineName, double processor, int memUsage, int totalMemory, string services, string addressIP, string disk, string sysos, string processador, string filesVersion,string pais, string iisSites, string processNode)
        {
            this.Clients.All.cpuInfoMessage(machineName, processor, memUsage, totalMemory, services, addressIP, disk, sysos, processador, filesVersion, pais, iisSites, processNode);
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

        public void SendMessageStart(string idConnection, String wServiceName)
        {
            Clients.Client(idConnection).sendMessageStart(wServiceName);
        }
        #endregion
    }


}