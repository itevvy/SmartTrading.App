using System.Text.Json.Serialization;

namespace SmartTrading.App.Models;

/// <summary>
/// 주식 현재가 시세 응답 모델
/// (API 문서: 주식현재가 시세 -> output)
/// </summary>
public class KisQuoteResponse
{
    /// <summary>
    /// API 응답 결과 코드 (0: 성공)
    /// </summary>
    [JsonPropertyName("rt_cd")]
    public string ResultCode { get; set; } = string.Empty;

    /// <summary>
    /// 응답 메시지
    /// </summary>
    [JsonPropertyName("msg1")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 실제 데이터 (output)
    /// </summary>
    [JsonPropertyName("output")]
    public KisQuoteOutput? Output { get; set; }
}

public class KisQuoteOutput
{
    /// <summary>
    /// 현재가 (stck_prpr)
    /// </summary>
    [JsonPropertyName("stck_prpr")]
    public string CurrentPrice { get; set; } = string.Empty;

    /// <summary>
    /// 전일 대비 (prdy_vrss)
    /// </summary>
    [JsonPropertyName("prdy_vrss")]
    public string ChangeAmount { get; set; } = string.Empty;

    /// <summary>
    /// 전일 대비율 (prdy_ctrt)
    /// </summary>
    [JsonPropertyName("prdy_ctrt")]
    public string ChangeRate { get; set; } = string.Empty;
}