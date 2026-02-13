#region using directives

using Microsoft.AspNetCore.Components;
using SmartTrading.App.Models;
using SmartTrading.App.Services.Common;
using SmartTrading.App.Services.Kis;
using System.Runtime.Versioning;

#endregion

namespace SmartTrading.App.Components.Pages;

/// <summary>
/// 메인 대시보드 페이지의 비하인드 로직을 담당하는 클래스입니다.
/// </summary>
[SupportedOSPlatform("Android21.0")]
[SupportedOSPlatform("iOS13.0")]
[SupportedOSPlatform("MacCatalyst13.0")]
[SupportedOSPlatform("windows10.0.17763.0")]
public partial class Home
{
    #region Injection

    /// <summary>
    /// 국내 및 지수 시세 조회를 위한 KIS 서비스
    /// </summary>
    [Inject] private KisQuoteService QuoteService { get; set; } = default!;

    /// <summary>
    /// 사용자 설정 관리 서비스 (보안 및 환경 설정)
    /// </summary>
    [Inject] protected SettingsService SettingsSvc { get; set; } = default!;

    #endregion

    #region Variable

    /// <summary>
    /// 코스피 지수 데이터 모델
    /// </summary>
    private KisIndexOutput? _kospi; // 💡 유지

    /// <summary>
    /// 코스닥 지수 데이터 모델
    /// </summary>
    private KisIndexOutput? _kosdaq; // 💡 유지

    #endregion

    #region Function

    /// <summary>
    /// 페이지 초기화 시 지수 데이터를 로드합니다.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // SettingsSvc의 실시간 설정값을 확인합니다.
        // 사용자가 설정 화면에서 바꾼 '모의투자' 여부에 따라 동작합니다.
        if (!SettingsSvc.IsVirtual)
        {
            try
            {
                // 1. 코스피(0001)와 코스닥(1001) 지수 조회를 병렬 실행
                var kospiTask = QuoteService.GetIndexPriceAsync("0001");
                var kosdaqTask = QuoteService.GetIndexPriceAsync("1001");

                // 2. 모든 비동기 작업 대기
                await Task.WhenAll(kospiTask, kosdaqTask);

                // 3. 결과값을 변수에 할당
                _kospi = await kospiTask;
                _kosdaq = await kosdaqTask;

                // 4. 데이터 로드 완료 후 UI 강제 갱신
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Dashboard 로딩 오류] {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 전일 대비 등락 구분 코드에 따라 화살표 기호를 반환
    /// </summary>
    private static string GetSign(string dir) => dir is "1" or "2" ? "▲" : dir is "4" or "5" ? "▼" : "";

    /// <summary>
    /// 전일 대비 등락 상태에 따라 UI에 적용할 CSS 클래스를 결정
    /// </summary>
    private static string GetColorClass(string dir) => dir is "1" or "2" ? "text-up" : dir is "4" or "5" ? "text-down" : "";

    #endregion
}