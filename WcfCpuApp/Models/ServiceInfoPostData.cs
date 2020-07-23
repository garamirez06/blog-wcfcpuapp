
namespace WcfCpuApp.Models
{
    public class ServiceInfoPostData
    {
        public string MachineName { get; set; }
        public string serviceName { get; set; }
        public string serviceDisplayName { get; set; }
        public string serviceType { get; set; }
        public string status { get; set; }
        public string startType { get; set; }
        public string path { get; set; }
        public string version { get; set; }
    }
}