using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.Grpc;
using MyJetWallet.Sdk.NoSql;
using Service.AssetsDictionary.Client;
using Service.BalanceHistory.Client;
using Service.Balances.Client;
using Service.ClientPortfolioHistory.Services;
using Service.ClientWallets.Client;
using Service.IndexPrices.Client;
using SimpleTrading.CandlesHistory.Grpc;

namespace Service.ClientPortfolioHistory.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));

            var factory = new MyGrpcClientFactory(Program.Settings.CandlesServiceGrpcUrl);
            
            builder
                .RegisterInstance(factory.CreateGrpcService<ISimpleTradingCandlesHistoryGrpc>())
                .As<ISimpleTradingCandlesHistoryGrpc>()
                .SingleInstance();
            
            builder.RegisterBalancesClients(Program.Settings.BalancesGrpcServiceUrl, myNoSqlClient);
            
            builder.RegisterIndexPricesClient(myNoSqlClient);
            builder.RegisterOperationHistoryClient(myNoSqlClient, Program.Settings.BalanceHistoryGrpcServiceUrl,
                Program.Settings.MaxCachedEntities);
            
            builder.RegisterClientWalletsClients(myNoSqlClient, Program.Settings.ClientWalletsGrpcServiceUrl);
            builder.RegisterAssetsDictionaryClients(myNoSqlClient);
            
            builder.RegisterType<PortfolioGraphService>().AsSelf();
        }
    }
}