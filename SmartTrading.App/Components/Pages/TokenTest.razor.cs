#region using directives

using Microsoft.AspNetCore.Components;

using SmartTrading.App.Services.Kis;

#endregion

namespace SmartTrading.App.Components.Pages;

/// <summary>
/// 토큰 테스트 페이지의 로직 (Code-Behind)
/// </summary>
public partial class TokenTest
{
    #region Injection

    /// <summary>
    /// KIS 인증 서비스 주입
    /// (@inject KisAuthService KisService 와 동일)
    /// </summary>
    [Inject]
    private KisAuthService KisService { get; set; } = default!;

    /// <summary>
    /// KIS 시세 조회 서비스 (현재가 조회용)
    /// </summary>
    [Inject]
    private KisQuoteService QuoteService { get; set; } = default!;

    #endregion

    #region Variable

    /// <summary>
    /// 발급된 접근 토큰 (화면 표시용)
    /// </summary>
    private string _accessToken = "";

    /// <summary>
    /// 조회된 주식 가격 정보 (화면 표시용)
    /// </summary>
    private string _stockPrice = "";

    /// <summary>
    /// 오류 메시지 저장 변수
    /// </summary>
    private string _errorMessage = "";

    /// <summary>
    /// 로딩 인디케이터 상태 (true: 로딩중)
    /// </summary>
    private bool _isLoading = false;

    #endregion

    #region Function

    /// <summary>
    /// 버튼 클릭 시 토큰 발급 요청
    /// </summary>
    private async Task OnGetTokenClick()
    {
        _isLoading = true;
        _errorMessage = "";
        _accessToken = "";

        try
        {
            // 서비스 호출
            _accessToken = await KisService.GetAccessTokenAsync();
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isLoading = false;
            // Blazor는 비동기 작업 후 UI 갱신을 위해 StateHasChanged()가 필요할 때가 있음
            StateHasChanged();
        }
    }

    /// <summary>
    /// [삼성전자 현재가 조회] 버튼 클릭 이벤트 핸들러 테스트
    /// </summary>
    /// <returns></returns>
    private async Task OnCheckSamsungPrice()
    {
        try
        {
            // 삼성전자 종목코드: 005930
            var result = await QuoteService.GetCurrentPriceAsync("005930");

            if (result != null)
            {
                // 보기 좋게 포맷팅 (예: 74,500원 (-0.5%))
                // int.Parse로 쉼표(N0) 포맷 적용
                if (int.TryParse(result.CurrentPrice, out int price))
                {
                    _stockPrice = $"{price:N0}원 ({result.ChangeRate}%)";
                }
                else
                {
                    _stockPrice = $"{result.CurrentPrice}원 ({result.ChangeRate}%)";
                }
            }
            else
            {
                _errorMessage = "시세 데이터를 가져오지 못했습니다.";
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"시세 조회 실패: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    #endregion
}