using System;
using System.Collections.Generic;
using MyJetWallet.Domain;
using MyJetWallet.Domain.Assets;
using Service.AssetsDictionary.Client;
using Service.AssetsDictionary.Domain.Models;

namespace Service.ClientPortfolioHistory.Tests.Mocks
{
    public class AssetDictionaryMock : IAssetsDictionaryClient
    {
        public IAsset GetAssetById(IAssetIdentity assetId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IAsset> GetAssetsByBroker(IJetBrokerIdentity brokerId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IAsset> GetAssetsByBrand(IJetBrandIdentity brandId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IAsset> GetAllAssets()
        {
            return new List<IAsset>()
            {
                new Asset()
                {
                    Symbol = "ETH",
                    Accuracy = 8
                },
                new Asset()
                {
                    Symbol = "USD",
                    Accuracy = 2
                },
                new Asset()
                {
                    Symbol = "BTC",
                    Accuracy = 8
                }
            };
        }

        public event Action OnChanged;
    }
}