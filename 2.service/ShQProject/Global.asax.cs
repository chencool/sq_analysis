namespace Dxc.Shq.WebApi
{
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Core;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configure(FilterConfig.Configure);


            Database.SetInitializer(new Initializer());

            var formatters = GlobalConfiguration.Configuration.Formatters;
            var jsonFormatter = formatters.JsonFormatter;
            var settings = jsonFormatter.SerializerSettings;
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}