using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace HangfireService
{
    class Program
    {
        static void Main(string[] args)
        {
            //Use TopShelf Host factory to configure the service
            HostFactory.Run(hostConfig =>
            {

                hostConfig.UseAssemblyInfoForServiceInfo();
                hostConfig.StartAutomaticallyDelayed();
                hostConfig.RunAsLocalSystem();

                //Configure the recovery
                hostConfig.EnableServiceRecovery(recovery => {
                    //first failure, 5 minute delay
                    recovery.RestartService(5);
                    //Second failure, 10 minute delay
                    recovery.RestartService(10);

                });
                hostConfig.Service<BackgroundService>(serviceConfig =>
                {
                    serviceConfig.ConstructUsing(() => new BackgroundService());
                    serviceConfig.WhenStarted(s => s.Start());
                    serviceConfig.WhenStopped(s => s.Stop());
                    serviceConfig.WhenContinued(s => s.Continue());
                    serviceConfig.WhenShutdown(s => s.Stop());
                    serviceConfig.WhenPaused(s => s.Stop());
                });
            });

            //GlobalConfiguration.Configuration
            //         .UseSqlServerStorage("AppJob");
            ////100
            //for (int cnt = 0; cnt < 100; cnt++)
            //{
            //    RecurringJob.RemoveIfExists(cnt.ToString());
            //}


        }
    }
}
