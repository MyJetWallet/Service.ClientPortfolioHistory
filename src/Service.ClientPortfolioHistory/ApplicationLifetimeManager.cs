using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.DataReader;

namespace Service.ClientPortfolioHistory
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly MyNoSqlTcpClient[] _myNoSqlTcpClientManagers;

        public ApplicationLifetimeManager(IHostApplicationLifetime appLifetime, ILogger<ApplicationLifetimeManager> logger, MyNoSqlTcpClient[] myNoSqlTcpClientManagers)
            : base(appLifetime)
        {
            _logger = logger;
            _myNoSqlTcpClientManagers = myNoSqlTcpClientManagers;
        }

        protected override void OnStarted()
        {           
            _logger.LogInformation("OnStarted has been called.");
            foreach(var client in _myNoSqlTcpClientManagers)
            {
                client.Start();
            }
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.");
            foreach(var client in _myNoSqlTcpClientManagers)
            {
                try
                {
                    client.Stop();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");
        }
    }
}
