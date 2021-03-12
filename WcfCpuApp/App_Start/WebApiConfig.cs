using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Cors;
using System.Web.Http.Cors;

namespace WcfCpuApp.App_Start
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            //config.MessageHandlers.Add(new JwtAuthHandler());

            //config.SetCorsPolicyProviderFactory(new CorsPolicyFactory());
            var cors = new EnableCorsAttribute("http://localhost:8080", "*", "*");
            config.EnableCors();
            //Attribute routing
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                 routeTemplate: "api/{controller}/{action}/{id}",
                 defaults: new { action = "Get", id = RouteParameter.Optional }
                 );

            config.Formatters.Clear();

            config.Formatters.Add(new JsonMediaTypeFormatter());

            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;

            config.Formatters.Remove(config.Formatters.XmlFormatter);

            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;

            ((DefaultContractResolver)config.Formatters.JsonFormatter.SerializerSettings.ContractResolver).IgnoreSerializableAttribute = true;

            //quita en el json de resultado el $id 
            json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None;

        }
    }
}