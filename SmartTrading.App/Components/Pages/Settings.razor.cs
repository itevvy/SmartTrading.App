#region using directives

using Microsoft.AspNetCore.Components;
using SmartTrading.App.Services.Common;

#endregion

namespace SmartTrading.App.Components.Pages;

public partial class Settings
{
    #region Injection

    [Inject] private SettingsService SettingsSvc { get; set; } = default!;

    [Inject] private NavigationManager Nav { get; set; } = default!;

    #endregion

    #region Variables

    private bool _isVirtual;
    private string _geminiKey = "";
    private string _kisRealAppKey = "";
    private string _kisRealSecret = "";
    private string _kisVirtualAppKey = "";
    private string _kisVirtualSecret = "";

    // 임시 변수
    private bool _isVirtualTemp;
    private string _geminiKeyTemp = "";

    #endregion

    #region Lifecycle

    protected override async Task OnInitializedAsync()
    {
        // 처음 로드할 때만 서비스의 값을 임시 변수에 복사
        _isVirtualTemp = SettingsSvc.IsVirtual;
        _geminiKeyTemp = await SettingsSvc.GetGeminiApiKeyAsync();

        _isVirtual = SettingsSvc.IsVirtual;
        _geminiKey = await SettingsSvc.GetGeminiApiKeyAsync();

        _kisRealAppKey = await SettingsSvc.GetKisAppKeyAsync(false);
        _kisRealSecret = await SettingsSvc.GetKisSecretAsync(false);
        _kisVirtualAppKey = await SettingsSvc.GetKisAppKeyAsync(true);
        _kisVirtualSecret = await SettingsSvc.GetKisSecretAsync(true);
    }

    #endregion

    #region Function

    private async Task SaveSettings()
    {
        // [저장] 버튼을 눌러야만 실제 서비스(메모리 및 저장소)에 반영됩니다.
        SettingsSvc.IsVirtual = _isVirtualTemp;
        await SettingsService.SetSecureValueAsync("gemini_api_key", _geminiKeyTemp);

        SettingsSvc.IsVirtual = _isVirtual;
        await SettingsService.SetSecureValueAsync("gemini_api_key", _geminiKey);
        await SettingsService.SetSecureValueAsync("kis_real_appkey", _kisRealAppKey);
        await SettingsService.SetSecureValueAsync("kis_real_secret", _kisRealSecret);
        await SettingsService.SetSecureValueAsync("kis_virtual_appkey", _kisVirtualAppKey);
        await SettingsService.SetSecureValueAsync("kis_virtual_secret", _kisVirtualSecret);

        Nav.NavigateTo("/", true);
    }

    #endregion
}