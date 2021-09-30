using System.Collections.Generic;
using System.Threading.Tasks;
using MyJetWallet.Domain;
using Service.ClientWallets.Domain.Models;
using Service.ClientWallets.Grpc;
using Service.ClientWallets.Grpc.Models;

namespace Service.ClientPortfolioHistory.Tests.Mocks
{
    public class ClientWalletMock : IClientWalletService
    {
        public async Task<ClientWalletList> GetWalletsByClient(JetClientIdentity clientId)
        {
            return new ClientWalletList()
            {
                Wallets = new List<ClientWallet>()
                {
                    new ClientWallet()
                    {
                        WalletId = ""
                    }
                }
            };
        }

        public async Task<CreateWalletResponse> CreateWalletAsync(CreateWalletRequest request)
        {
            throw new System.NotImplementedException();
        }

        public async Task<SearchWalletsResponse> SearchClientsAsync(SearchWalletsRequest request)
        {
            throw new System.NotImplementedException();
        }

        public async Task<SetBaseAssetResponse> SetBaseAssetAsync(SetBaseAssetRequest request)
        {
            throw new System.NotImplementedException();
        }

        public async Task SetEnableUseTestNetAsync(SetEnableUseTestNetRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}