#region using directives

using Microsoft.AspNetCore.Components;
using SmartTrading.App.Services.Common;

#endregion

namespace SmartTrading.App.Components.Layout;

/// <summary>
/// 메인 레이아웃: 상단 상태바의 투자 모드 배지를 관리하고 설정 변경 이벤트를 구독합니다.
/// </summary>
public partial class MainLayout : IDisposable
{
    #region Injection

    [Inject] private SettingsService SettingsSvc { get; set; } = default!;

    #endregion

    #region Lifecycle

    protected override void OnInitialized()
    {
        // 설정 서비스의 변경 이벤트를 구독
        SettingsSvc.OnSettingsChanged += HandleSettingsChanged;
    }

    /// <summary>
    /// 컴포넌트 소멸 시 호출
    /// </summary>
    public void Dispose()
    {
        // 1. 관리되는 리소스(이벤트 구독 등) 해제
        SettingsSvc.OnSettingsChanged -= HandleSettingsChanged;

        // 💡 2. [해결] GC에게 이 객체의 파이널라이저를 호출할 필요가 없음을 알립니다.
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Event Handler

    private void HandleSettingsChanged()
    {
        // 설정이 변경되면 UI 스레드에서 화면 갱신
        InvokeAsync(StateHasChanged);
    }

    #endregion
}