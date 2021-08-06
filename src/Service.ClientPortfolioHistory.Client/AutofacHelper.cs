using Autofac;
using Service.ClientPortfolioHistory.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.ClientPortfolioHistory.Client
{
    public static class AutofacHelper
    {
        public static void RegisterClientPortfolioHistoryClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new ClientPortfolioHistoryClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetPortfolioGraphService()).As<IPortfolioGraphService>().SingleInstance();
        }
    }
}
