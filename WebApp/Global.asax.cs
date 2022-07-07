using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebApp
{
    public class WebApiApplication : System.Web.HttpApplication
    {

        protected void Application_Start()
        {
            
            GlobalConfiguration.Configure(WebApiConfig.Register);
            
        }
    }
}
