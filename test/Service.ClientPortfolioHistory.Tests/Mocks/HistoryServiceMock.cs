using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Service.BalanceHistory.Domain.Models;
using Service.BalanceHistory.Grpc;
using Service.BalanceHistory.Grpc.Models;

namespace Service.ClientPortfolioHistory.Tests.Mocks
{
    public class HistoryServiceMock : IOperationHistoryService
    {
        public async Task<OperationUpdateList> GetBalanceUpdatesAsync(GetOperationsRequest request)
        {
             var updates = new List<OperationUpdate>();
            updates.Add(new OperationUpdate()
            {
                AssetId = "BTC",
                TimeStamp = DateTime.Parse("2020-01-02T22:00:00"),
                Balance = 28,
                Amount = -4
            });
            updates.Add(new OperationUpdate()
            {
                AssetId = "BTC",
                TimeStamp = DateTime.Parse("2020-01-02T18:00:00"),
                Balance = 32,
                Amount = 30
            });
            updates.Add(new OperationUpdate()
            {
                AssetId = "ETH",
                TimeStamp = DateTime.Parse("2020-01-02T16:00:00"),
                Balance = 44,
                Amount = 20
            });
            updates.Add(new OperationUpdate()
            {
                AssetId = "BTC",
                TimeStamp = DateTime.Parse("2020-01-02T12:00:00"),
                Balance = 2,
                Amount = 2
            });
            updates.Add(new OperationUpdate()
            {
                AssetId = "ETH",
                TimeStamp = DateTime.Parse("2019-12-03T21:00:00"),
                Balance = 20,
                Amount = 20
            });
            updates.Add(new OperationUpdate()
            {
                AssetId = "LTC",
                TimeStamp = DateTime.Parse("2019-12-03T18:00:00"),
                Balance = 0,
                Amount = -10
            });
            updates.Add(new OperationUpdate()
            {
                AssetId = "LTC",
                TimeStamp = DateTime.Parse("2019-12-03T14:00:00"),
                Balance = 10,
                Amount = 5
            });
            updates.Add(new OperationUpdate()
            {
                AssetId = "LTC",
                TimeStamp = DateTime.Parse("2019-12-02T12:00:00"),
                Balance = 15,
                Amount = 2
            });
            return new OperationUpdateList()
            {
                OperationUpdates = updates.Where(t=>t.TimeStamp <= request.To && t.TimeStamp >=request.From).ToList()
            };
        }
    }
}