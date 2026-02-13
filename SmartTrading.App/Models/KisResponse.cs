using System.Text.Json.Serialization;

namespace SmartTrading.App.Models;

/// <summary>
/// KIS API 접근 토큰 발급 응답 모델
/// </summary>
public class KisTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

/// <summary>
/// [New] 국내 지수(코스피, 코스닥 등) 조회 API 최상위 응답 모델
/// </summary>
public class KisIndexResponse
{
    [JsonPropertyName("rt_cd")]
    public string ResultCode { get; set; } = string.Empty;

    [JsonPropertyName("msg_cd")]
    public string MessageCode { get; set; } = string.Empty;

    [JsonPropertyName("msg1")]
    public string Message { get; set; } = string.Empty;

    /// <summary> 지수 상세 데이터 </summary>
    [JsonPropertyName("output")]
    public KisIndexOutput? Output { get; set; }
}

/// <summary>
/// [New] 국내 지수 상세 정보 모델 (TR: FHPST01010000)
/// </summary>
public class KisIndexOutput
{
    /// <summary> 업종 지수 현재가 (Price) </summary>
    [JsonPropertyName("bstp_nmix_prpr")]
    public string Price { get; set; } = "0";

    /// <summary> 전일 대비 등락 (Change) </summary>
    [JsonPropertyName("bstp_nmix_prdy_vrss")]
    public string Change { get; set; } = "0";

    /// <summary> 전일 대비 부호 (1:상승, 2:상한, 3:보합, 4:하락, 5:하한) </summary>
    [JsonPropertyName("prdy_vrss_sign")]
    public string ChangeDir { get; set; } = "3";

    /// <summary> 전일 대비 등락률 (Rate) </summary>
    [JsonPropertyName("bstp_nmix_prdy_ctrt")]
    public string ChangeRate { get; set; } = "0";

    /// <summary> 누적 거래량 </summary>
    [JsonPropertyName("acml_tr_vol")]
    public string Volume { get; set; } = "0";

    /// <summary> 누적 거래 대금 </summary>
    [JsonPropertyName("acml_tr_pbmn")]
    public string TransactionAmount { get; set; } = "0";
}