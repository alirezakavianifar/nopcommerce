using System.Text.Json.Serialization;

namespace Nop.Plugin.Misc.ArtificialIntelligence.Services;

public class AvalAiCreditResponse
{
    [JsonPropertyName("limit")]
    public decimal Limit { get; set; }

    [JsonPropertyName("remaining_irt")]
    public decimal RemainingIrt { get; set; }

    [JsonPropertyName("remaining_unit")]
    public decimal RemainingUnit { get; set; }

    [JsonPropertyName("total_unit")]
    public decimal TotalUnit { get; set; }

    [JsonPropertyName("exchange_rate")]
    public decimal ExchangeRate { get; set; }

    [JsonPropertyName("account_tier")]
    public int AccountTier { get; set; }

    [JsonPropertyName("credit_sources")]
    public CreditSources CreditSources { get; set; }
}

public class CreditSources
{
    [JsonPropertyName("grants")]
    public List<CreditSourceDetail> Grants { get; set; } = new();

    [JsonPropertyName("packages")]
    public List<CreditSourceDetail> Packages { get; set; } = new();
}

public class CreditSourceDetail
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("amount_irt")]
    public string AmountIrt { get; set; } // string in response JSON, e.g. "25000.00"

    [JsonPropertyName("remaining_irt")]
    public string RemainingIrt { get; set; } // string in response JSON, e.g. "25000.00"

    [JsonPropertyName("end_date")]
    public string EndDate { get; set; }
}
