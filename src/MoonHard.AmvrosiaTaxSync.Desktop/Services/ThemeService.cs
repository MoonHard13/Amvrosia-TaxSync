using System.Windows;
using Microsoft.Extensions.Logging;

namespace MoonHard.AmvrosiaTaxSync.Desktop.Services;

public sealed class ThemeService(ILogger<ThemeService> logger)
{
    public string CurrentTheme { get; private set; } = "Dark";

    public void Apply(string theme)
    {
        var selected = theme == "Light" ? "Light" : "Dark";
        var dictionaries = System.Windows.Application.Current.Resources.MergedDictionaries;
        var current = dictionaries.FirstOrDefault(dictionary =>
            dictionary.Source?.OriginalString.Contains("Themes/", StringComparison.OrdinalIgnoreCase) == true);

        if (current is not null &&
            current.Source?.OriginalString.EndsWith($"/{selected}.xaml", StringComparison.OrdinalIgnoreCase) == true)
        {
            CurrentTheme = selected;
            return;
        }

        var replacement = new ResourceDictionary
        {
            Source = new Uri($"pack://application:,,,/Themes/{selected}.xaml", UriKind.Absolute)
        };

        if (current is not null)
            dictionaries.Remove(current);

        dictionaries.Insert(0, replacement);
        CurrentTheme = selected;
        logger.LogInformation("User interface theme applied: {Theme}.", selected);
    }
}
