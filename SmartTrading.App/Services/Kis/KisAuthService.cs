#region using directives

using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using SmartTrading.App.Models;
using SmartTrading.App.Utils;

#endregion

namespace SmartTrading.App.Services.Kis;

/// <summary>
/// 한국투자증권(KIS) 인증 관련 로직을 담당하는 서비스.
/// 접근 토큰(Access Token)의 발급, 저장, 갱신을 관리함.
/// (Primary Constructor 적용됨)
/// </summary>
public class KisAuthService(HttpClient httpClient, IConfiguration config)
{
    #region Variable

    /// <summary>
    /// HTTP 통신 클라이언트
    /// </summary>
    private readonly HttpClient _httpClient = httpClient;
    
    /// <summary>
    /// 설정 파일 접근 객체
    /// </summary>
    private readonly IConfiguration _config = config;

    /// <summary>
    ///  실전투자(Real) URL
    /// </summary>
    private const string RealUrl = "https://openapi.koreainvestment.com:9443";

    /// <summary>
    /// 모의투자(Virtual) URL
    /// </summary>
    private const string VirtualUrl = "https://openapivts.koreainvestment.com:29443";

    #endregion

    #region public Function

    /// <summary>
    /// 유효한 접근 토큰을 반환함.
    /// 1. 저장소(SecureStorage)에 있는 토큰을 먼저 확인.
    /// 2. 없으면 서버에 요청해서 새로 발급.
    /// </summary>
    /// <returns>Bearer Access Token</returns>
    public async Task<string> GetAccessTokenAsync()
    {
        // 1. 저장된 토큰이 있는지 먼저 확인 (매번 요청하면 차단당함)
        var savedToken = await KeyStorage.GetAsync("kis_access_token");

        if (!string.IsNullOrEmpty(savedToken))
        {
            return savedToken;
        }

        // 2. 없으면 새로 발급 요청
        return await IssueNewTokenAsync();
    }

    /// <summary>
    /// 실제 한투 서버에 접속하여 토큰을 발급받고 내부 저장소에 저장함.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<string> IssueNewTokenAsync()
    {
        var appKey = _config["KisApi:AppKey"];
        var appSecret = _config["KisApi:AppSecret"];

        // 설정값 검증
        if (string.IsNullOrEmpty(appKey) || string.IsNullOrEmpty(appSecret))
        {
            throw new Exception("API Key가 설정되지 않았습니다. appsettings.json을 확인하세요.");
        }

        var isVirtual = _config.GetValue<bool>("KisApi:IsVirtual");
        var baseUrl = isVirtual ? VirtualUrl : RealUrl;

        // 요청 바디 생성
        var requestBody = new
        {
            grant_type = "client_credentials",
            appkey = appKey,
            appsecret = appSecret
        };

        try
        {
            // 3. API 호출 (POST)
            // tokenP는 접근토큰 발급용 경로
            var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/oauth2/tokenP", requestBody);

            // 4. 응답 실패 처리 (200 OK가 아닐 경우)
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"[KIS API 오류] 토큰 발급 실패. 상태코드: {response.StatusCode}, 내용: {errorMsg}");
            }

            // 5. 응답 파싱 및 검증
            var result = await response.Content.ReadFromJsonAsync<KisTokenResponse>();
            if (result == null || string.IsNullOrEmpty(result.AccessToken))
            {
                throw new Exception("[KIS API 오류] 응답은 성공했으나 토큰 데이터가 비어있습니다.");
            }

            // 6. 중요: 발급받은 토큰을 기기 내부에 암호화하여 저장
            // 앱을 껐다 켜도 다시 로그인할 필요 없게 만듦
            await KeyStorage.SaveAsync("kis_access_token", result.AccessToken);

            Console.WriteLine($"[알림] 토큰 신규 발급 완료: {result.AccessToken[..10]}...");

            return result.AccessToken;
        }
        catch (Exception ex)
        {
            // 디버깅을 위해 콘솔에 에러 출력 후 상위로 던짐
            Console.WriteLine($"[치명적 오류] IssueNewTokenAsync: {ex.Message}");
            throw;
        }
    }

    #endregion
}