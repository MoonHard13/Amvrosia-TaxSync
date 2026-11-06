using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MoonHard.AmvrosiaTaxSync.Desktop.Logging;

public sealed class JsonFileLoggerProvider : ILoggerProvider
{
    private readonly object _gate = new();
    private readonly string _folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MoonHard", "AmvrosiaTaxSync", "Logs");

    public ILogger CreateLogger(string categoryName) => new JsonFileLogger(this, categoryName);

    public void Dispose() { }

    private void Write(string category, LogLevel level, string message, string? exception)
    {
        try
        {
            lock (_gate)
            {
                Directory.CreateDirectory(_folder);
                var path = Path.Combine(_folder, $"taxsync-{DateTime.UtcNow:yyyy-MM-dd}.jsonl");
                var line = JsonSerializer.Serialize(new
                {
                    timestampUtc = DateTimeOffset.UtcNow,
                    level = level.ToString(),
                    category,
                    message,
                    exceptionType = exception
                });
                File.AppendAllText(path, line + Environment.NewLine);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            System.Diagnostics.Debug.WriteLine($"Logging unavailable: {ex.GetType().Name}");
        }
    }

    private sealed class JsonFileLogger(JsonFileLoggerProvider owner, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            owner.Write(category, logLevel, formatter(state, exception), exception?.GetType().Name);
        }
    }
}
