using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleTrading.CandlesHistory.Grpc;
using SimpleTrading.CandlesHistory.Grpc.Contracts;
using SimpleTrading.CandlesHistory.Grpc.Models;

namespace Service.ClientPortfolioHistory.Tests.Mocks
{
    public class CandleServiceMock : ISimpleTradingCandlesHistoryGrpc
    {
        public async ValueTask<IEnumerable<CandleGrpcModel>> GetCandlesHistoryAsync(GetCandlesHistoryGrpcRequestContract requestContract)
        {
            if(requestContract.Instrument=="BTCUSD")
            {
                var list = new List<CandleGrpcModel>();
                while (requestContract.To >= requestContract.From)
                {
                    list.Add(new CandleGrpcModel()
                    {
                        Open = 10,
                        Close = 10,
                        DateTime = requestContract.To
                    });
                    requestContract.To = requestContract.To.Subtract(TimeSpan.FromMinutes(1));
                }

                return list;
            }
            if(requestContract.Instrument=="ETHUSD")
            {
                var list = new List<CandleGrpcModel>();
                while (requestContract.To >= requestContract.From)
                {
                    list.Add(new CandleGrpcModel()
                    {
                        Open = 2,
                        Close = 2,
                        DateTime = requestContract.To
                    });
                    requestContract.To = requestContract.To.Subtract(TimeSpan.FromMinutes(1));
                }

                return list;
            }

            if (requestContract.Instrument == "LTCUSD" && requestContract.CandleType == CandleTypeGrpcModel.Minute)
            {
                var list = new List<CandleGrpcModel>();
                var to = requestContract.To;
                var from = requestContract.From.Add(TimeSpan.FromDays(1)); 
                while (to >= from)
                {
                    list.Add(new CandleGrpcModel()
                    {
                        Open = 2,
                        Close = 2,
                        DateTime = to
                    });
                    to = to.Subtract(TimeSpan.FromMinutes(1));
                }
                return list;
            }
            
            if (requestContract.Instrument == "LTCUSD" && requestContract.CandleType == CandleTypeGrpcModel.Hour)
            {
                var list = new List<CandleGrpcModel>();
                var to = requestContract.To; 
                var from = requestContract.From; 
                while (to >= from)
                {
                    list.Add(new CandleGrpcModel()
                    {
                        Open = 3,
                        Close = 3,
                        DateTime = to
                    });
                    to = to.Subtract(TimeSpan.FromHours(1));
                }
                return list;
            }

            return new List<CandleGrpcModel>();
        }

        public IAsyncEnumerable<CandleGrpcModel> GetCandlesHistoryStream(GetCandlesHistoryGrpcRequestContract requestContract)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask<IEnumerable<CandleGrpcModel>> GetLastCandlesAsync(GetLastCandlesGrpcRequestContract requestContract)
        {
            throw new System.NotImplementedException();
        }

        public async ValueTask<ReloadInstrumentModel> ReloadInstrumentAsync(ReloadInstrumentContract requestContract)
        {
            throw new System.NotImplementedException();
        }

        public IAsyncEnumerable<CacheCandleGrpcModel> GetAllFromCacheAsync(GetAllFromCacheGrpcRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}