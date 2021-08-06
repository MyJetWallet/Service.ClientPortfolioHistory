using System;
using System.Threading.Tasks;
using ProtoBuf.Grpc.Client;
using Service.ClientPortfolioHistory.Client;
using Service.ClientPortfolioHistory.Grpc.Models;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;

            Console.Write("Press enter to start");
            Console.ReadLine();


            var factory = new ClientPortfolioHistoryClientFactory("http://localhost:5001");
            //var client = factory.GetPortfolioGraphService();

            // var resp = await  client.SayHelloAsync(new HistoryGraphRequest(){Name = "Alex"});
            // Console.WriteLine(resp?.Message);

            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
