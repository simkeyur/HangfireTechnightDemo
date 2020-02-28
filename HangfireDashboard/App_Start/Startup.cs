using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.Owin;
using Owin;
using Hangfire.SqlServer;
[assembly: OwinStartup(typeof(HangfireDashboard.App_Start.Startup))]

namespace HangfireDashboard.App_Start
{
    public class Startup
    {
        [Obsolete]
        public void Configuration(IAppBuilder app)
        {
            //HangfireJobServer server = new HangfireJobServer();
            //server.InitializeJobServer();
            GlobalConfiguration.Configuration
                   .UseSqlServerStorage("HangfireDB");

            //app.UseHangfire(config =>
            //{
            //    config.UseAuthorizationFilters(
            //            new BasicAuthAuthorizationFilter(
            //                    new BasicAuthAuthorizationFilterOptions
            //                    {
            //                // Require secure connection for dashboard
            //                RequireSsl = true,

            //                // Case sensitive login checking
            //                LoginCaseSensitive = true,

            //                // Users
            //                Users = new[]
            //                        {
            //                            new BasicAuthAuthorizationUser
            //                            {
            //                                Login = "hfadmin",

            //                                // Password as plain text
            //                                PasswordClear = "Sogeti123"
            //                            }
            //                        }
            //                    }));
            //});

            app.UseHangfireDashboard();
        }
    }
}