#region using directives

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Runtime.Versioning;

using Microsoft.Extensions.Configuration;

#endregion

namespace SmartTrading.App.Services.Kis;

/// <summary>
/// 한국투자증권(KIS) 실시간 시세 수신 서비스
/// </summary>
[SupportedOSPlatform("Android21.0")]
[SupportedOSPlatform("iOS13.0")]
[SupportedOSPlatform("MacCatalyst13.0")]
[SupportedOSPlatform("windows10.0.17763.0")]
public class KisRealtimeService(IConfiguration config, KisAuthService authService)
{
    #region Variable & Constants

    private readonly IConfiguration _config = config;
    private readonly KisAuthService _authService = authService;

    private ClientWebSocket _webSocket = new();
    private CancellationTokenSource _cts = new();

    /// <summary>
    /// [수정] 실전투자 웹소켓 서버 URL (표준: 9443 포트)
    /// </summary>
    private const string RealWsUrl = "ws://ws-api.koreainvestment.com:9443";

    /// <summary>
    /// [수정] 모의투자 웹소켓 서버 URL (표준: 21000 포트)
    /// </summary>
    private const string VirtualWsUrl = "ws://ops.koreainvestment.com:21000";

    public event Action<string>? OnDataReceived;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = false
    };

    #endregion

    #region Public Methods

    /// <summary>
    /// 설정된 투자 모드에 맞춰 웹소켓 연결을 시작합니다.
    /// </summary>
    public async Task ConnectAsync()
    {
        if (_webSocket.State == WebSocketState.Open) return;

        // 1. 설정값(IsVirtual)에 따라 정확한 웹소켓 서버 주소 선택
        var isVirtual = _config.GetValue<bool>("KisApi:IsVirtual");
        var wsUrl = isVirtual ? VirtualWsUrl : RealWsUrl;

        _webSocket = new ClientWebSocket();
        _cts = new CancellationTokenSource();

        try
        {
            System.Diagnostics.Debug.WriteLine($"[WS] 접속 시도: {wsUrl}");
            await _webSocket.ConnectAsync(new Uri(wsUrl), _cts.Token);
            System.Diagnostics.Debug.WriteLine("[WS] 서버 연결 성공!");

            _ = Task.Run(ReceiveLoopAsync, _cts.Token);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WS Error] 연결 실패: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 종목 코드를 구독합니다. (H0STCNT0: 실시간 체결가 사용 권장)
    /// </summary>
    public async Task SubscribeAsync(string stockCode)
    {
        if (_webSocket.State != WebSocketState.Open)
        {
            await ConnectAsync();
        }

        // 2. 현재 모드에 맞는 승인키(Approval Key) 확보
        var approvalKey = await _authService.GetWebSocketApprovalKeyAsync();

        var request = new
        {
            header = new
            {
                approval_key = approvalKey,
                custtype = "P",
                tr_type = "1", // 1: 등록
                content_type = "utf-8"
            },
            body = new
            {
                input = new
                {
                    // 💡 실시간 '현재가(체결)'를 보려면 H0STCNT0를 사용
                    // 기존 H0STASP0는 '호가' 데이터
                    tr_id = "H0STCNT0",
                    tr_key = stockCode
                }
            }
        };

        var jsonString = JsonSerializer.Serialize(request, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(jsonString);

        await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts.Token);
        System.Diagnostics.Debug.WriteLine($"[WS] 구독 요청: {stockCode}");
    }

    public async Task DisconnectAsync()
    {
        _cts.Cancel();
        if (_webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "User Disconnect", CancellationToken.None);
        }
    }

    #endregion

    #region Private Methods

    private async Task ReceiveLoopAsync()
    {
        var buffer = new byte[4096 * 4];
        try
        {
            while (_webSocket.State == WebSocketState.Open && !_cts.IsCancellationRequested)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                if (result.EndOfMessage)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var parsedPrice = ParsePriceFromRawData(message);
                    OnDataReceived?.Invoke(parsedPrice);
                }
            }
        }
        catch { /* 종료 처리 */ }
    }

    private static string ParsePriceFromRawData(string message)
    {
        try
        {
            // 1. JSON 응답(구독 결과 등) 처리
            if (message.StartsWith('{'))
            {
                using var jsonDoc = JsonDocument.Parse(message);
                var root = jsonDoc.RootElement;

                // PINGPONG 메시지는 무시
                if (root.GetProperty("header").GetProperty("tr_id").GetString() == "PINGPONG")
                {
                    return "PING";
                }

                // 구독 응답 결과 확인
                if (root.TryGetProperty("body", out var body))
                {
                    var rtCd = body.GetProperty("rt_cd").GetString();
                    var msg = body.GetProperty("msg1").GetString();

                    // rt_cd가 "0"이면 성공, 아니면 실패 메시지 출력
                    return rtCd == "0" ? "✅ 실시간 연결 성공" : $"❌ 구독 실패: {msg}";
                }
            }

            // 2. 실시간 시세 데이터(구분자 '|' 방식) 처리
            var parts = message.Split('|');
            if (parts.Length >= 4)
            {
                var realData = parts[3];
                var values = realData.Split('^');

                // H0STCNT0(체결) 기준: 2번 인덱스가 현재가
                if (values.Length > 2)
                {
                    return values[2];
                }
            }

            return message;
        }
        catch
        {
            return "데이터 해석 오류";
        }
    }

    #endregion
}