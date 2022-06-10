using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Balances.Domain.Models;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;

namespace Service.ClientPortfolioHistory.Tests.Mocks
{
    public class WalletBalanceMock : IWalletBalanceService
    {
        public async Task<WalletBalanceList> GetWalletBalancesAsync(GetWalletBalancesRequest request)
        {
            return new WalletBalanceList()
            {
                Balances = new List<WalletBalance>()
                {
                    new WalletBalance()
                    {
                        AssetId = "BTC",
                        Balance = 28
                    },
                    new WalletBalance()
                    {
                        AssetId = "ETH",
                        Balance = 44
                    },
                    new WalletBalance()
                    {
                        AssetId = "LTC",
                        Balance = 0
                    }
                }
            };
        }

        public async Task<WalletListResponse> GetWalletsByBalanceAsync(GetWalletsByBalanceRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}