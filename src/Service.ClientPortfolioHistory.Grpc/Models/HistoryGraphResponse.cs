using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.ClientPortfolioHistory.Domain.Models;

namespace Service.ClientPortfolioHistory.Grpc.Models
{
    [DataContract]
    public class HistoryGraphResponse
    {
        [DataMember(Order = 1)]
        public Dictionary<DateTime, decimal> Graph { get; set; }
        
        [DataMember(Order = 2)]
        public PeriodEnum Period { get; set; }
    }
}