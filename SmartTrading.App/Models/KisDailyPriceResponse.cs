using System.Text.Json.Serialization;

namespace SmartTrading.App.Models;

/// <summary>
/// 주식 일별 시세 조회 응답 모델 (TR_ID: FHKST01010400)
/// </summary>
public class KisDailyPriceResponse
{
    /// <summary>
    /// 결과 코드 (0: 성공)
    /// </summary>
    [JsonPropertyName("rt_cd")]
    public string ResultCode { get; set; } = string.Empty;

    /// <summary>
    /// 응답 메시지
    /// </summary>
    [JsonPropertyName("msg1")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 일별 데이터 리스트 (최대 30일치)
    /// </summary>
    [JsonPropertyName("output")]
    public List<KisDailyPriceDetail> Output { get; set; } = [];
}

/// <summary>
/// 일별 상세 데이터
/// </summary>
public class KisDailyPriceDetail
{
    /// <summary>
    /// 영업일자 (YYYYMMDD)
    /// </summary>
    [JsonPropertyName("stck_bsop_date")]
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// 종가
    /// </summary>
    [JsonPropertyName("stck_clpr")]
    public string Close { get; set; } = string.Empty;

    /// <summary>
    /// 시가
    /// </summary>
    [JsonPropertyName("stck_oprc")]
    public string Open { get; set; } = string.Empty;

    /// <summary>
    /// 고가
    /// </summary>
    [JsonPropertyName("stck_hgpr")]
    public string High { get; set; } = string.Empty;

    /// <summary>
    /// 저가
    /// </summary>
    [JsonPropertyName("stck_lwpr")]
    public string Low { get; set; } = string.Empty;

    /// <summary>
    /// 누적 거래량
    /// </summary>
    [JsonPropertyName("acml_vol")]
    public string Volume { get; set; } = string.Empty;
}