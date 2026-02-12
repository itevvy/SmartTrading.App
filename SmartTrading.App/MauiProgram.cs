#region using Directives

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using SmartTrading.App.Services.Kis;

#endregion

namespace SmartTrading.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts => fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"));

        // ---------------------------------------------------------
        // appsettings.json 설정 파일 로드
        // 앱 안에 숨겨진(Embedded) json 파일을 읽어서 Configuration에 주입
        // ---------------------------------------------------------
        var assembly = Assembly.GetExecutingAssembly();

        // 주의: 파일명 앞의 네임스페이스(SmartTrading.App)가 정확해야 함
        using var stream = assembly.GetManifestResourceStream("SmartTrading.App.appsettings.json");

        if (stream != null)
        {
            builder.Configuration.AddConfiguration(new ConfigurationBuilder().AddJsonStream(stream).Build());
        }
        else
        {
            // 파일을 못 찾으면 개발자가 바로 알 수 있게 로그 출력
            Console.WriteLine("⚠️ 경고: appsettings.json 파일을 찾을 수 없습니다. 빌드 작업이 '포함 리소스(Embedded Resource)'인지 확인하세요.");
        }

        // ---------------------------------------------------------
        // 서비스 등록 (Dependency Injection)
        // HttpClient를 사용하는 KisAuthService를 싱글톤(또는 Scoped)처럼 등록
        // ---------------------------------------------------------
        builder.Services.AddHttpClient<KisAuthService>();
        builder.Services.AddHttpClient<KisQuoteService>();

        // MAUI Blazor 필수 서비스
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}