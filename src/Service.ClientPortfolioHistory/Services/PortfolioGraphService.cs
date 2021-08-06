using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using Service.BalanceHistory.Client;
using Service.BalanceHistory.Domain.Models;
using Service.BalanceHistory.Grpc;
using Service.BalanceHistory.Grpc.Models;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;
using Service.ClientPortfolioHistory.Grpc;
using Service.ClientPortfolioHistory.Grpc.Models;
using Service.ClientWallets.Grpc;
using SimpleTrading.CandlesHistory.Grpc;
using SimpleTrading.CandlesHistory.Grpc.Contracts;
using SimpleTrading.CandlesHistory.Grpc.Models;

namespace Service.ClientPortfolioHistory.Services
{
    public class PortfolioGraphService: IPortfolioGraphService
    {
        private readonly IOperationHistoryService _historyService;
        private readonly ISimpleTradingCandlesHistoryGrpc _candleService;
        private readonly IClientWalletService _clientWalletService;
        private readonly IWalletBalanceService _walletBalanceService;
        private readonly ILogger<PortfolioGraphService> _logger;
        
        private const string UsdAsset = "USD";
        private readonly TimeSpan _period;
        private readonly List<DateTime> _timeSlots = new List<DateTime>();
        public PortfolioGraphService(ISimpleTradingCandlesHistoryGrpc candleService, ILogger<PortfolioGraphService> logger, IClientWalletService clientWalletService, IOperationHistoryService historyService, IWalletBalanceService walletBalanceService) 
        {
            _candleService = candleService;
            _logger = logger;
            _clientWalletService = clientWalletService;
            _historyService = historyService;
            _walletBalanceService = walletBalanceService;
            _period = TimeSpan.FromMinutes(Program.Settings != null ? Program.Settings.GraphPeriodInMin : 5);
        }


        public async Task<HistoryGraphResponse> CreateHistoryGraph(HistoryGraphRequest request)
        {
            var to = request.To;
            var from = request.From;
            var wallets = await _clientWalletService.GetWalletsByClient(new JetClientIdentity()
            {
                ClientId = request.ClientId,
                BrandId = request.BrandId,
                BrokerId = request.BrokerId
            });

            var initialBalances = new Dictionary<string,decimal>();
            var operations = new List<OperationUpdate>();
            foreach (var wallet in wallets.Wallets)
            {
                var ops = await _historyService.GetBalanceUpdatesAsync(new GetOperationsRequest()
                {
                    WalletId = wallet.WalletId,
                    From = from,
                    To = to
                });
                
                operations.AddRange(ops.OperationUpdates);
                var balanceResponse = await _walletBalanceService.GetWalletBalancesAsync(new GetWalletBalancesRequest()
                {
                    WalletId = wallet.WalletId
                });
                
                if (!balanceResponse.Balances.Any())
                   _logger.LogWarning("Unable to find any actual balances for user {ClientId} and wallet {WalletId}. Unable to create graph without balances"); 
                    
                initialBalances = balanceResponse.Balances.ToDictionary(key => key.AssetId, value => (decimal) value.Balance);
            }

            var toRounded = to.Floor(new TimeSpan(0, 5, 0));
            var fromRounded = from.Floor(new TimeSpan(0, 5, 0));
            while (toRounded>fromRounded)
            {
                _timeSlots.Add(toRounded);
                toRounded -= _period;
            }

            var assetList = initialBalances.Keys.ToList();
            var balancesDictionaryInUsdByAsset = new Dictionary<string, Dictionary<DateTime, decimal>>();
            var operationsDictionaryByAsset = operations.OrderByDescending(e => e.TimeStamp).GroupBy(e => e.AssetId).ToDictionary(key=>key.Key, value=>value.ToList());
            
            foreach (var asset in assetList)
            {
                var candleDictionary = await GetCandlesDictionary(asset, from, to);
                var balanceDictionary = new Dictionary<DateTime, decimal>();
                var balanceDictionaryInUsd = new Dictionary<DateTime, decimal>();

                for (var index = 0; index < _timeSlots.Count; index++)
                {
                    var time = _timeSlots[index];

                    if (index == 0)
                    {
                        balanceDictionary.Add(time, initialBalances[asset]);
                    }
                    else
                    {
                        var balance = balanceDictionary[_timeSlots[index - 1]];
                        if(operationsDictionaryByAsset.TryGetValue(asset, out var operationsList))
                            balance = GetBalancePoint(operationsList, time);
                        
                        balanceDictionary.Add(time, balance);
                    }
                    
                    var price = asset == UsdAsset
                        ? 1
                        : GetCandlePoint(time, candleDictionary, asset);
                    
                    var balanceInUsd = balanceDictionary[time] * price;
                    balanceDictionaryInUsd.Add(time, balanceInUsd);
                }
                balancesDictionaryInUsdByAsset.Add(asset, balanceDictionaryInUsd);
            }

            var totalBalanceInUsd = new Dictionary<DateTime, decimal>();
            foreach (var time in _timeSlots)
            {
                var balance = assetList.Sum(asset => balancesDictionaryInUsdByAsset[asset][time]);
                totalBalanceInUsd.Add(time, balance);
                Console.WriteLine($"{time} {balance}");
            }

            if (request.TargetAsset == UsdAsset)
                return new HistoryGraphResponse()
                {
                    Graph = totalBalanceInUsd
                };
            
            var totalBalanceInTarget = new Dictionary<DateTime, decimal>();
            var candles = await GetCandlesDictionary(request.TargetAsset, from, to);
            foreach (var time in _timeSlots)
            {
                var price = 1 / GetCandlePoint(time, candles, request.TargetAsset);
                var balance = totalBalanceInUsd[time] * price;
                totalBalanceInTarget.Add(time, balance);
                Console.WriteLine($"{time} {balance}");
            }

            return new HistoryGraphResponse()
            {
                Graph = totalBalanceInTarget
            };
        }

        private decimal GetBalancePoint(List<OperationUpdate> updates, DateTime timePoint)
        {
            var relevantUpdates = updates.Where(e => e.TimeStamp <= timePoint).ToList();
            if (relevantUpdates.Any())
                return relevantUpdates.First().Balance;

            var otherUpdates = updates.OrderBy(e => e.TimeStamp).ToList();
            if (otherUpdates.Any())
                return otherUpdates.First().Balance - otherUpdates.First().Amount;

            return 0;
        }


        private decimal GetCandlePoint(DateTime timePoint, Dictionary<DateTime, decimal> candleDict, string asset)
        {
            try
            {
                decimal price;
                while (!candleDict.TryGetValue(timePoint, out price))
                {
                    timePoint = timePoint.Subtract(TimeSpan.FromMinutes(1));
                }

                return price;
            }
            catch
            {
                _logger.LogError("Unable to find candle for asset {Asset} for timepoint {TimePoint}", asset, timePoint);
                throw;
            }
        }

        private async Task<Dictionary<DateTime, decimal>> GetCandlesDictionary(string asset, DateTime from, DateTime to)
        {
            var candles = (await _candleService.GetCandlesHistoryAsync(new GetCandlesHistoryGrpcRequestContract()
            {
                Bid = false,
                CandleType = CandleTypeGrpcModel.Minute,
                From = from,
                To = to,
                Instrument = $"{asset}USD"
            })).OrderByDescending(e=>e.DateTime).ToList();

            if (candles.Last().DateTime != from)
            {
                _logger.LogWarning("Unable get minute candles for instrument {Instrument} on time period {from} {to}", $"{asset}USD", from, to);

                to = candles.Last().DateTime.Subtract(TimeSpan.FromHours(1));
                var archiveCandles = (await _candleService.GetCandlesHistoryAsync(new GetCandlesHistoryGrpcRequestContract()
                {
                    Bid = false,
                    CandleType = CandleTypeGrpcModel.Hour,
                    From = from,
                    To = to,
                    Instrument = $"{asset}USD"
                })).OrderByDescending(e=>e.DateTime).ToList();
                candles.AddRange(archiveCandles);
            }

            return candles.ToDictionary(key => key.DateTime, value => (decimal)value.Close);
        }
    }
}