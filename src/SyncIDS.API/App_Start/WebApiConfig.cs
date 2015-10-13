using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.ExceptionHandling;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SyncIDS.API
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            // Make json the only and default response
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // Camel-casing json response
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
            
            // Enable CORS
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
        }
    }
}