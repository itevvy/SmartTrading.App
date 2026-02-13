#region using directives
using System.Net.Http.Json;
using System.Runtime.Versioning;

using Microsoft.Extensions.Configuration;

using SmartTrading.App.Models;
#endregion

namespace SmartTrading.App.Services.Kis;

/// <summary>
/// KIS 시세 조회 서비스: 지수, 현재가, 일별 시세
/// </summary>
[SupportedOSPlatform("Android21.0")]
[SupportedOSPlatform("iOS13.0")]
[SupportedOSPlatform("MacCatalyst13.0")]
[SupportedOSPlatform("windows10.0.17763.0")]
public class KisQuoteService(HttpClient httpClient, IConfiguration config, KisAuthService authService)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IConfiguration _config = config;
    private readonly KisAuthService _authService = authService;

    private const string RealUrl = "https://openapi.koreainvestment.com:9443";
    private const string VirtualUrl = "https://openapivts.koreainvestment.com:29443";

    /// <summary>
    /// [실전 전용] 시장 지수 조회 (모의는 미지원으로 Home에서 처리됨)
    /// </summary>
    public async Task<KisIndexOutput?> GetIndexPriceAsync(string indexCode)
    {
        try
        {
            // 지수는 무조건 실전 토큰과 실전 서버 사용
            var token = await _authService.GetAccessTokenAsync();
            var section = _config.GetSection("KisApi:Real");

            var request = new HttpRequestMessage(HttpMethod.Get, $"{RealUrl}/uapi/domestic-stock/v1/quotations/inquire-index-price?FID_COND_MRKT_DIV_CODE=U&FID_INPUT_ISCD={indexCode}");
            request.Headers.Add("authorization", $"Bearer {token}");
            request.Headers.Add("appkey", section["AppKey"]);
            request.Headers.Add("appsecret", section["AppSecret"]);
            request.Headers.Add("tr_id", "FHPST01010000");
            request.Headers.Add("custtype", "P");

            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadFromJsonAsync<KisIndexResponse>();
            return result?.ResultCode == "0" ? result.Output : null;
        }
        catch { return null; }
    }

    /// <summary>
    /// 종목 현재가 조회 (설정된 모드 자동 적용)
    /// </summary>
    public async Task<KisQuoteOutput?> GetCurrentPriceAsync(string stockCode)
    {
        bool isVirtual = _config.GetValue<bool>("KisApi:IsVirtual");
        string mode = isVirtual ? "Virtual" : "Real";
        string baseUrl = isVirtual ? VirtualUrl : RealUrl;

        var token = await _authService.GetAccessTokenAsync();
        var section = _config.GetSection($"KisApi:{mode}");

        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/uapi/domestic-stock/v1/quotations/inquire-price?FID_COND_MRKT_DIV_CODE=J&FID_INPUT_ISCD={stockCode}");
        request.Headers.Add("authorization", $"Bearer {token}");
        request.Headers.Add("appkey", section["AppKey"]);
        request.Headers.Add("appsecret", section["AppSecret"]);
        request.Headers.Add("tr_id", "FHKST01010100");
        request.Headers.Add("custtype", "P");

        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadFromJsonAsync<KisQuoteResponse>();
        return result?.ResultCode == "0" ? result.Output : null;
    }

    /// <summary>
    /// 일/주/월봉 조회 (차트용)
    /// </summary>
    public async Task<List<KisDailyPriceDetail>?> GetDailyPriceAsync(string stockCode, string periodCode = "D")
    {
        bool isVirtual = _config.GetValue<bool>("KisApi:IsVirtual");
        string mode = isVirtual ? "Virtual" : "Real";
        string baseUrl = isVirtual ? VirtualUrl : RealUrl;

        var token = await _authService.GetAccessTokenAsync();
        var section = _config.GetSection($"KisApi:{mode}");

        var url = $"{baseUrl}/uapi/domestic-stock/v1/quotations/inquire-daily-price?FID_COND_MRKT_DIV_CODE=J&FID_INPUT_ISCD={stockCode}&FID_PERIOD_DIV_CODE={periodCode}&FID_ORG_ADJ_PRC=0";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("authorization", $"Bearer {token}");
        request.Headers.Add("appkey", section["AppKey"]);
        request.Headers.Add("appsecret", section["AppSecret"]);
        request.Headers.Add("tr_id", "FHKST01010400");
        request.Headers.Add("custtype", "P");

        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadFromJsonAsync<KisDailyPriceResponse>();
        return result?.ResultCode == "0" ? result.Output : null;
    }
}