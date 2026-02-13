#region using directives

using Microsoft.AspNetCore.Components;

using SmartTrading.App.Models;
using SmartTrading.App.Services.Common;
using SmartTrading.App.Utils;

using System.Net.Http.Json;
using System.Runtime.Versioning;

#endregion

namespace SmartTrading.App.Services.Kis;

/// <summary>
/// KIS 인증 서비스: 보안 저장소에서 키를 읽어 접근 토큰을 발급 및 관리합니다.
/// </summary>
[SupportedOSPlatform("Android21.0")]
[SupportedOSPlatform("iOS13.0")]
[SupportedOSPlatform("MacCatalyst13.0")]
[SupportedOSPlatform("windows10.0.17763.0")]
public class KisAuthService(HttpClient httpClient, SettingsService settingsSvc)
{
    #region Variable & Constants

    private readonly HttpClient _httpClient = httpClient;

    /// <summary>
    /// 생성자(Primary Constructor)를 통해 주입된 서비스를 이 필드에 담아 사용
    /// </summary>
    private readonly SettingsService _settingsSvc = settingsSvc;

    private const string RealUrl = "https://openapi.koreainvestment.com:9443";
    private const string VirtualUrl = "https://openapivts.koreainvestment.com:29443";

    #endregion

    #region Public Methods

    /// <summary>
    /// 설정된 투자 모드(실전/모의)에 맞는 유효한 접근 토큰을 반환합니다.
    /// </summary>
    public async Task<string> GetAccessTokenAsync()
    {
        bool isVirtual = _settingsSvc.IsVirtual;
        string mode = isVirtual ? "virtual" : "real";

        var savedToken = await KeyStorage.GetAsync($"kis_token_{mode}");
        var expiryStr = await KeyStorage.GetAsync($"kis_expiry_{mode}");

        if (!string.IsNullOrEmpty(savedToken) && DateTime.TryParse(expiryStr, out var expiryTime))
        {
            if (expiryTime > DateTime.Now.AddMinutes(5))
            {
                return savedToken;
            }
        }

        return await IssueNewTokenAsync(isVirtual);
    }

    /// <summary>
    /// 웹소켓 접속을 위한 보안 승인키(Approval Key)를 발급받습니다.
    /// </summary>
    public async Task<string> GetWebSocketApprovalKeyAsync()
    {
        bool isVirtual = _settingsSvc.IsVirtual;
        string baseUrl = isVirtual ? VirtualUrl : RealUrl;

        string appKey = await _settingsSvc.GetKisAppKeyAsync(isVirtual);
        string appSecret = await _settingsSvc.GetKisSecretAsync(isVirtual);

        var requestBody = new
        {
            grant_type = "client_credentials",
            appkey = appKey,
            secretkey = appSecret
        };

        var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/oauth2/Approval", requestBody);
        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonNode>();

        return json?["approval_key"]?.ToString() ?? string.Empty;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// KIS 서버에 토큰 발급을 요청하고 결과를 로컬에 저장합니다.
    /// </summary>
    private async Task<string> IssueNewTokenAsync(bool isVirtual)
    {
        string baseUrl = isVirtual ? VirtualUrl : RealUrl;
        string modeKey = isVirtual ? "virtual" : "real";

        string appKey = await _settingsSvc.GetKisAppKeyAsync(isVirtual);
        string appSecret = await _settingsSvc.GetKisSecretAsync(isVirtual);

        if (string.IsNullOrEmpty(appKey) || string.IsNullOrEmpty(appSecret))
        {
            throw new Exception("API 키가 설정되지 않았습니다. 설정 메뉴에서 KIS API 키를 입력해주세요.");
        }

        var requestBody = new
        {
            grant_type = "client_credentials",
            appkey = appKey,
            appsecret = appSecret
        };

        var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/oauth2/tokenP", requestBody);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"토큰 발급 실패: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<KisTokenResponse>() ?? throw new Exception("응답 데이터 해석 실패");

        await KeyStorage.SaveAsync($"kis_token_{modeKey}", result.AccessToken);
        await KeyStorage.SaveAsync($"kis_expiry_{modeKey}", DateTime.Now.AddSeconds(result.ExpiresIn).ToString("yyyy-MM-dd HH:mm:ss"));

        return result.AccessToken;
    }

    #endregion
}