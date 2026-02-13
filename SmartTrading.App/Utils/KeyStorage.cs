#region using directives

using System.Runtime.Versioning;

#endregion

namespace SmartTrading.App.Utils;

/// <summary>
/// 기기의 보안 저장소(SecureStorage)를 사용하여 민감한 데이터를 안전하게 관리하는 유틸리티 클래스입니다.
/// </summary>
/// <remarks>
/// CA1416: 특정 플랫폼 버전에서만 지원되는 SecureStorage API 호출에 따른 
/// 플랫폼 호환성 경고를 어트리뷰트 선언을 통해 해결
/// </remarks>
[SupportedOSPlatform("Android21.0")]
[SupportedOSPlatform("iOS13.0")]
[SupportedOSPlatform("MacCatalyst13.0")]
[SupportedOSPlatform("windows10.0.17763.0")]
public static class KeyStorage
{
    /// <summary>
    /// 지정된 키(Key)를 사용하여 값을 보안 저장소에 비동기적으로 저장합니다.
    /// </summary>
    /// <param name="key">저장할 데이터의 식별자</param>
    /// <param name="value">저장할 문자열 값</param>
    /// <returns>비동기 작업 태스크</returns>
    public static async Task SaveAsync(string key, string value) => await SecureStorage.Default.SetAsync(key, value);

    /// <summary>
    /// 보안 저장소로부터 지정된 키(Key)에 해당하는 값을 비동기적으로 가져옵니다.
    /// </summary>
    /// <param name="key">찾으려는 데이터의 식별자</param>
    /// <returns>저장된 값. 데이터가 없으면 빈 문자열(string.Empty)을 반환</returns>
    public static async Task<string> GetAsync(string key) => await SecureStorage.Default.GetAsync(key) ?? string.Empty;

    /// <summary>
    /// 보안 저장소에서 특정 키(Key)와 연결된 데이터를 삭제
    /// </summary>
    /// <param name="key">삭제할 데이터의 식별자</param>
    public static void Remove(string key) => SecureStorage.Default.Remove(key);
}