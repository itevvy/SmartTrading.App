#region using directives

using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using SmartTrading.App.Models;

#endregion

namespace SmartTrading.App.Services.Kis;

/// <summary>
/// 주식 시세 조회 서비스
/// </summary>
public class KisQuoteService(HttpClient httpClient, IConfiguration config, KisAuthService authService)
{
    #region Variable

    private readonly HttpClient _httpClient = httpClient;
    private readonly IConfiguration _config = config;
    private readonly KisAuthService _authService = authService;

    private const string RealUrl = "https://openapi.koreainvestment.com:9443";
    private const string VirtualUrl = "https://openapivts.koreainvestment.com:29443";

    #endregion

    #region public Function

    /// <summary>
    /// 특정 종목의 현재가를 조회합니다.
    /// </summary>
    /// <param name="stockCode">종목코드 (예: 005930)</param>
    public async Task<KisQuoteOutput?> GetCurrentPriceAsync(string stockCode)
    {
        // 1. 토큰 확보 (없으면 알아서 발급받음)
        var token = await _authService.GetAccessTokenAsync();
        var appKey = _config["KisApi:AppKey"];
        var appSecret = _config["KisApi:AppSecret"];
        var isVirtual = _config.GetValue<bool>("KisApi:IsVirtual");
        var baseUrl = isVirtual ? VirtualUrl : RealUrl;

        // 2. 헤더 설정 (한투 API 필수 요구사항)
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("authorization", $"Bearer {token}");
        _httpClient.DefaultRequestHeaders.Add("appkey", appKey);
        _httpClient.DefaultRequestHeaders.Add("appsecret", appSecret);
        _httpClient.DefaultRequestHeaders.Add("tr_id", isVirtual ? "FHKST01010100" : "FHKST01010100"); // 현재가 조회 TR ID
        _httpClient.DefaultRequestHeaders.Add("custtype", "P"); // 개인

        // 3. API 호출
        // URL: /uapi/domestic-stock/v1/quotations/inquire-price
        // 파라미터: FID_COND_MRKT_DIV_CODE=J(주식), FID_INPUT_ISCD=종목코드
        var url = $"{baseUrl}/uapi/domestic-stock/v1/quotations/inquire-price?FID_COND_MRKT_DIV_CODE=J&FID_INPUT_ISCD={stockCode}";

        var response = await _httpClient.GetFromJsonAsync<KisQuoteResponse>(url);

        if (response != null && response.ResultCode == "0" && response.Output != null)
        {
            return response.Output;
        }

        // 실패 시 로그
        Console.WriteLine($"[Quote Error] {response?.Message}");
        return null;
    }

    #endregion
}