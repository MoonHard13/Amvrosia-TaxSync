using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MoonHard.AmvrosiaTaxSync.Desktop.Services;

public sealed record ConnectionPreferences(
    string Server = "",
    string Database = "",
    bool WindowsAuthentication = true,
    string Username = "",
    int Port = 0,
    int TimeoutSeconds = 15);

public sealed class ConnectionPreferencesStore(ILogger<ConnectionPreferencesStore> logger)
{
    private static readonly string DirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MoonHard", "AmvrosiaTaxSync");

    private static readonly string FilePath = Path.Combine(DirectoryPath, "settings.json");

    public ConnectionPreferences Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return new ConnectionPreferences();

            return JsonSerializer.Deserialize<ConnectionPreferences>(File.ReadAllText(FilePath))
                ?? new ConnectionPreferences();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            logger.LogWarning("Non-secret settings load failed: {Type}.", ex.GetType().Name);
            return new ConnectionPreferences();
        }
    }

    public void Save(ConnectionPreferences preferences)
    {
        Directory.CreateDirectory(DirectoryPath);
        var temporaryFile = FilePath + ".tmp";
        try
        {
            File.WriteAllText(temporaryFile, JsonSerializer.Serialize(preferences,
                new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporaryFile, FilePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryFile))
                File.Delete(temporaryFile);
        }

        logger.LogInformation("Non-secret connection preferences saved.");
    }
}
