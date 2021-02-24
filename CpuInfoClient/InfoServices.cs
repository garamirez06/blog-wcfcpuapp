using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CpuInfoClient
{
    public class InfoServices
    {
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

                    if (!itService.ServiceName.ToLower().Contains("sql"))
                        if (!itService.ServiceName.ToLower().Contains("epiron"))
                            if (!itService.ServiceName.ToLower().Contains("nginx"))
                                if (!itService.ServiceName.ToUpper().Contains("MSDTC"))
                                    continue;
                    //if (itService.ServiceName.ToLower().Contains("sql"))
                    //    continue;

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
                        if (wService.serviceName.ToLower().Contains("sql") || wService.serviceName.ToUpper().Contains("MSDTC"))
                        {
                            // Query WMI for additional information about this service.
                            // Display the start name (LocalSystem, etc) and the service
                            // description.
                            ManagementObject wmiService;
                            wmiService = new ManagementObject("Win32_Service.Name='" + wService.serviceName + "'");
                            wmiService.Get();
                            wService.filesVersion = wmiService["StartName"].ToString();
                        }
                        else
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
                                                sb.Append("<button type=\"button\" class=\"btn btn-info collapsible\"  onclick=\"collapse()\">Ver " + di.GetFiles().Length + " Archivos </button>");
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

            Console.WriteLine("Final de Busqueda de Servicios: " + cont);
            return wServiceList;
        }
    }
}
