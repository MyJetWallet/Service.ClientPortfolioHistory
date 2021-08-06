using System.ServiceModel;
using System.Threading.Tasks;
using Service.ClientPortfolioHistory.Grpc.Models;

namespace Service.ClientPortfolioHistory.Grpc
{
    [ServiceContract]
    public interface IPortfolioGraphService
    {
        [OperationContract]
        Task<HistoryGraphResponse> CreateHistoryGraph(HistoryGraphRequest request);
    }
}