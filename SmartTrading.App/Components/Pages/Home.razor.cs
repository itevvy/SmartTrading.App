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
    private KisIndexOutput? _kospi;

    /// <summary>
    /// 코스닥 지수 데이터 모델
    /// </summary>
    private KisIndexOutput? _kosdaq;

    private KisQuoteOutput? _featuredStock;
    private readonly string _featuredStockCode = "005930"; // 삼성전자 (샘플)

    #endregion

    #region Function

    /// <summary>
    /// 페이지 초기화 시 지수 데이터를 로드합니다.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        try
        {
            List<Task> tasks = [];

            // 1. [실전 모드] 지수 데이터 호출 (모의투자는 미지원으로 skip됨)
            if (!SettingsSvc.IsVirtual)
            {
                tasks.Add(LoadIndexAsync());
            }

            // 2. [공통] 관심 종목 현재가 호출 (모의/실전 모두 가능)
            tasks.Add(LoadFeaturedStockAsync());

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[초기화 에러] {ex.Message}");
        }

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// 지수 데이터 로드
    /// </summary>
    private async Task LoadIndexAsync()
    {
        _kospi = await QuoteService.GetIndexPriceAsync("0001");
        _kosdaq = await QuoteService.GetIndexPriceAsync("1001");
    }

    /// <summary>
    /// 관심 종목 데이터 로드
    /// </summary>
    private async Task LoadFeaturedStockAsync()
    {
        // SettingsService의 IsVirtual 설정에 맞춰 KisQuoteService가 알아서 URL을 바꿉니다.
        _featuredStock = await QuoteService.GetCurrentPriceAsync(_featuredStockCode);
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