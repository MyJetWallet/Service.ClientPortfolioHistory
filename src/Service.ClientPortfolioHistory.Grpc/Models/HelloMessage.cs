using System.Runtime.Serialization;
using Service.ClientPortfolioHistory.Domain.Models;

namespace Service.ClientPortfolioHistory.Grpc.Models
{
    [DataContract]
    public class HelloMessage : IHelloMessage
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
    }
}