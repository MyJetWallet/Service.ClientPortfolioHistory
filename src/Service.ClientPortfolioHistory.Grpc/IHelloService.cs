using System.ServiceModel;
using System.Threading.Tasks;
using Service.ClientPortfolioHistory.Grpc.Models;

namespace Service.ClientPortfolioHistory.Grpc
{
    [ServiceContract]
    public interface IHelloService
    {
        [OperationContract]
        Task<HelloMessage> SayHelloAsync(HelloRequest request);
    }
}