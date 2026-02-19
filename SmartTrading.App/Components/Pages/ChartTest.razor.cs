#region using directives

using ApexCharts;
using Microsoft.AspNetCore.Components;
using SmartTrading.App.Models;
using SmartTrading.App.Services.Kis;
using System.Globalization;
using System.Runtime.Versioning;

#endregion

namespace SmartTrading.App.Components.Pages;

/// <summary>
/// 주식 캔들 차트 테스트 페이지의 비하인드 로직 클래스입니다.
/// KIS API를 통해 수집된 실데이터를 ApexCharts를 이용해 시각화하며, 일/주/월봉 전환 기능을 제공하는 서비스
/// </summary>
/// <remarks>
/// [경고 해결] CA1416: 호출하는 KisQuoteService가 플랫폼별 SecureStorage를 사용하므로,
/// 해당 서비스를 의존하는 UI 페이지 역시 동일한 지원 플랫폼을 명시하여 빌드 경고를 제거했습니다.
/// </remarks>
[SupportedOSPlatform("Android21.0")]
[SupportedOSPlatform("iOS13.0")]
[SupportedOSPlatform("MacCatalyst13.0")]
[SupportedOSPlatform("windows10.0.17763.0")]
public partial class ChartTest
{
    #region Injection

    /// <summary>
    /// 한국투자증권 시세 조회 서비스 (Program.cs에 등록된 싱글톤/스코프 서비스)
    /// </summary>
    [Inject]
    private KisQuoteService QuoteService { get; set; } = default!;

    #endregion

    #region Variable

    /// <summary>
    /// ApexCharts의 전반적인 동작 및 디자인 설정 옵션 (축, 툴팁, 커스텀 포맷터 등)
    /// </summary>
    private ApexChartOptions<StockData> _options = new();

    /// <summary>
    /// 차트의 CandleSeries에 바인딩되는 실제 주가 데이터 리스트
    /// </summary>
    private List<StockData> _stockData = [];

    /// <summary>
    /// API 통신 중 화면에 로딩 스피너를 표시하기 위한 상태 변수
    /// </summary>
    private bool _isLoading = false;

    /// <summary>
    /// API 호출 실패나 데이터 부재 시 사용자에게 보여줄 안내 메시지
    /// </summary>
    private string _errorMessage = "";

    /// <summary>
    /// .razor 화면에 선언된 ApexChart 컴포넌트에 직접 접근하기 위한 참조 변수
    /// </summary>
    private ApexChart<StockData> _chart = default!;

    /// <summary>
    /// 현재 조회 중인 차트의 주기 (D: 일봉, W: 주봉, M: 월봉)
    /// </summary>
    private string _currentPeriod = "D";

    /// <summary>
    /// 차트 상단 제목 영역에 표시될 텍스트
    /// </summary>
    private string _chartTitle = "일봉 차트";

    #endregion

    #region Function

    /// <summary>
    /// Blazor 생명주기 메서드로, 페이지 로드 시 차트 옵션을 초기화하고 기본 데이터를 로드합니다.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // 1. ApexCharts 상세 옵션 빌드
        _options = new ApexChartOptions<StockData>
        {
            Chart = new Chart
            {
                Id = "stock_chart",
                Toolbar = new ApexCharts.Toolbar
                {
                    Show = true,
                    // 사용자 혼동 방지를 위해 범위 이탈 가능성이 있는 줌인/아웃 버튼은 제외
                    Tools = new Tools { Zoomin = false, Zoomout = false, Pan = true, Reset = true }
                },
                // 차트 확대 시 가격대(Y축)가 자동으로 현재 화면에 맞게 최적화되도록 설정
                Zoom = new Zoom { Enabled = true, AutoScaleYaxis = true }
            },
            Xaxis = new XAxis
            {
                // Category 타입을 사용해 주말/공휴일 등 데이터가 없는 날의 여백을 제거
                Type = XAxisType.Category,
                Labels = new XAxisLabels
                {
                    // 축 라벨 가독성을 위해 "2026-02-13" -> "26-02-13"으로 앞 2자리 절삭
                    Formatter = @"function(value) { return value ? value.substring(2) : value; }",
                    Rotate = -45,
                    HideOverlappingLabels = true
                },
                Tooltip = new AxisTooltip { Enabled = false }
            },
            Yaxis =
            [
                new()
                {
                    // 우측 가격 축에 천 단위 콤마(,) 포맷터 적용 (index.html 정의 함수 호출)
                    Labels = new YAxisLabels { Formatter = "window.formatNumber" },
                    Tooltip = new YAxisTooltip { Enabled = true }
                }
            ],
            // 마우스 오버 시 표시되는 툴팁을 HTML로 직접 구성하여 디자인 제어
            Tooltip = new Tooltip { Enabled = true, Custom = "window.customTooltip" },
            PlotOptions = new PlotOptions
            {
                Candlestick = new PlotOptionsCandlestick
                {
                    // 국내 증권 시장 표준 색상 적용 (양봉: 빨강, 음봉: 파랑)
                    Colors = new PlotOptionsCandlestickColors { Upward = "#FF0000", Downward = "#0000FF" }
                }
            }
        };

        // 2. 초기 로드 시 삼성전자(005930) 일봉 데이터를 기본으로 표시
        await LoadRealDataAsync("005930", "D");
    }

    /// <summary>
    /// KIS 서비스를 통해 원본 데이터를 가져온 뒤, 차트 라이브러리가 인식할 수 있는 StockData 모델로 변환합니다.
    /// </summary>
    /// <param name="code">종목코드 (6자리)</param>
    /// <param name="period">기간코드 (D, W, M)</param>
    private async Task LoadRealDataAsync(string code, string period)
    {
        _isLoading = true;
        _errorMessage = "";
        _stockData = [];

        try
        {
            // API로부터 일별 시세 내역 수신
            var result = await QuoteService.GetDailyPriceAsync(code, period);

            if (result != null && result.Count > 0)
            {
                foreach (var item in result)
                {
                    // KIS 날짜 문자열(yyyyMMdd)을 DateTime으로 안전하게 파싱
                    if (DateTime.TryParseExact(item.Date, "yyyyMMdd", null, DateTimeStyles.None, out DateTime date))
                    {
                        _stockData.Add(new StockData
                        {
                            Date = date,
                            Open = decimal.Parse(item.Open),
                            High = decimal.Parse(item.High),
                            Low = decimal.Parse(item.Low),
                            Close = decimal.Parse(item.Close),
                            Volume = decimal.Parse(item.Volume)
                        });
                    }
                }
                // 최신 날짜가 우측으로 오도록 오름차순 정렬하여 데이터 바인딩
                _stockData = [.. _stockData.OrderBy(x => x.Date)];
            }
            else
            {
                _errorMessage = "데이터 수신 결과가 비어있습니다. 토큰이나 장 마감 여부를 확인하세요.";
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"시스템 오류: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    /// <summary>
    /// 상단 기간 선택 버튼 클릭 시 호출되어 차트의 데이터 주기를 변경하고 화면을 갱신합니다.
    /// </summary>
    /// <param name="period">선택된 기간 코드 (D, W, M)</param>
    private async Task ChangePeriod(string period)
    {
        if (_currentPeriod == period) return;

        _currentPeriod = period;
        _chartTitle = period switch
        {
            "D" => "일봉 차트",
            "W" => "주봉 차트",
            "M" => "월봉 차트",
            _ => "차트"
        };

        // 새 기간 데이터 로드
        await LoadRealDataAsync("005930", period);

        // 차트 컴포넌트가 데이터를 인지하고 다시 그리도록 명시적 호출
        if (_chart != null)
        {
            await _chart.RenderAsync();
        }
    }

    /// <summary>
    /// 현재 활성화된 기간 버튼에 강조색(Primary)을 적용하기 위한 부트스트랩 클래스 반환 메서드입니다.
    /// </summary>
    /// <param name="period">버튼의 기준 기간</param>
    private string GetButtonClass(string period) => _currentPeriod == period ? "btn-primary" : "btn-outline-primary";


    #endregion
}