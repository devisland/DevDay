using System;
using System.Security.Principal;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using DevDay.Authentication;

namespace DevDay
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        public override void Init()
        {
            PostAuthenticateRequest += OnPostAuthenticateRequestHandler;

            base.Init();
        }

        private void OnPostAuthenticateRequestHandler(object sender, EventArgs e)
        {
            var formsCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (formsCookie == null) return;

            var auth = FormsAuthentication.Decrypt(formsCookie.Value);

            var userID = int.Parse(auth.UserData);

            var principal = new CustomPrincipal(Roles.Provider.Name, new GenericIdentity(auth.Name), userID);

            Context.User = Thread.CurrentPrincipal = principal;
        }
    }
}