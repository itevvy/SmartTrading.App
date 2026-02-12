namespace SmartTrading.App.Utils;

public static class KeyStorage
{
    /// <summary>
    /// 값을 안전하게 저장.
    /// Default 속성을 통해 접근
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task SaveAsync(string key, string value) => await SecureStorage.Default.SetAsync(key, value);

    /// <summary>
    /// 저장된 값 가져오기.
    /// 값이 없으면 빈 문자열 반환
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static async Task<string> GetAsync(string key) => await SecureStorage.Default.GetAsync(key) ?? string.Empty;

    /// <summary>
    /// 키 삭제
    /// </summary>
    /// <param name="key"></param>
    public static void Remove(string key) => SecureStorage.Default.Remove(key);
}