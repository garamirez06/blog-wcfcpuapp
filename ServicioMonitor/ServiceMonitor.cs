using AgenteManager;
using System.ServiceProcess;

namespace ServicioMonitor
{
    public partial class ServiceMonitor : ServiceBase
    {
        public ServiceMonitor()
        {
            InitializeComponent();

            //Cargar la Clase EventLog
            eventosSistemas = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("FalconAgente")) {
                System.Diagnostics.EventLog.CreateEventSource("FalconAgente", "Application");
            }
            eventosSistemas.Source = "FalconAgente";
            eventosSistemas.Log = "Application";
        }



        protected override void OnStart(string[] args)
        {
            eventosSistemas.WriteEntry("Iniciando");
            try
            {
                var agente = new AgentManager();
                agente.StartAgent();
            }
            catch (System.Exception)
            {

                throw;
            }
            

        }

        protected override void OnStop()
        {
        }
    }
}
