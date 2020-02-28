using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using System.Configuration;
using Unity;
using Unity.Extension;
using Unity.Injection;
using log4net;

namespace HangfireService
{
    /// <summary>
    /// This class will be hosting the Hangfire background process in Windows service
    /// </summary>
    partial class BackgroundService
    {
        private int MAX_TIMEOUT = 2;
        private int POLL_INTERVAL = 300000; //Milliseconds
        //private string SYSTEM_USER = "System";
        //private int ADMIN_ROLE = 0;

        // Eventlog
        EventLog _eventLog;
        private static readonly ILog _log = LogManager.GetLogger(nameof(BackgroundService));

        private const string SERVICE_NAME = "Background Service";
        private const string LOG_NAME = "TN.AppJob";
        private const string MEMBER_CONTACT_SERVICE = "NotificationFramework";
        private const string APP_JOB_DB_CONNNECTION = "AppJob";

        //Hangfire Background job server
        private BackgroundJobServer _jobServer;

        public BackgroundService()
        {
            try
            {
                _eventLog = new EventLog();
                if (!EventLog.SourceExists(SERVICE_NAME))
                {
                    EventLog.CreateEventSource(
                        SERVICE_NAME, LOG_NAME);
                }
                _eventLog.Source = SERVICE_NAME;
                _eventLog.Log = LOG_NAME;
            }
            catch (Exception ex)
            {
                _log.Error("TN.AppJob is failed to create the BackgroundService.", ex);
            }
        }
        /// <summary>
        /// Initialize the Unity Container and Job Server
        /// </summary>
        protected void InitializeJobServer()
        {
            try
            {
                //Get Unity Container
                var container = new UnityContainer();

                //Register type -- Member Contact -------------
                //container.RegisterType<IRepository, Repository>();
                //container.RegisterType<ILogger, Logger>();
                //container.RegisterType<IBmailLogger, BmailLogger>();
                //container.RegisterType<ITNAuditHelper, TNAuditHelper>();
                //container.RegisterType<ITNLookUp, TNLookUp>();
                //container.RegisterType<IUserService, UserService>();
                //container.RegisterType<IUserRoleRepository, UserRoleRepository>();
                //container.RegisterType<IUserRoleService, UserRoleService>();
                //container.RegisterType<IUserRepository, UserRepository>();
                //container.RegisterType<IService, Service>();
                //container.RegisterType<ICampaignRepository, CampaignRepository>();
                //container.RegisterType<ICampaignService, CampaignService>();
                //container.RegisterType<IWorkflowManager, WorkflowManager>();
                //container.RegisterType<IProcessFactory, ProcessFactory>();
                //container.RegisterType<IProcessRepository, ProcessRepository>();
                //container.RegisterType<ISendBmailService, SendBmailService>();

                //Set the Db connection 
                GlobalConfiguration.Configuration.UseSqlServerStorage(APP_JOB_DB_CONNNECTION)
                    .UseUnityActivator(container);
                //ICampaignRepository repo = container.Resolve<ICampaignRepository>();
                // repo.CheckCampaignRunStatusAndUpdateRequest(5);


                //Current
                JobActivator.Current = new UnityJobActivator(container);

                //This service will be used as common background service in TN, so we need to differenciate the instances while running multiple instances
                var options = new BackgroundJobServerOptions
                {
                    ServerName = string.Format("{0}.{1}", Environment.MachineName, ConfigurationManager.AppSettings.Get("AppJob.InstanceName").ToString()),
                    WorkerCount = Environment.ProcessorCount * 5

                };

                //Start the Job Servicer'
                _jobServer = new BackgroundJobServer(options);

                int maxRetries = 10;
                int.TryParse(ConfigurationManager.AppSettings.Get("MaximumRetries"), out maxRetries);

                GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = maxRetries });

                //Initialized
                _log.Info("TN App Job has been initialized.");
            }
            catch (Exception ex)
            {
                _log.Fatal("TN.AppJob failed to initialize the Job server.", ex);
            }
        }

        public void Start()
        {
            //Start
            _log.Info("Starting TN App Job Service...");
            _eventLog.WriteEntry("Starting TN App Job Service...");
            //Initialize Job server
            InitializeJobServer();

            //Create Member Contact Recurring Job
            //Campaign Timeout
            int campaignTimeout = MAX_TIMEOUT;
            int.TryParse(ConfigurationManager.AppSettings.Get("MaximumCampaignTimeout"), out campaignTimeout);

            //Poll Interval
            int pollDelay = POLL_INTERVAL;
            int.TryParse(ConfigurationManager.AppSettings.Get("PollFrequencey"), out pollDelay);

            //Create/Update the background job
            //RecurringJob.AddOrUpdate<CampaignService>(s => s.CheckCampaignRunStatusAndUpdateRequest(campaignTimeout, SYSTEM_USER, ADMIN_ROLE), Cron.MinuteInterval(pollDelay));
        }

        public void Stop()
        {
            //Stop
            _log.Info("Stopping TN App Job Service...");
            _eventLog.WriteEntry("Stopping TN App Job Service !");
            //Dispose the service
            _jobServer.Dispose();
        }
        public void Continue()
        {
            //Stop
            _log.Info("Continuing TN App Job Service...");
            _eventLog.WriteEntry("Continuing TN App Job Service...");
            //Initialize Job server
            InitializeJobServer();
        }

    }
}
