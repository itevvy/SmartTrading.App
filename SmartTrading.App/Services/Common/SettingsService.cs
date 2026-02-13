#region using directives

using Microsoft.Maui.Storage; 
using System.Runtime.Versioning; 
using System.Diagnostics.CodeAnalysis;

#endregion

namespace SmartTrading.App.Services.Common;

/// <summary>
/// 앱의 전역 설정 및 보안이 필요한 API 키를 관리하는 서비스 클래스입니다.
/// 일반 설정은 Preferences를, 민감한 키는 하드웨어 암호화 저장소(SecureStorage)를 사용합니다.
/// </summary>
[SupportedOSPlatform("Android21.0")]
[SupportedOSPlatform("iOS13.0")]
[SupportedOSPlatform("MacCatalyst13.0")]
[SupportedOSPlatform("windows10.0.17763.0")]
public class SettingsService
{
    #region Constants (저장소 키 정의)

    /// <summary>
    /// 투자 환경 설정 키
    /// </summary>
    private const string IsVirtualKey = "user_is_virtual";

    /// <summary>
    /// 보안 키 명칭 (SecureStorage용)
    /// </summary>
    private const string GeminiApiKeyName = "gemini_api_key";
    private const string KisRealAppKeyName = "kis_real_appkey";
    private const string KisRealSecretName = "kis_real_secret";
    private const string KisVirtualAppKeyName = "kis_virtual_appkey";
    private const string KisVirtualSecretName = "kis_virtual_secret";

    /// <summary>
    /// 설정 변경 시 UI를 갱신하기 위한 이벤트입니다.
    /// </summary>
    public event Action? OnSettingsChanged;

    #endregion

    #region Properties (일반 설정)

    /// <summary>
    /// 모의투자 모드 활성화 여부를 가져오거나 설정
    /// DI 구조를 유지하기 위해 인스턴스 멤버로 두되 경고를 억제합니다.
    /// </summary>
    public bool IsVirtual
    {
        get => Preferences.Default.Get(IsVirtualKey, true);
        set
        {
            Preferences.Default.Set(IsVirtualKey, value);
            // 값이 바뀌면 등록된 모든 곳에 알림
            OnSettingsChanged?.Invoke();
        }
    }

    #endregion

    #region Public Methods (인스턴스 멤버 - 외부 호출용)

    /// <summary>
    /// 저장된 Gemini API 키를 반환
    /// </summary>
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public async Task<string> GetGeminiApiKeyAsync() => await GetSecureValueAsync(GeminiApiKeyName);

    /// <summary>
    /// 투자 모드에 따른 KIS AppKey를 반환
    /// </summary>
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public async Task<string> GetKisAppKeyAsync(bool isVirtual) => await GetSecureValueAsync(isVirtual ? KisVirtualAppKeyName : KisRealAppKeyName);

    /// <summary>
    /// 투자 모드에 따른 KIS AppSecret을 반환
    /// </summary>
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public async Task<string> GetKisSecretAsync(bool isVirtual) => await GetSecureValueAsync(isVirtual ? KisVirtualSecretName : KisRealSecretName); // 💡 오타 수정 완료

    #endregion

    #region Core Helpers (정적 멤버 - 내부 처리용)

    /// <summary> 
    /// 보안 저장소에서 값을 가져오는 핵심 헬퍼 (중복 정의 제거됨)
    /// </summary>
    public static async Task<string> GetSecureValueAsync(string key) => await SecureStorage.Default.GetAsync(key) ?? string.Empty;

    /// <summary>
    /// 보안 저장소에 값을 저장하거나 제거
    /// </summary>
    public static async Task SetSecureValueAsync(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            SecureStorage.Default.Remove(key);
        }
        else
        {
            await SecureStorage.Default.SetAsync(key, value);
        }
    }

    #endregion
}