using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Domain;
using MyJetWallet.Domain.Assets;
using Service.AssetsDictionary.Client;
using Service.AssetsDictionary.Domain.Models;
using Service.BalanceHistory.Client;
using Service.BalanceHistory.Domain.Models;
using Service.BalanceHistory.Grpc;
using Service.BalanceHistory.Grpc.Models;
using Service.Balances.Domain.Models;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;
using Service.ClientPortfolioHistory.Grpc;
using Service.ClientPortfolioHistory.Grpc.Models;
using Service.ClientWallets.Grpc;
using SimpleTrading.Abstraction.Candles;
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
        private readonly IAssetsDictionaryClient _assetsDictionary;
        private readonly ILogger<PortfolioGraphService> _logger;
        
        private const string UsdAsset = "USD";
        private readonly List<DateTime> _timeSlots = new List<DateTime>();
        public PortfolioGraphService(ISimpleTradingCandlesHistoryGrpc candleService, ILogger<PortfolioGraphService> logger, IClientWalletService clientWalletService, IOperationHistoryService historyService, IWalletBalanceService walletBalanceService, IAssetsDictionaryClient assetsDictionary) 
        {
            _candleService = candleService;
            _logger = logger;
            _clientWalletService = clientWalletService;
            _historyService = historyService;
            _walletBalanceService = walletBalanceService;
            _assetsDictionary = assetsDictionary;
        }


        public async Task<HistoryGraphResponse> CreateHistoryGraph(HistoryGraphRequest request)
        {
            var to = request.To == DateTime.MinValue
                ? DateTime.UtcNow
                : request.To;
            
            var from = request.From == DateTime.MinValue
                ? GetFromPoint(request.Period, to)
                : request.From;

            var step = GetStepByPeriod(request.Period);

            List<IAsset> assets = new();
            while (!assets.Any())
            {
                assets = _assetsDictionary.GetAllAssets().ToList();
            }

            var assetAccuracy = assets.First(t => t.Symbol == request.TargetAsset).Accuracy;

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
                if (wallet.CreatedAt != DateTime.MinValue && request.Period == PeriodEnum.All)
                    from = wallet.CreatedAt;

                operations = await GetOperations(assets, wallet.WalletId, from);
                var balanceResponse = await _walletBalanceService.GetWalletBalancesAsync(new GetWalletBalancesRequest()
                {
                    WalletId = wallet.WalletId
                });

                if (balanceResponse == null || balanceResponse.Balances == null || !balanceResponse.Balances.Any())
                {
                    _logger.LogWarning(
                        "Unable to find any actual balances for user {ClientId} and wallet {WalletId}. Unable to create graph without balances");
                    balanceResponse.Balances = new List<WalletBalance>();
                }

                initialBalances = balanceResponse.Balances.ToDictionary(key => key.AssetId, value => (decimal) value.Balance);
            }

            var toRounded = to.Floor(new TimeSpan(0, 5, 0));
            var fromRounded = from.Floor(new TimeSpan(0, 5, 0));
            while (toRounded>fromRounded)
            {
                _timeSlots.Add(toRounded);
                toRounded -= step;
            }

            var assetList = initialBalances.Keys.ToList();
            var balancesDictionaryInUsdByAsset = new Dictionary<string, Dictionary<DateTime, decimal>>();
            var operationsDictionaryByAsset = operations.OrderByDescending(e => e.TimeStamp).GroupBy(e => e.AssetId).ToDictionary(key=>key.Key, value=>value.ToList());
            
            foreach (var asset in assetList)
            {
                var candleDictionary = await GetCandlesDictionary(asset, from, to, request.Period);
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
            }
            
            if (request.TargetAsset == UsdAsset)
            {
                foreach (var (key, value) in totalBalanceInUsd)
                {
                    totalBalanceInUsd[key] = Math.Round(value, assetAccuracy);
                }
                return new HistoryGraphResponse()
                {
                    Graph = totalBalanceInUsd
                };
            }

            var totalBalanceInTarget = new Dictionary<DateTime, decimal>();
            var candles = await GetCandlesDictionary(request.TargetAsset, from, to, request.Period);
            foreach (var time in _timeSlots)
            {
                var price = 1 / GetCandlePoint(time, candles, request.TargetAsset);
                var balance = totalBalanceInUsd[time] * price;
                var roundedBalance = Math.Round(balance, assetAccuracy);
                totalBalanceInTarget.Add(time, roundedBalance);
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
                if (!candleDict.Any())
                    return 0;
                
                return !candleDict.TryGetValue(timePoint, out var price) 
                        ? candleDict.First(c=>c.Key < timePoint).Value 
                        : price;
            }
            catch
            {
                _logger.LogError("Unable to find candle for asset {Asset} for timepoint {TimePoint}", asset, timePoint);
                return candleDict.LastOrDefault().Value;
            }
        }

        private async Task<Dictionary<DateTime, decimal>> GetCandlesDictionary(string asset, DateTime from, DateTime to, PeriodEnum period)
        {
            try
            {
                var tries = 0;
                var maxTries = 10;

                if (asset == UsdAsset)
                    return new Dictionary<DateTime, decimal>();

                var candleType = GetCandleTypeByPeriod(period);
                
                var candles = (await _candleService.GetCandlesHistoryAsync(new GetCandlesHistoryGrpcRequestContract
                {
                    Bid = false,
                    CandleType = candleType,
                    From = from,
                    To = to,
                    Instrument = $"{asset}USD"
                })).OrderByDescending(e => e.DateTime).ToList();


                while (!candles.Any())
                {
                    _logger.LogWarning(
                        "Unable get minute candles for instrument {Instrument} on time period from: {from} to: {to}",
                        $"{asset}USD", from, to);
                    
                    tries++;
                    if (tries > maxTries)
                        break;
                    
                    candles = (await _candleService.GetCandlesHistoryAsync(
                        new GetCandlesHistoryGrpcRequestContract
                        {
                            Bid = false,
                            CandleType = GetCandleTypeByPeriod(period+1),
                            From = from,
                            To = to,
                            Instrument = $"{asset}USD"
                        })).OrderByDescending(e => e.DateTime).ToList();
                    
                    if(!candles.Any())
                        from = from.Subtract(TimeSpan.FromDays(3));
                }

                if (candles.Last().DateTime != from)
                {
                    _logger.LogWarning(
                        "Unable get minute candles for instrument {Instrument} on time period from: {from} to: {to}",
                        $"{asset}USD", from, candles.Last().DateTime);

                    to = candles.Last().DateTime.Subtract(TimeSpan.FromHours(1));
                    var archiveCandles = (await _candleService.GetCandlesHistoryAsync(
                        new GetCandlesHistoryGrpcRequestContract
                        {
                            Bid = false,
                            CandleType = GetCandleTypeByPeriod(period+1),
                            From = from,
                            To = to,
                            Instrument = $"{asset}USD"
                        })).OrderByDescending(e => e.DateTime).ToList();
                    candles.AddRange(archiveCandles);
                }

                return candles.ToDictionary(key => key.DateTime, value => (decimal)value.Close);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "When trying to get candles for instrument {Instrument}", $"{asset}USD");
                return new Dictionary<DateTime, decimal>();
            }
        }

        private async Task<List<OperationUpdate>> GetOperations(List<IAsset> assetIds, string walletId, DateTime from)
        {
            const int batchSize = 100;
            var operation = new List<OperationUpdate>();
            foreach (var assetId in assetIds)
            {
                var ops = await _historyService.GetBalanceUpdatesAsync(new GetOperationsRequest()
                {
                    WalletId = walletId,
                    AssetId = assetId.Symbol,
                    BatchSize = batchSize,
                    LastDate = DateTime.UtcNow
                });
                if(!ops.OperationUpdates.Any())
                    continue;
                
                operation.AddRange(ops.OperationUpdates);
                var lastSeen = ops.OperationUpdates.Last().TimeStamp;
                while (ops.OperationUpdates.Count == batchSize && lastSeen <= from)
                {
                    ops = await _historyService.GetBalanceUpdatesAsync(new GetOperationsRequest()
                    {
                        WalletId = walletId,
                        AssetId = assetId.Symbol,
                        BatchSize = batchSize,
                        LastDate = lastSeen
                    });
                    operation.AddRange(ops.OperationUpdates);
                    lastSeen = ops.OperationUpdates.Last().TimeStamp;
                }
            }

            return operation;
        }


        private TimeSpan GetStepByPeriod(PeriodEnum period)
        {
            return period switch
            {
                PeriodEnum.OneDay => TimeSpan.FromMinutes(5),
                PeriodEnum.OneWeek => TimeSpan.FromHours(1),
                PeriodEnum.OneMonth => TimeSpan.FromHours(4),
                PeriodEnum.ThreeMonth => TimeSpan.FromDays(1),
                PeriodEnum.OneYear => TimeSpan.FromDays(7),
                PeriodEnum.All => TimeSpan.FromDays(7),
                _ => TimeSpan.FromDays(1)
            };
        }
        private DateTime GetFromPoint(PeriodEnum period, DateTime to)
        {
            return period switch
            {
                PeriodEnum.OneDay => to.Subtract(TimeSpan.FromDays(1)),
                PeriodEnum.OneWeek => to.Subtract(TimeSpan.FromDays(7)),
                PeriodEnum.OneMonth => to.AddMonths(-1),
                PeriodEnum.ThreeMonth => to.AddMonths(-3),
                PeriodEnum.OneYear => to.AddYears(-1),
                PeriodEnum.All => to.AddYears(-1),
                _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
            };
        }
        
        private CandleType GetCandleTypeByPeriod(PeriodEnum period)
        {
            return period switch
            {
                PeriodEnum.OneDay => CandleType.Minute,
                PeriodEnum.OneWeek => CandleType.Hour,
                PeriodEnum.OneMonth => CandleType.Hour,
                PeriodEnum.ThreeMonth => CandleType.Day,
                PeriodEnum.OneYear => CandleType.Day,
                PeriodEnum.All => CandleType.Day,
                _ => CandleType.Day
            };
        }
    }
}