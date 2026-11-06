using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using MoonHard.AmvrosiaTaxSync.Application.Contracts;
using MoonHard.AmvrosiaTaxSync.Desktop.Services;
using MoonHard.AmvrosiaTaxSync.Domain.Models;

namespace MoonHard.AmvrosiaTaxSync.Desktop.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly ISqlSchemaService _service;
    private readonly ConnectionPreferencesStore _store;
    private readonly ILogger<MainViewModel> _logger;
    private CancellationTokenSource? _cancellation;
    private string _server = "";
    private string _database = "";
    private string _port = "0";
    private string _timeout = "15";
    private string _username = "";
    private bool _windowsAuthentication = true;
    private bool _isBusy;
    private string _status = "Συμπλήρωσε στοιχεία σύνδεσης για να ξεκινήσεις.";
    private SchemaObject? _selectedObject;

    public MainViewModel(
        ISqlSchemaService service,
        ConnectionPreferencesStore store,
        ILogger<MainViewModel> logger)
    {
        _service = service;
        _store = store;
        _logger = logger;
        var preferences = store.Load();
        _server = preferences.Server;
        _database = preferences.Database;
        _port = preferences.Port.ToString();
        _timeout = preferences.TimeoutSeconds.ToString();
        _windowsAuthentication = preferences.WindowsAuthentication;
        _username = preferences.Username;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<SchemaObject> Objects { get; } = [];
    public ObservableCollection<SchemaColumn> SelectedColumns { get; } = [];

    public string Server { get => _server; set => Set(ref _server, value); }
    public string Database { get => _database; set => Set(ref _database, value); }
    public string Port { get => _port; set => Set(ref _port, value); }
    public string Timeout { get => _timeout; set => Set(ref _timeout, value); }
    public string Username { get => _username; set => Set(ref _username, value); }

    public bool WindowsAuthentication
    {
        get => _windowsAuthentication;
        set
        {
            if (Set(ref _windowsAuthentication, value))
                Changed(nameof(IsSqlAuthentication));
        }
    }

    public bool IsSqlAuthentication => !WindowsAuthentication;

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (Set(ref _isBusy, value))
                Changed(nameof(IsIdle));
        }
    }

    public bool IsIdle => !IsBusy;

    public string Status
    {
        get => _status;
        private set => Set(ref _status, value);
    }

    public SchemaObject? SelectedObject
    {
        get => _selectedObject;
        set
        {
            if (!Set(ref _selectedObject, value))
                return;

            SelectedColumns.Clear();
            if (value is null)
                return;

            foreach (var column in value.Columns)
                SelectedColumns.Add(column);
        }
    }

    public Task TestConnectionAsync(string password)
        => ExecuteAsync(password, inspect: false);

    public Task InspectSchemaAsync(string password)
        => ExecuteAsync(password, inspect: true);

    public void Cancel() => _cancellation?.Cancel();

    public void SavePreferences()
    {
        try
        {
            _store.Save(new ConnectionPreferences(
                Server, Database, WindowsAuthentication, Username,
                int.TryParse(Port, out var port) ? port : 0,
                int.TryParse(Timeout, out var timeout) ? timeout : 15));
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to save non-secret preferences: {Type}.", ex.GetType().Name);
        }
    }

    private async Task ExecuteAsync(string password, bool inspect)
    {
        if (IsBusy)
            return;

        IsBusy = true;
        using var cancellation = new CancellationTokenSource();
        _cancellation = cancellation;

        try
        {
            if (!int.TryParse(Port, out var port))
                throw new ArgumentException("Μη έγκυρος αριθμός port.");
            if (!int.TryParse(Timeout, out var timeout))
                throw new ArgumentException("Μη έγκυρο connection timeout.");

            var options = new SqlConnectionOptions(
                Server, Database, WindowsAuthentication, Username, password, port, timeout);

            Status = inspect ? "Γίνεται read-only επιθεώρηση..." : "Έλεγχος σύνδεσης...";
            if (inspect)
            {
                var schema = await _service.InspectAsync(options, cancellation.Token);
                Objects.Clear();
                SelectedObject = null;
                foreach (var item in schema)
                    Objects.Add(item);
                Status = $"Ολοκληρώθηκε η επιθεώρηση: {schema.Count} αντικείμενα.";
                _logger.LogInformation("Schema view refreshed with {Count} objects.", schema.Count);
            }
            else
            {
                var database = await _service.TestConnectionAsync(options, cancellation.Token);
                Status = $"Επιτυχής σύνδεση: {database.Name} • SQL Server {database.ServerVersion}.";
                _logger.LogInformation("Connection test succeeded.");
            }
            SavePreferences();
        }
        catch (OperationCanceledException)
        {
            Status = "Η διαδικασία ακυρώθηκε.";
            _logger.LogInformation("SQL operation canceled by user.");
        }
        catch (Exception ex)
        {
            Status = $"Αποτυχία: {ex.Message}";
            _logger.LogWarning("SQL operation failed: {ErrorType}.", ex.GetType().Name);
        }
        finally
        {
            _cancellation = null;
            IsBusy = false;
        }
    }

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        Changed(name);
        return true;
    }

    private void Changed(string? name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
