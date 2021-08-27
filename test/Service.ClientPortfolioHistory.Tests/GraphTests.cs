using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Service.AssetsDictionary.Client;
using Service.BalanceHistory.Domain.Models;
using Service.BalanceHistory.Grpc;
using Service.Balances.Grpc;
using Service.ClientPortfolioHistory.Grpc.Models;
using Service.ClientPortfolioHistory.Services;
using Service.ClientPortfolioHistory.Tests.Mocks;
using Service.ClientWallets.Grpc;
using SimpleTrading.CandlesHistory.Grpc;

namespace Service.ClientPortfolioHistory.Tests
{
    public class GraphTests
    {
        private PortfolioGraphService _graphService;
        private IOperationHistoryService _historyMock;
        private ISimpleTradingCandlesHistoryGrpc _candleService;
        private ILogger<PortfolioGraphService> _logger;
        private IClientWalletService _clientWallet;
        private IWalletBalanceService _walletBalance;
        private IAssetsDictionaryClient _assetsDictionary;
        [SetUp]
        public void Setup()
        {
             _historyMock = new HistoryServiceMock();
             _candleService = new CandleServiceMock();
             _clientWallet = new ClientWalletMock();
             _walletBalance = new WalletBalanceMock();
             _assetsDictionary = new AssetDictionaryMock();
             _logger = new NullLogger<PortfolioGraphService>();
        }

        [DatapointSource] 
        public (string,decimal)[] TargetAssets =
        {
            ("USD",1),
            ("BTC",10), 
            ("ETH",2)
        };
        [Theory]
        public async Task TestGraphPoints((string targetAsset, decimal rateToUsd) input)
        {           
            _graphService = new PortfolioGraphService(_candleService, _logger, _clientWallet, _historyMock, _walletBalance, _assetsDictionary);

            var toPoint = DateTime.Parse("2020-01-04T00:00:00");
            var fromPoint = DateTime.Parse("2019-12-03T19:00:00");

            var btcToUsd = 10;
            var ethToUsd = 2;
            var ltcToUsdComplete = 2;

            var response = await _graphService.CreateHistoryGraph(new HistoryGraphRequest()
            {
                To = toPoint,
                From = fromPoint,
                TargetAsset = input.targetAsset
            });
            var graph = response.Graph;

            var pointBtc28 = DateTime.Parse("2020-01-03T00:00:00");
            var pointBtc32 = DateTime.Parse("2020-01-02T20:00:00");
            var pointEth24 = DateTime.Parse("2020-01-02T17:00:00");
            var pointEth0 = DateTime.Parse("2019-12-03T20:00:00");

            Assert.AreEqual(graph[pointBtc28], (28 * btcToUsd + 44 * ethToUsd)/input.rateToUsd);
            Assert.AreEqual(graph[pointBtc32], (32 * btcToUsd + 44 * ethToUsd)/input.rateToUsd);
            Assert.AreEqual(graph[pointEth24], (2 * btcToUsd + 44 * ethToUsd)/input.rateToUsd);
            Assert.AreEqual(graph[pointEth0], (0 * btcToUsd + 0 * ethToUsd)/input.rateToUsd);

        }
        
        [Theory]
        public async Task TestGraphPointsWithIncompleteCandles((string targetAsset, decimal rateToUsd) input)
        {           
            _graphService = new PortfolioGraphService(_candleService, _logger, _clientWallet, _historyMock, _walletBalance, _assetsDictionary);

            var toPoint = DateTime.Parse("2020-01-04T00:00:00");
            var fromPoint = DateTime.Parse("2019-12-02T09:00:00");
            var ltcToUsdComplete = 2;
            var ltcToUsdIncomplete = 3;
            
            var response = await _graphService.CreateHistoryGraph(new HistoryGraphRequest()
            {
                To = toPoint,
                From = fromPoint,
                TargetAsset = input.targetAsset
            });
            var graph = response.Graph;
            
            var pointLtc0 = DateTime.Parse("2019-12-03T20:00:00");
            var pointLtc10 = DateTime.Parse("2019-12-03T17:00:00");
            var pointLtc15 = DateTime.Parse("2019-12-02T14:00:00");
            var pointLtc13 = DateTime.Parse("2019-12-02T10:00:00");
            
            Assert.AreEqual((0 * ltcToUsdComplete)/input.rateToUsd, graph[pointLtc0]);
            Assert.AreEqual((10 * ltcToUsdComplete)/input.rateToUsd, graph[pointLtc10]);
            Assert.AreEqual((15 * ltcToUsdIncomplete)/input.rateToUsd, graph[pointLtc15]);
            Assert.AreEqual((13 * ltcToUsdIncomplete)/input.rateToUsd, graph[pointLtc13]);
        }

    }
}
