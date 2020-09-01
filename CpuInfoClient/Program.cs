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
using Microsoft.Web.Administration;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CpuInfoClient
{
    public class Program
    {
        static bool isAvailable46 = false;
        static bool _running = true;
        static PerformanceCounter _cpuCounter, _memUsageCounter;
        static string wConnectionID = string.Empty;

        static WebProxy _wProxy = null;

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
            // Hello!
            Console.WriteLine("Falcon Agent - Captura de Información en Tiempo Real");
            Console.WriteLine("----------------------------------------------------");
            Thread pollingThread = null;

            //Buscamos si hay configuracion de Proxy
            bool wProxyEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["UseProxy"].ToString());
            if (wProxyEnabled)
            {
                Console.WriteLine(DateTime.Now.ToString() + " - Proxy activado");
                string wDomain = ConfigurationManager.AppSettings["ProxyDomain"].ToString();
                string wUser = ConfigurationManager.AppSettings["ProxyUser"].ToString();
                string wPass = ConfigurationManager.AppSettings["ProxyPassword"].ToString();
                int wPort = Convert.ToInt32(ConfigurationManager.AppSettings["ProxyPort"].ToString());
                string wHost = ConfigurationManager.AppSettings["ProxyHost"].ToString();

                _wProxy = new WebProxy(wHost, wPort);
                _wProxy.Credentials = new System.Net.NetworkCredential(wUser, wPass, wDomain);
            }


            try
            {
                Console.WriteLine(DateTime.Now.ToString() + " - Iniciamos el Modo Escucha");
                //Creamos hilo de Escucha
                HubConnection hubConnection;
                IHubProxy hubProxy;
                var serverUrlTest = new Uri(ConfigurationManager.AppSettings["ServerUrlBase"] + "signalr/hubs");
                hubConnection = new HubConnection(serverUrlTest.ToString());
                if (_wProxy != null)
                {
                    hubConnection.Proxy = _wProxy;
                }
                hubConnection.Headers.Add("HostClient", Environment.MachineName);
                hubProxy = hubConnection.CreateHubProxy("CpuInfo");
                hubProxy.On<string>("Connect", (message) => Console.WriteLine(message));
                hubProxy.On<string>("SendMessageWelcome", (message) => Console.WriteLine(message));
                hubProxy.On<string>("SendMessage", (message) => Console.WriteLine(message));
                hubProxy.On<string>("SendMessageStart", (message) => startService(message));
                hubConnection.Start().Wait();
                wConnectionID = hubConnection.ConnectionId;
                Console.WriteLine(DateTime.Now.ToString() + " - Conectado al Hub, listo para Escuchar Eventos. Cliente: " + wConnectionID);
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString() + " No se pudo conectar al Hub. Mas Info: " + ex.Message);
            }


            try
            {
                #region Creacion de Contadores de Perfomance
                // Sets the culture to French (France)
                //Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
                // Sets the UI culture to French (France)
                //Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");
                //en-US
                //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                //Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                Console.WriteLine(DateTime.Now.ToString() + " - Creamos Contadores de Perfomance para CPU");
                _cpuCounter = new PerformanceCounter();
                _cpuCounter.CategoryName = "Processor";
                _cpuCounter.CounterName = "% Processor Time";
                _cpuCounter.InstanceName = "_Total";
                Console.WriteLine(DateTime.Now.ToString() + " - Creamos Contadores de Perfomance para RAM");
                _memUsageCounter = new PerformanceCounter("Memory", "Available KBytes");

                // Create a new thread to start polling and sending the data
                #endregion
                pollingThread = new Thread(new ParameterizedThreadStart(RunPollingThread));
                pollingThread.Start();
                Console.WriteLine("*****************************************************");
                Console.WriteLine("Presione una tecla para detener el proceso y salir...");
                Console.WriteLine("*****************************************************");
                Console.ReadKey();

                Console.WriteLine("Deteniendo thread..");

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
            // Convert the object that was passed in
            DateTime lastPollTime = DateTime.MinValue;

            Console.WriteLine(DateTime.Now.ToString() + " - Inicio de polling...");
            int frecuencia = 0;
            try
            {
                frecuencia = Convert.ToInt32((ConfigurationManager.AppSettings["frecuencia"]).ToString());
                Console.WriteLine(DateTime.Now.ToString() + " - Polling cada " + frecuencia + " segundos");
                frecuencia = frecuencia * 1000;
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString() + " - Error con la Configuracion de Frecuencia, se utiliza valor por defecto de 10 segundos. Mas Info: " + ex.Message);
                frecuencia = 10000;
            }

            // Start the polling loop
            while (_running)
            {
                // Poll segun frecuencia
                if ((DateTime.Now - lastPollTime).TotalMilliseconds >= frecuencia)
                {
                    double cpuTime;
                    ulong memUsage, totalMemory;
                    Console.WriteLine(DateTime.Now.ToString() + " - Obtenemos los datos de la RAM utilizada");
                    // Get the stuff we need to send
                    GetMetrics(out cpuTime, out memUsage, out totalMemory);
                    #region Direcciones IP
                    Console.WriteLine(DateTime.Now.ToString() + " - Obtenemos las Direcciones IP");
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

                    #endregion
                    /*
                    #region Framework
                    //llamamos a los netframework
                    var net4 = Get1To45VersionFromRegistry();
                    var net45=Get45PlusFromRegistry();
                    #endregion
                    */
                    #region Servicios
                    var services = JsonConvert.SerializeObject(GetAllServices());
                    Console.WriteLine(DateTime.Now.ToString() + " - Obtenemos datos de los Discos Duros");
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
                    Console.WriteLine(DateTime.Now.ToString() + " - Obtenemos información del Sistema Operativo");
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
                    Console.WriteLine(DateTime.Now.ToString() + " - Obtenemos información del Procesador");
                    string wProcesador = getInfoProcesador();
                    #endregion
                    #region IIS
                    var iisSites = JsonConvert.SerializeObject(GetAllSitesIIS());
                    #endregion
                    #region Node
                    var node = JsonConvert.SerializeObject(getInfoNode());
                    #endregion
                    #region Enviar Metricas PC
                    //Obtenemos Pais
                    var pais = (ConfigurationManager.AppSettings["pais"]).ToString();
                    // Send the data
                    var postData = new
                    {
                        MachineName = System.Environment.MachineName,
                        pais = pais.ToUpper(),
                        Processor = cpuTime,
                        MemUsage = memUsage,
                        TotalMemory = totalMemory,
                        Services = services,
                        addressIp = wListIP.ToString(),
                        disk = wListDisk.ToString(),
                        sysos = wSO,
                        processador = wProcesador,
                        iisSites = iisSites,
                        processNode = node
                    };
                    try
                    {
                        var json = JsonConvert.SerializeObject(postData);

                        // Post the data to the server
                        var serverUrl = new Uri(ConfigurationManager.AppSettings["ServerUrl"]);

                        var client = new WebClient();
                        client.Headers.Add("Content-Type", "application/json");
                        client.Headers.Add("HostClient", Environment.MachineName);

                        if (_wProxy != null)
                            client.Proxy = _wProxy;

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
                wText.Append(string.Format("Nombre: {0}<br>", MO["Name"]));
                wText.Append(string.Format("Número de Cores: {0}<br>", MO["NumberOfCores"]));
                wText.Append(string.Format("Número de Procesadores Lógicos: {0}<br>", MO["NumberOfLogicalProcessors"]));
                break;
            }
            return wText.ToString();
        }
        public static List<ServiceBE> GetAllServices()
        {
            var pais = (ConfigurationManager.AppSettings["pais"]).ToUpper().ToString();
            List<ServiceBE> wServiceList = new List<ServiceBE>();
            ServiceBE wService = new ServiceBE();
            string wPath = string.Empty;
            int cont = 0;
            try
            {
                Console.WriteLine(DateTime.Now.ToString() + " - Inicio de la búsqueda de servicios");
                foreach (var itService in ServiceController.GetServices().OrderBy(p => p.DisplayName))
                {

                    if (itService.ServiceType.ToString().Equals("Win32ShareProcess"))
                        continue;


                    if (!itService.ServiceName.ToLower().Contains("epiron"))
                        if (!itService.ServiceName.ToLower().Contains("nginx"))
                            continue;
                    if (itService.ServiceName.ToLower().Contains("sql"))
                        continue;

                    wService = new ServiceBE();
                    wService.pais = pais.ToUpper();
                    wService.machineName = System.Environment.MachineName;
                    wService.serviceName = itService.ServiceName;
                    wService.serviceDisplayName = itService.DisplayName;
                    wService.serviceType = itService.ServiceType.ToString();
                    wService.status = itService.Status.ToString();

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
                                    //Busco si el servicio es Chat y si contiene en path la carpeta daemon
                                    //Ejemplo:
                                    //epiron3chattrynube3.exe
                                    //"D:\Epiron\Epiron Try\Epiron Chat\Chat\daemon\epiron3chattrynube3.exe"
                                    if (wService.serviceName.ToLower().Contains("chat") && wService.Path.ToLower().Contains("daemon"))
                                    {
                                        string wDirectoryParent = Directory.GetParent(wService.Path).ToString();
                                        wDirectoryParent = wDirectoryParent.Replace(@"\daemon\", "\\");
                                        //Preguntamos si existe el archivo versioninfo.json
                                        string wVersionChat = wDirectoryParent.Replace("\\daemon", "") + "\\versioninfo.json";
                                        if (File.Exists(wVersionChat))
                                        {

                                            // read JSON directly from a file
                                            using (StreamReader file = File.OpenText(wVersionChat))
                                            using (JsonTextReader reader = new JsonTextReader(file))
                                            {
                                                JObject o2 = (JObject)JToken.ReadFrom(reader);
                                                VersionChatBE wChat = JsonConvert.DeserializeObject<VersionChatBE>(o2.ToString());
                                                wService.Path = wDirectoryParent;
                                                wService.serviceVersion = wChat.version;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (wService.serviceName.ToLower().Contains("epiron"))
                                        {
                                            if (File.Exists(wService.Path))
                                            {
                                                wService.serviceVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(wService.Path).FileVersion;
                                            }
                                            else
                                            {
                                                wService.serviceVersion = "NO EXISTE LA RUTA - " + wService.Path;
                                            }
                                        }
                                        else
                                        {
                                            wService.serviceVersion = "VERIFICAR SERVICIO: " + wService.serviceName;
                                        }
                                    }


                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(wService.serviceDisplayName + " -  ERROR: " + ex.Message);
                                    wService.serviceVersion = "N/A";
                                }


                            }
                        }
                    }
                    //Buscamos todos los archivos dlls y exe en el folder path
                    if (wService.serviceVersion != "N/A")
                    {
                        if (wService.serviceName.ToLower().Contains("chat"))
                        {
                            wService.filesVersion = "NO Aplica para CHAT";
                        }
                        else
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
                                            sb.Append("<table class=\"table table-striped table-hover\" style='font-size: 11px;'>");
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
                Console.WriteLine("ERROR en Servicios: " + ex.Message);
                return null;
            }


            return wServiceList;
        }

        public static List<SitesIISBE> GetAllSitesIIS()
        {
            List<SitesIISBE> wList = null;
            SitesIISBE wSite = null;
            var pais = (ConfigurationManager.AppSettings["pais"]).ToUpper().ToString();
            var iisManager = new ServerManager();
            SiteCollection sites = iisManager.Sites;
            Console.WriteLine(DateTime.Now.ToString() + " - Listado de Sitios IIS: Se detectaron: {0} sitios", sites.Count);
            if (sites.Count > 0)
            {
                wList = new List<SitesIISBE>();
                foreach (var site in sites)
                {
                    var applicationRoot = site.Applications.Where(a => a.Path == "/").Single();
                    var virtualRoot = applicationRoot.VirtualDirectories.Where(v => v.Path == "/").Single();

                    var binding = string.Empty;
                    //item.Applications[1].VirtualDirectories[0].PhysicalPath
                    if (site.Bindings.Count > 0)
                    {
                        foreach (var enl in site.Bindings)
                        {
                            binding += enl.BindingInformation;
                            if (site.Bindings.Count > 1)
                                binding += " | ";
                        }
                    }
                    wSite = new SitesIISBE();
                    wSite.pais = pais.ToUpper();
                    wSite.machineName = System.Environment.MachineName;
                    wSite.siteID = site.Id;
                    wSite.siteName = site.Name;
                    wSite.siteBinding = binding;
                    wSite.siteState = site.State.ToString();
                    wSite.sitePath = virtualRoot.PhysicalPath;
                    wList.Add(wSite);
                    //Console.WriteLine(string.Format("Id Sitio: {0} - Nombre del Sitio: {1} \n Enlaces:  {2} - Estado: {3} - Ruta: {4}", site.Id, site.Name, binding, site.State, virtualRoot.PhysicalPath));
                }
            }

            return wList;
        }

        #region NetFramework
        //Writes the version
        private static string WriteVersion(string version, string spLevel = "")
        {
            version = version.Trim();
            if (string.IsNullOrEmpty(version))
                return string.Empty;

            string spLevelString = "";
            if (!string.IsNullOrEmpty(spLevel))
                spLevelString = " Service Pack " + spLevel;

            Console.WriteLine($"{version}{spLevelString}");
            return $"{version}{spLevelString}";
        }

        private static string Get1To45VersionFromRegistry()
        {
            StringBuilder wListFramework = new StringBuilder();
            Console.WriteLine("Buscamos los .NET Framework < 4.5");
            // Opens the registry key for the .NET Framework entry.
            using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            {
                foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                {
                    // Skip .NET Framework 4.5 version information.
                    if (versionKeyName == "v4")
                    {
                        continue;
                    }

                    if (versionKeyName.StartsWith("v"))
                    {

                        RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                        // Get the .NET Framework version value.
                        string name = (string)versionKey.GetValue("Version", "");
                        // Get the service pack (SP) number.
                        string sp = versionKey.GetValue("SP", "").ToString();

                        // Get the installation flag, or an empty string if there is none.
                        string install = versionKey.GetValue("Install", "").ToString();
                        if (string.IsNullOrEmpty(install)) // No install info; it must be in a child subkey.
                            wListFramework.AppendLine(WriteVersion(name));
                        else
                        {
                            if (!(string.IsNullOrEmpty(sp)) && install == "1")
                            {
                                wListFramework.AppendLine(WriteVersion(name, sp));
                            }
                        }
                        if (!string.IsNullOrEmpty(name))
                        {
                            continue;
                        }
                        foreach (string subKeyName in versionKey.GetSubKeyNames())
                        {
                            RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                            name = (string)subKey.GetValue("Version", "");
                            if (!string.IsNullOrEmpty(name))
                                sp = subKey.GetValue("SP", "").ToString();

                            install = subKey.GetValue("Install", "").ToString();
                            if (string.IsNullOrEmpty(install)) //No install info; it must be later.
                                wListFramework.AppendLine(WriteVersion(name));
                            else
                            {
                                if (!(string.IsNullOrEmpty(sp)) && install == "1")
                                {
                                    wListFramework.AppendLine(WriteVersion(name, sp));
                                }
                                else if (install == "1")
                                {
                                    wListFramework.AppendLine(WriteVersion(name));
                                }
                            }
                        }
                    }
                }
            }
            return wListFramework.ToString();
        }
        private static string Get45PlusFromRegistry()
        {
            StringBuilder wListFramework = new StringBuilder();
            Console.WriteLine("Buscamos los .NET Framework > 4.5");
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(subkey))
            {
                if (ndpKey == null)
                    return string.Empty;
                //First check if there's an specific version indicated
                if (ndpKey.GetValue("Version") != null)
                {
                    wListFramework.AppendLine(WriteVersion(ndpKey.GetValue("Version").ToString()));
                }
                else
                {
                    if (ndpKey != null && ndpKey.GetValue("Release") != null)
                    {
                        wListFramework.AppendLine((WriteVersion(CheckFor45PlusVersion((int)ndpKey.GetValue("Release")))));
                    }
                }
            }

            // Checking the version using >= enables forward compatibility.
            string CheckFor45PlusVersion(int releaseKey)
            {
                if (releaseKey >= 528040)
                {
                    isAvailable46 = true;
                    return "4.8";
                }
                if (releaseKey >= 461808)
                {
                    isAvailable46 = true;
                    return "4.7.2";
                }
                if (releaseKey >= 461308)
                {
                    isAvailable46 = true;
                    return "4.7.1";
                }
                if (releaseKey >= 460798)
                {
                    isAvailable46 = true;
                    return "4.7";
                }
                if (releaseKey >= 394802)
                {
                    isAvailable46 = true;
                    return "4.6.2";
                }
                if (releaseKey >= 394254)
                {
                    isAvailable46 = true;
                    return "4.6.1";
                }
                if (releaseKey >= 393295)
                {
                    isAvailable46 = true;
                    return "4.6";
                }
                if (releaseKey >= 379893)
                { return "4.5.2"; }
                if (releaseKey >= 378675)
                { return "4.5.1"; }
                if (releaseKey >= 378389)
                { return "4.5"; }
                // This code should never execute. A non-null release key should mean
                // that 4.5 or later is installed.
                return "";
            }

            return wListFramework.ToString();
        }
        #endregion

        #region Node
        private static List<ProcessBE> getInfoNode()
        {
            List<ProcessBE> wListProcessNode = new List<ProcessBE>();
            ProcessBE wProcess = null;
            string wID = string.Empty;
            string wName = string.Empty;
            string wCommandLine = string.Empty;
            string wText = string.Empty;
            string wHandles = string.Empty;
            string wThreads = string.Empty;
            string wMemory = string.Empty;
            string wCPU = string.Empty;
            string wDescription = string.Empty;
            string wOSName = string.Empty;
            Process p = null;

            var pais = (ConfigurationManager.AppSettings["pais"]).ToUpper().ToString();

            ManagementClass mngmtClass = new ManagementClass("Win32_Process");
            foreach (ManagementObject o in mngmtClass.GetInstances())
            {
                //Obtengo los datos del proceso
                if (!o["Name"].ToString().ToLower().Contains("node.exe"))
                    continue;

                //if ((o["CommandLine"] != null))
                //    if (!o["CommandLine"].ToString().Contains("app.js") || !o["CommandLine"].ToString().Contains("server.js"))
                //            continue;


                wText = string.Empty;
                if (o["ProcessId"] != null)
                {
                    wID = o["ProcessId"].ToString();
                    p = Process.GetProcessById(int.Parse(wID));
                }
                wName = o["Name"].ToString();

                wDescription = o["Description"].ToString();

                if (o["CommandLine"] != null)
                {
                    wCommandLine = o["CommandLine"].ToString();
                }

                if (o["HandleCount"] != null)
                    wHandles = o["HandleCount"].ToString();
                if (o["ThreadCount"] != null)
                    wThreads = o["ThreadCount"].ToString();



                string instanceName = GetProcessInstanceName(int.Parse(wID));
                try
                {
                    wProcess = new ProcessBE();
                    wProcess.pais = pais;
                    wProcess.machineName = System.Environment.MachineName;
                    wProcess.processID = wID;
                    wProcess.processName = wName;
                    wProcess.instanceName = instanceName;
                    wProcess.commandLine = wCommandLine;

                    wListProcessNode.Add(wProcess);

                    //Console.WriteLine("ID: " + wID + " - Nombre: " + wName + " - Instancia: " + instanceName + " - " + wCommandLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now.ToString() + " Error: " + ex.Message);
                }
            }

            return wListProcessNode;
        }

        private static string GetProcessInstanceName(int pid)
        {
            try
            {
                PerformanceCounterCategory cat = new PerformanceCounterCategory("Process");
                string[] instances = cat.GetInstanceNames();
                foreach (string instance in instances)
                {
                    if (!instance.ToLower().Contains("node"))
                        continue;

                    using (PerformanceCounter cnt = new PerformanceCounter("Process",
                         "ID Process", instance, true))
                    {
                        int val = (int)cnt.RawValue;
                        if (val == pid)
                        {
                            return instance;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString() + " - Error en la busqueda de Procesos: " + ex.Message);
                return string.Empty;
            }

            return string.Empty;
        }
        #endregion
    }
}
