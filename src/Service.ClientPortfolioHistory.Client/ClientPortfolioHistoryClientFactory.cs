using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.ClientPortfolioHistory.Grpc;

namespace Service.ClientPortfolioHistory.Client
{
    [UsedImplicitly]
    public class ClientPortfolioHistoryClientFactory: MyGrpcClientFactory
    {
        public ClientPortfolioHistoryClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IPortfolioGraphService GetHelloService() => CreateGrpcService<IPortfolioGraphService>();
    }
}
