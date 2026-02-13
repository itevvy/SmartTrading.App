#region using directives

using Microsoft.AspNetCore.Components;

using SmartTrading.App.Models;
using SmartTrading.App.Services.Common;

using System.Net.Http.Json;
using System.Runtime.Versioning;

#endregion

namespace SmartTrading.App.Services.Kis;

/// <summary>
/// KIS 시세 조회 서비스: 지수, 현재가, 일별 시세 데이터를 제공합니다.
/// 모든 보안 키는 IConfiguration 대신 SettingsService(보안 저장소)에서 가져옵니다.
/// </summary>
[SupportedOSPlatform("Android21.0")]
[SupportedOSPlatform("iOS13.0")]
[SupportedOSPlatform("MacCatalyst13.0")]
[SupportedOSPlatform("windows10.0.17763.0")]
public class KisQuoteService(HttpClient httpClient, KisAuthService authService, SettingsService settingsSvc)
{
    #region Variable & Constants

    private readonly HttpClient _httpClient = httpClient;
    private readonly KisAuthService _authService = authService;

    // 💡 생성자 주입을 통해 Null 에러를 방지합니다.
    private readonly SettingsService _settingsSvc = settingsSvc;

    private const string RealUrl = "https://openapi.koreainvestment.com:9443";
    private const string VirtualUrl = "https://openapivts.koreainvestment.com:29443";

    #endregion

    #region Public Methods

    #region Public Methods

    /// <summary>
    /// [1] 시장 지수 조회 (KOSPI: 0001, KOSDAQ: 1001)
    /// </summary>
    public async Task<KisIndexOutput?> GetIndexPriceAsync(string indexCode)
    {
        if (_settingsSvc.IsVirtual)
        {
            return null;
        }

        var token = await _authService.GetAccessTokenAsync();
        string appKey = await _settingsSvc.GetKisAppKeyAsync(false);
        string appSecret = await _settingsSvc.GetKisSecretAsync(false);

        // 지수 조회 전용 URL 및 TR_ID (FID_ORG_ADJ_PRC 필요 없음)
        var url = $"{RealUrl}/uapi/domestic-stock/v1/quotations/inquire-index-price?FID_COND_MRKT_DIV_CODE=U&FID_INPUT_ISCD={indexCode}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Add("authorization", $"Bearer {token}");
        request.Headers.Add("appkey", appKey);
        request.Headers.Add("appsecret", appSecret);
        request.Headers.Add("tr_id", "FHPUP02100000"); // 💡 지수 현재가 전용 TR_ID
        request.Headers.Add("custtype", "P");

        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadFromJsonAsync<KisIndexResponse>();
        return result?.ResultCode == "0" ? result.Output : null;
    }

    /// <summary>
    /// [2] 주식 현재가 조회 (삼성전자 등 개별 종목)
    /// </summary>
    public async Task<KisQuoteOutput?> GetCurrentPriceAsync(string stockCode)
    {
        bool isVirtual = _settingsSvc.IsVirtual;
        string baseUrl = isVirtual ? VirtualUrl : RealUrl;
        var token = await _authService.GetAccessTokenAsync();

        string appKey = await _settingsSvc.GetKisAppKeyAsync(isVirtual);
        string appSecret = await _settingsSvc.GetKisSecretAsync(isVirtual);

        // 주식 현재가 전용 URL (FID_ORG_ADJ_PRC 넣으면 안 됨)
        var url = $"{baseUrl}/uapi/domestic-stock/v1/quotations/inquire-price?FID_COND_MRKT_DIV_CODE=J&FID_INPUT_ISCD={stockCode}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Add("authorization", $"Bearer {token}");
        request.Headers.Add("appkey", appKey);
        request.Headers.Add("appsecret", appSecret);
        request.Headers.Add("tr_id", "FHKST01010100"); // 💡 주식 현재가 전용 TR_ID
        request.Headers.Add("custtype", "P");

        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadFromJsonAsync<KisQuoteResponse>();
        return result?.ResultCode == "0" ? result.Output : null;
    }

    /// <summary>
    /// [3] 일별 시세 조회 (차트용 데이터)
    /// </summary>
    public async Task<List<KisDailyPriceDetail>?> GetDailyPriceAsync(string stockCode, string periodCode = "D")
    {
        bool isVirtual = _settingsSvc.IsVirtual;
        string baseUrl = isVirtual ? VirtualUrl : RealUrl;
        var token = await _authService.GetAccessTokenAsync();

        string appKey = await _settingsSvc.GetKisAppKeyAsync(isVirtual);
        string appSecret = await _settingsSvc.GetKisSecretAsync(isVirtual);

        // 💡 일별 시세는 반드시 FID_ORG_ADJ_PRC=0 (또는 1)이 포함되어야 합니다!
        var url = $"{baseUrl}/uapi/domestic-stock/v1/quotations/inquire-daily-price" +
                  $"?FID_COND_MRKT_DIV_CODE=J" +
                  $"&FID_INPUT_ISCD={stockCode}" +
                  $"&FID_PERIOD_DIV_CODE={periodCode}" +
                  $"&FID_ORG_ADJ_PRC=0"; // 💡 에러의 주범! 여기서 반드시 0을 넣어줘야 합니다.

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("authorization", $"Bearer {token}");
        request.Headers.Add("appkey", appKey);
        request.Headers.Add("appsecret", appSecret);
        request.Headers.Add("tr_id", "FHKST01010400"); // 💡 일별 시세 전용 TR_ID
        request.Headers.Add("custtype", "P");

        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadFromJsonAsync<KisDailyPriceResponse>();
        return result?.ResultCode == "0" ? result.Output : null;
    }

    #endregion

    #endregion
}