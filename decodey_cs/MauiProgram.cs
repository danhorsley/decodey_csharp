using Microsoft.Extensions.Logging;
using Decodey.Services;
using Decodey.ViewModels;
using Decodey.Views;
using Decodey.Converters;

namespace Decodey;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register services
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IGameService, GameService>();

        // Register view models
        builder.Services.AddTransient<GameViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Register pages
        builder.Services.AddTransient<GamePage>();
        builder.Services.AddTransient<AboutDialog>();

        // Configure app
        ConfigureResources(builder.Services);

        // Configure logging
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void ConfigureResources(IServiceCollection services)
    {
        // Add converters to application resources
        Application.Current.Resources.Add("LetterStateConverter", new LetterStateConverter());
        Application.Current.Resources.Add("GuessStateConverter", new GuessStateConverter());
        Application.Current.Resources.Add("FrequencyConverter", new FrequencyConverter());
        Application.Current.Resources.Add("RemainingMistakesConverter", new RemainingMistakesConverter());
        Application.Current.Resources.Add("StringNotEmptyConverter", new StringNotEmptyConverter());
    }
}

// String converters
public class StringNotEmptyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !string.IsNullOrEmpty(value as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}