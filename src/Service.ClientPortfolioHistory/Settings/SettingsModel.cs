using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.ClientPortfolioHistory.Settings
{
    public class SettingsModel
    {
        [YamlProperty("ClientPortfolioHistory.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("ClientPortfolioHistory.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("ClientPortfolioHistory.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
        
        [YamlProperty("ClientPortfolioHistory.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }
        
        [YamlProperty("ClientPortfolioHistory.BalancesGrpcServiceUrl")]
        public string BalancesGrpcServiceUrl { get; set; }
        
        [YamlProperty("ClientPortfolioHistory.CandlesServiceGrpcUrl")]
        public string CandlesServiceGrpcUrl { get; set; }
        
        [YamlProperty("ClientPortfolioHistory.BalanceHistoryGrpcServiceUrl")]
        public string BalanceHistoryGrpcServiceUrl { get; set; }
        
        [YamlProperty("ClientPortfolioHistory.MaxCachedEntities")]
        public int MaxCachedEntities { get; set; }
        
        [YamlProperty("ClientPortfolioHistory.ClientWalletsGrpcServiceUrl")]
        public string ClientWalletsGrpcServiceUrl { get; set; }
        
        [YamlProperty("ClientPortfolioHistory.GraphPeriodInMin")]
        public int GraphPeriodInMin { get; set; }
    }
}
