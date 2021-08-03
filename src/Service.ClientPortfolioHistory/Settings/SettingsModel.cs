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
    }
}
