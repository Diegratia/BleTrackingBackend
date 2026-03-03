using System.Text.Json.Serialization;

namespace Shared.Contracts.Read
{
    public class MstIntegrationRead : BaseRead
    {
        [JsonPropertyOrder(1)]
        public Guid BrandId { get; set; }
        [JsonPropertyOrder(2)]
        public string? IntegrationType { get; set; }
        [JsonPropertyOrder(3)]
        public string? ApiTypeAuth { get; set; }
        [JsonPropertyOrder(4)]
        public string? ApiUrl { get; set; }
        [JsonPropertyOrder(5)]
        public string? ApiAuthUsername { get; set; }
        [JsonPropertyOrder(6)]
        public string? ApiAuthPasswd { get; set; }
        [JsonPropertyOrder(7)]
        public string? ApiKeyField { get; set; }
        [JsonPropertyOrder(8)]
        public string? ApiKeyValue { get; set; }
        [JsonPropertyOrder(9)]
        public BrandNavigationRead? Brand { get; set; }
    }
}
