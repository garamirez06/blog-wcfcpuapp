using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Configuration;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace CpuInfoClient
{
    class Program
    {
        static bool _running = true;
        static PerformanceCounter _cpuCounter, _memUsageCounter;
        static string wConnectionID = string.Empty;


        private static bool startService(string wNameService)
        {
            bool wRes = false;
            foreach (var wService in ServiceController.GetServices().OrderBy(p => p.DisplayName))
            {
                if (wService.ServiceName.ToLower().Equals(wNameService.ToLower()))
                {
                    wService.Start();
                    wRes = true;
                }

            }
            return wRes;
        }
        static void Main(string[] args)
        {
            Thread pollingThread = null;

            // Hello!
            Console.WriteLine("CPU Info Client: Reporting your CPU usage today!");

            try
            {
                //Creamos hilo de Escucha
                HubConnection hubConnection;
                IHubProxy hubProxy;
                var serverUrlTest = new Uri(ConfigurationManager.AppSettings["ServerUrlBase"] + "signalr/hubs");

                hubConnection = new HubConnection(serverUrlTest.ToString());
                hubConnection.Headers.Add("HostClient", Environment.MachineName);
                hubProxy = hubConnection.CreateHubProxy("CpuInfo");
                hubProxy.On<string>("Connect", (message) => Console.WriteLine(message));
                hubProxy.On<string>("SendMessageWelcome", (message) => Console.WriteLine(message));
                hubProxy.On<string>("SendMessage", (message) => Console.WriteLine(message));
                hubProxy.On<string>("SendMessageStart", (message) => startService(message));
                hubConnection.Start().Wait();
                wConnectionID = hubConnection.ConnectionId;
                Console.WriteLine("Cliente: " + wConnectionID);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            try
            {
                
                // Sets the culture to French (France)
                //Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
                // Sets the UI culture to French (France)
                //Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");
                //en-US
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                Console.WriteLine("1");
                _cpuCounter = new PerformanceCounter();
                _cpuCounter.CategoryName = "Processor";
                _cpuCounter.CounterName = "% Processor Time";
                _cpuCounter.InstanceName = "_Total";
                Console.WriteLine("2");
                _memUsageCounter = new PerformanceCounter("Memory", "Available KBytes");
                // Create a new thread to start polling and sending the data
                Console.WriteLine("3");
                pollingThread = new Thread(new ParameterizedThreadStart(RunPollingThread));
                pollingThread.Start();

                Console.WriteLine("Press a key to stop and exit");
                Console.ReadKey();

                Console.WriteLine("Stopping thread..");

                _running = false;

                pollingThread.Join(5000);

            }
            catch (Exception)
            {
                pollingThread.Abort();

                throw;
            }
        }

        static void RunPollingThread(object data)
        {
            Console.WriteLine("4");
            // Convert the object that was passed in
            DateTime lastPollTime = DateTime.MinValue;

            Console.WriteLine("Started polling...");

            // Start the polling loop
            while (_running)
            {
                // Poll every second
                if ((DateTime.Now - lastPollTime).TotalMilliseconds >= 1000)
                {
                    double cpuTime;
                    ulong memUsage, totalMemory;
                    Console.WriteLine("5");
                    // Get the stuff we need to send
                    GetMetrics(out cpuTime, out memUsage, out totalMemory);
                    #region Direcciones IP
                    IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
                    StringBuilder wListIP = new StringBuilder();

                    if (ipv4Addresses.Length > 0)
                    {
                        for (int i = 0; i < ipv4Addresses.Length; i++)
                        {
                            wListIP.Append(ipv4Addresses[i].ToString());
                            if ((i + 1) < ipv4Addresses.Length)
                            {
                                wListIP.Append(", ");
                            }
                        }
                    }
                    Console.WriteLine("6");
                    #endregion
                    #region Servicios
                    Console.WriteLine("7");
                    var services = JsonConvert.SerializeObject(GetAllServices());
                    Console.WriteLine("8");
                    #endregion
                    #region Disco Duros
                    /*
                     DriveType
                    CDRom	5	The drive is an optical disc device, such as a CD or DVD-ROM.
                    Fixed	3	The drive is a fixed disk.
                    Network	4	The drive is a network drive.
                    NoRootDirectory	1	The drive does not have a root directory.
                    Ram	6	The drive is a RAM disk.
                    Removable	2	The drive is a removable storage device, such as a USB flash drive.
                    Unknown	0	The type of drive is unknown.
                    */

                    DriveInfo[] allDrives = DriveInfo.GetDrives();
                    StringBuilder wListDisk = new StringBuilder();
                    if (DriveInfo.GetDrives().Length > 0)
                    {

                        wListDisk.Append("<button type=\"button\" class=\"btn btn-info collapsible\"  onclick=\"collapse()\">+</button>");
                        wListDisk.Append("<div id=\"collapseDisk\" class=\"content\">");
                        wListDisk.Append("<ul class=\"list-group\">");
                    }

                    foreach (DriveInfo d in allDrives)
                    {
                        //Excluimos los discos
                        if (d.DriveType != DriveType.Fixed)
                            continue;
                        if (d.IsReady == true)
                        {
                            wListDisk.Append("<li class=\"list-group-item\">");
                            wListDisk.Append(string.Format("{0}", d.Name));
                            wListDisk.Append(" - ");
                            wListDisk.Append(string.Format("Etiqueta de Volumen: {0}", d.VolumeLabel));
                            wListDisk.Append(" - ");
                            wListDisk.Append(string.Format("Sistema de Archivos: {0}", d.DriveFormat));
                            wListDisk.Append(" - ");
                            wListDisk.Append(string.Format("Espacio Disponible: {0} GB", (((d.TotalFreeSpace / 1024) / 1024) / 1024)));
                            wListDisk.Append(" - ");
                            wListDisk.Append(string.Format("Espacio Total en Disco: {0} GB ", (((d.TotalSize / 1024) / 1024) / 1024)));
                            wListDisk.Append("</li>");
                        }
                    }
                    if (DriveInfo.GetDrives().Length > 0)
                    {
                        wListDisk.Append("</ul>");
                        wListDisk.Append("</div>");
                    }

                    #endregion
                    #region Sistema Operativo
                    string wNameOS = new ComputerInfo().OSFullName;
                    string wVersionOS = new ComputerInfo().OSVersion;
                    bool wIs64Bit = Environment.Is64BitOperatingSystem;
                    string wArqOS = string.Empty;
                    if (wIs64Bit)
                        wArqOS = "64 bits";
                    else
                        wArqOS = "32 bits";

                    string wSO = wNameOS + " - Version: " + wVersionOS + " - Arquitectura: " + wArqOS;
                    #endregion

                    #region Procesador
                    string wProcesador = getInfoProcesador();
                    #endregion

                    #region Enviar Metricas PC
                    // Send the data
                    var postData = new
                    {
                        MachineName = System.Environment.MachineName,
                        Processor = cpuTime,
                        MemUsage = memUsage,
                        TotalMemory = totalMemory,
                        Services = services,
                        addressIp = wListIP.ToString(),
                        disk = wListDisk.ToString(),
                        sysos = wSO,
                        processador = wProcesador
                    };
                    try
                    {
                        var json = JsonConvert.SerializeObject(postData);

                        // Post the data to the server
                        var serverUrl = new Uri(ConfigurationManager.AppSettings["ServerUrl"]);

                        var client = new WebClient();
                        client.Headers.Add("Content-Type", "application/json");
                        client.Headers.Add("HostClient", Environment.MachineName);
                        //client.Headers.Add("ClientConnectionID", wConnectionID);
                        var response = client.UploadString(serverUrl, json);
                        Console.WriteLine(DateTime.Now.ToString() + " - URL: " + serverUrl + " - Resultado: OK");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(DateTime.Now.ToString() + " - URL: " + new Uri(ConfigurationManager.AppSettings["ServerUrl"]).ToString() + " - Error: " + ex.Message);
                    }
                    #endregion


                    // Reset the poll time
                    lastPollTime = DateTime.Now;
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        static void GetMetrics(out double processorTime, out ulong memUsage, out ulong totalMemory)
        {
            processorTime = (double)_cpuCounter.NextValue();
            memUsage = (ulong)_memUsageCounter.NextValue();
            totalMemory = 0;

            // Get total memory from WMI
            ObjectQuery memQuery = new ObjectQuery("SELECT * FROM CIM_OperatingSystem");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(memQuery);

            foreach (ManagementObject item in searcher.Get())
            {
                totalMemory = (ulong)item["TotalVisibleMemorySize"];
            }
        }

        static string getInfoProcesador()
        {
            StringBuilder wText = new StringBuilder();
            //Get Info of Cpu from WMI
            ManagementObjectSearcher MOS = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (ManagementObject MO in MOS.Get())
            {
                wText.Append("<li>");
                wText.Append(string.Format("Nombre: {0}", MO["Name"]));
                wText.Append(string.Format("Número de Cores: {0} - ", MO["NumberOfCores"]));
                wText.Append(string.Format("Número de Procesadores Lógicos: {0}", MO["NumberOfLogicalProcessors"]));
                wText.Append("</li>");
                break;
            }
            return wText.ToString();
        }
        public static List<ServiceBE> GetAllServices()
        {
            List<ServiceBE> wServiceList = new List<ServiceBE>();
            ServiceBE wService = new ServiceBE();
            string wPath = string.Empty;
            int cont = 0;
            try
            {
                //
                Console.WriteLine("Inicio de la busquedas de servicios");
                foreach (var itService in ServiceController.GetServices().OrderBy(p => p.DisplayName))
                {
                    if (itService.ServiceType.ToString().Equals("Win32ShareProcess"))
                        continue;

                    //if (!itService.Status.Equals(ServiceControllerStatus.Running))
                    //    continue;

                    if (!Environment.MachineName.Equals("GUSTAVO-ASUS-UX"))
                    {
                        if (!itService.ServiceName.ToLower().Contains("epiron"))
                            continue;
                        if (itService.ServiceName.ToLower().Contains("sql"))
                            continue;
                    }

                    wService = new ServiceBE();
                    wService.machineName = System.Environment.MachineName;
                    wService.serviceName = itService.ServiceName;
                    wService.serviceDisplayName = itService.DisplayName;
                    wService.serviceType = itService.ServiceType.ToString();
                    wService.status = itService.Status.ToString();

                    var obj = Environment.Version;
                    //Detectamos si el framework es 4.6 o Superior
                    if (obj.Major == 4 && obj.MajorRevision == 0 && obj.Build == 30319 && obj.Revision >= 42000)
                        //wService.startType = itService.StartType.ToString();
                        wService.startType = "N/A";
                    else
                        wService.startType = "N/A";

                    using (RegistryKey wKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + wService.serviceName))
                    {
                        if (wKey != null)
                        {
                            wService.Path = (string)wKey.GetValue("ImagePath");
                            wService.Path = wService.Path.Replace("\"", "");
                            if (wService.Path.Contains(@"C:\WINDOWS"))
                            {
                                wService.serviceVersion = "N/A";
                            }
                            else
                            {
                                //Buscamos la version del ejecutable
                                try
                                {
                                    var auxPath = wService.Path;
                                    int wIndex = auxPath.IndexOf("/");
                                    if (wIndex > 0)
                                    {
                                        auxPath = auxPath.Substring(0, wIndex - 1);
                                        wService.serviceVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(auxPath).FileVersion;
                                        wService.Path = auxPath;
                                    }
                                    else
                                    {
                                        wIndex = auxPath.IndexOf(" -");
                                        if (wIndex > 0)
                                        {
                                            auxPath = auxPath.Substring(0, wIndex);
                                            wService.serviceVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(auxPath).FileVersion;
                                            wService.Path = auxPath;
                                        }
                                        else
                                        {
                                            wIndex = auxPath.IndexOf(".exe ");
                                            if (wIndex > 0)
                                            {
                                                auxPath = auxPath.Substring(0, wIndex + 4);
                                                wService.serviceVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(auxPath).FileVersion;
                                                wService.Path = auxPath;
                                            }
                                            else
                                            {
                                                if (File.Exists(wService.Path))
                                                {
                                                    wService.serviceVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(wService.Path).FileVersion;
                                                }
                                                else
                                                {
                                                    wService.serviceVersion = "No existe la ruta " + wService.Path;
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("ERROR: " + ex.Message);
                                    wService.serviceVersion = "N/A";
                                }


                            }
                        }
                    }
                    //Buscamos todos los archivos dlls y exe en el folder path
                    if (wService.serviceVersion != "N/A")
                    {

                        string wDirectory = Path.GetDirectoryName(wService.Path);
                        if (!Directory.Exists(wDirectory))
                        {
                            wService.filesVersion = "N/A";
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();
                            try
                            {
                                if (!string.IsNullOrEmpty(wDirectory))
                                {
                                    //Buscamos todos los archivos del directorio
                                    DirectoryInfo di = new DirectoryInfo(wDirectory);
                                    if (di.GetFiles() != null && di.GetFiles().Length > 0)
                                    {
                                        sb.Append("<button type=\"button\" class=\"btn btn-info collapsible\"  onclick=\"collapse()\">+</button>");
                                        sb.Append("<div id=\"collapseServices\" class=\"content\">");
                                        sb.Append("<table class=\"table table-striped table-hover\">");
                                        sb.Append("<thead><tr><th scope=\"col\"> Archivo </th><th scope=\"col\"> Versión</th></tr></thead>");
                                        sb.Append("<tbody>");
                                    }
                                    foreach (var file in di.GetFiles())
                                    {
                                        if (!((file.Extension == ".dll") || (file.Extension == ".exe")))
                                            continue;
                                        if (file.Name.ToLower() == (ConfigurationManager.AppSettings["DllDefault"].ToString().ToLower()) || file.Name.ToLower() == (ConfigurationManager.AppSettings["DllAQDefault"].ToString().ToLower()))
                                        {
                                            wService.serviceVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(file.FullName).FileVersion;
                                            wService.Path = file.FullName;
                                        }

                                        sb.Append("<tr>");
                                        sb.AppendLine("<td>" + file.FullName + "</td><td>" + System.Diagnostics.FileVersionInfo.GetVersionInfo(file.FullName).FileVersion + "</td>");
                                        sb.Append("</tr>");
                                    }
                                    if (di.GetFiles() != null && di.GetFiles().Length > 0)
                                    {
                                        sb.Append("</tbody>");
                                        sb.Append("</table>");
                                        sb.Append("</div>");
                                        
                                    }

                                    wService.filesVersion = sb.ToString();
                                    if (string.IsNullOrEmpty(wService.filesVersion))
                                        wService.filesVersion = "N/A";
                                }
                                else
                                {
                                    wService.filesVersion = "N/A";
                                }

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(wDirectory + " - " + ex.Message);
                            }
                        }


                    }
                    else
                    {
                        wService.filesVersion = "N/A";
                    }

                    wService.connectionID = wConnectionID;
                    wServiceList.Add(wService);
                    cont++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                return null;
            }


            return wServiceList;
        }
    }
}
