namespace SmartTrading.App.Models;

/// <summary>
/// 차트 그리기에 필요한 주식 캔들(OHLC) 데이터 모델.
/// (Open, High, Low, Close, Volume)
/// </summary>
public class StockData
{
    /// <summary>
    /// 거래 일자 및 시간 (X축)
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 시가 (장 시작 가격)
    /// </summary>
    public decimal Open { get; set; }

    /// <summary>
    /// 고가 (장중 최고가)
    /// </summary>
    public decimal High { get; set; }

    /// <summary>
    /// 저가 (장중 최저가)
    /// </summary>
    public decimal Low { get; set; }

    /// <summary>
    /// 종가 (장 마감 가격)
    /// </summary>
    public decimal Close { get; set; }

    /// <summary>
    /// 거래량 (선택 사항)
    /// </summary>
    public decimal Volume { get; set; }
}