using System;
using System.Runtime.Serialization;

namespace Service.ClientPortfolioHistory.Grpc.Models
{
    [DataContract]
    public class HistoryGraphRequest
    {
        [DataMember(Order = 1)]
        public string ClientId { get; set; }
        [DataMember(Order = 2)]
        public string BrandId { get; set; }
        [DataMember(Order = 3)]
        public string BrokerId { get; set; }
        [DataMember(Order = 4)]
        public string TargetAsset { get; set; }
        [DataMember(Order = 5)]
        public DateTime From { get; set; }
        [DataMember(Order = 6)]
        public DateTime To { get; set; }
    }
}