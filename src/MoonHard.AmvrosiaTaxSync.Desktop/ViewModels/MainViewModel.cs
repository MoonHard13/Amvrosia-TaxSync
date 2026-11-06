using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MoonHard.AmvrosiaTaxSync.Infrastructure.SqlServer.Diagnostics;
using MoonHard.AmvrosiaTaxSync.Application.Contracts;
using MoonHard.AmvrosiaTaxSync.Application.Services;
using MoonHard.AmvrosiaTaxSync.Desktop.Services;
using MoonHard.AmvrosiaTaxSync.Domain.Models;

namespace MoonHard.AmvrosiaTaxSync.Desktop.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly ISqlSchemaService _service;
    private readonly UdlConnectionFileReader _udlReader;
    private readonly ConnectionPreferencesStore _store;
    private readonly ThemeService _themes;
    private readonly ILogger<MainViewModel> _logger;
    private CancellationTokenSource? _cancellation;
    private string _server = "";
    private string _database = "";
    private string _port = "0";
    private string _timeout = "15";
    private string _username = "";
    private bool _windowsAuthentication = true;
    private bool _isBusy;
    private string _status = "Συμπλήρωσε τα στοιχεία SQL ή φόρτωσε ένα αρχείο UDL.";
    private string _failureTitle = "";
    private string _failureGuidance = "";
    private string _failureTechnical = "";
    private bool _hasSqlFailure;
    private SchemaObject? _selectedObject;
    private string _schemaSearch = "";
    private string _udlFileName = "Δεν έχει επιλεγεί αρχείο";
    private string _theme = "Dark";
    private int _pageIndex;

    public MainViewModel(
        ISqlSchemaService service,
        UdlConnectionFileReader udlReader,
        ConnectionPreferencesStore store,
        ThemeService themes,
        ILogger<MainViewModel> logger)
    {
        _service = service;
        _udlReader = udlReader;
        _store = store;
        _themes = themes;
        _logger = logger;

        var preferences = store.Load();
        _server = preferences.Server;
        _database = preferences.Database;
        _port = preferences.Port.ToString();
        _timeout = preferences.TimeoutSeconds.ToString();
        _windowsAuthentication = preferences.WindowsAuthentication;
        _username = preferences.Username;
        _theme = preferences.Theme == "Light" ? "Light" : "Dark";

        SchemaView = CollectionViewSource.GetDefaultView(Objects);
        SchemaView.Filter = item => item is SchemaObject schema &&
            (string.IsNullOrWhiteSpace(_schemaSearch) ||
             schema.QualifiedName.Contains(_schemaSearch, StringComparison.OrdinalIgnoreCase) ||
             schema.Kind.Contains(_schemaSearch, StringComparison.OrdinalIgnoreCase));

        _themes.Apply(_theme);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<SchemaObject> Objects { get; } = [];
    public ObservableCollection<SchemaColumn> SelectedColumns { get; } = [];
    public ICollectionView SchemaView { get; }

    public string Server { get => _server; set => Set(ref _server, value); }
    public string Database { get => _database; set => Set(ref _database, value); }
    public string Port { get => _port; set => Set(ref _port, value); }
    public string Timeout { get => _timeout; set => Set(ref _timeout, value); }
    public string Username { get => _username; set => Set(ref _username, value); }

    public string SchemaSearch
    {
        get => _schemaSearch;
        set
        {
            if (Set(ref _schemaSearch, value))
                SchemaView.Refresh();
        }
    }

    public string UdlFileName
    {
        get => _udlFileName;
        private set => Set(ref _udlFileName, value);
    }

    public string ThemeButtonText => _theme == "Dark" ? "☀  Light mode" : "☾  Dark mode";

    public int PageIndex
    {
        get => _pageIndex;
        set
        {
            if (value is < 0 or > 1 || !Set(ref _pageIndex, value))
                return;

            Changed(nameof(IsConnectionPage));
            Changed(nameof(IsExplorerPage));
            Changed(nameof(PageTitle));
        }
    }

    public bool IsConnectionPage => PageIndex == 0;
    public bool IsExplorerPage => PageIndex == 1;
    public string PageTitle => IsConnectionPage ? "Σύνδεση SQL Server" : "Schema Explorer";

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

    // Η σύνδεση χρησιμοποιεί την επιλεγμένη σταθερή προεπιλογή.
    public bool TrustServerCertificate => true;

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

    public string FailureTitle
    {
        get => _failureTitle;
        private set => Set(ref _failureTitle, value);
    }

    public string FailureGuidance
    {
        get => _failureGuidance;
        private set => Set(ref _failureGuidance, value);
    }

    public string FailureTechnical
    {
        get => _failureTechnical;
        private set => Set(ref _failureTechnical, value);
    }

    public bool HasSqlFailure
    {
        get => _hasSqlFailure;
        private set => Set(ref _hasSqlFailure, value);
    }

    public string CopyableDiagnostic => $"{FailureTitle}\n{FailureGuidance}\n{FailureTechnical}";

    public void SetStatusMessage(string message) => Status = message;

    private void ClearSqlFailure()
    {
        HasSqlFailure = false;
        FailureTitle = "";
        FailureGuidance = "";
        FailureTechnical = "";
        Changed(nameof(CopyableDiagnostic));
    }

    public int ObjectCount => Objects.Count;
    public string SelectedObjectLabel => SelectedObject is null
        ? "Επίλεξε αντικείμενο από τη λίστα"
        : $"{SelectedObject.QualifiedName} • {SelectedObject.ColumnCount} πεδία";

    public SchemaObject? SelectedObject
    {
        get => _selectedObject;
        set
        {
            if (!Set(ref _selectedObject, value))
                return;

            SelectedColumns.Clear();
            if (value is not null)
            {
                foreach (var column in value.Columns)
                    SelectedColumns.Add(column);
            }

            Changed(nameof(SelectedObjectLabel));
        }
    }

    public void ToggleTheme()
    {
        _theme = _theme == "Dark" ? "Light" : "Dark";
        _themes.Apply(_theme);
        Changed(nameof(ThemeButtonText));
        SavePreferences();
    }

    public Task TestConnectionAsync(string password)
        => ExecuteAsync(password, inspect: false);

    public Task InspectSchemaAsync(string password)
        => ExecuteAsync(password, inspect: true);

    public void Cancel() => _cancellation?.Cancel();

    public async Task<string?> ImportUdlAsync(string path)
    {
        if (IsBusy)
            return null;

        IsBusy = true;
        using var cancellation = new CancellationTokenSource();
        _cancellation = cancellation;
        try
        {
            Status = "Διαβάζονται οι ρυθμίσεις UDL...";
            var imported = await _udlReader.LoadAsync(path, cancellation.Token);
            Server = imported.Options.Server;
            Database = imported.Options.Database;
            Port = imported.Options.Port.ToString();
            Timeout = imported.Options.TimeoutSeconds.ToString();
            Username = imported.Options.Username ?? "";
            WindowsAuthentication = imported.Options.WindowsAuthentication;
            UdlFileName = Path.GetFileName(path);
            Status = imported.ContainsPassword
                ? "Το UDL φορτώθηκε. Βρέθηκε SQL password στο αρχείο — μην το κοινοποιείς."
                : "Το UDL φορτώθηκε. Έλεγξε τα στοιχεία και πάτησε Έλεγχος σύνδεσης.";
            _logger.LogInformation("UDL configuration imported without storing source path or credentials.");
            SavePreferences();
            return imported.Options.Password ?? "";
        }
        catch (OperationCanceledException)
        {
            Status = "Η εισαγωγή UDL ακυρώθηκε.";
            _logger.LogInformation("UDL import cancelled.");
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or NotSupportedException)
        {
            Status = ex.Message;
            _logger.LogWarning("UDL import failed: {ErrorType}", ex.GetType().Name);
        }
        catch (Exception ex)
        {
            Status = "Δεν ήταν δυνατή η ανάγνωση του αρχείου UDL.";
            _logger.LogWarning("UDL file reading failed: {ErrorType}", ex.GetType().Name);
        }
        finally
        {
            _cancellation = null;
            IsBusy = false;
        }

        return null;
    }

    public void SavePreferences()
    {
        try
        {
            _store.Save(new ConnectionPreferences(
                Server, Database, WindowsAuthentication, Username,
                int.TryParse(Port, out var port) ? port : 0,
                int.TryParse(Timeout, out var timeout) ? timeout : 15,
                _theme));
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
        ClearSqlFailure();
        using var cancellation = new CancellationTokenSource();
        _cancellation = cancellation;

        try
        {
            if (!int.TryParse(Port, out var port))
                throw new ArgumentException("Μη έγκυρος αριθμός port.");
            if (!int.TryParse(Timeout, out var timeout))
                throw new ArgumentException("Μη έγκυρο connection timeout.");

            var options = new SqlConnectionOptions(
                Server, Database, WindowsAuthentication, Username, password, port, timeout,
                TrustServerCertificate);

            Status = inspect ? "Γίνεται read-only επιθεώρηση schema..." : "Δοκιμάζεται η σύνδεση SQL...";
            if (inspect)
            {
                var schema = await _service.InspectAsync(options, cancellation.Token);
                Objects.Clear();
                SelectedObject = null;
                foreach (var item in schema)
                    Objects.Add(item);
                Changed(nameof(ObjectCount));
                SchemaView.Refresh();
                PageIndex = 1;
                Status = $"Η επιθεώρηση ολοκληρώθηκε: {schema.Count} SQL αντικείμενα.";
                _logger.LogInformation("Schema view refreshed with {Count} objects.", schema.Count);
            }
            else
            {
                var database = await _service.TestConnectionAsync(options, cancellation.Token);
                Status = $"Επιτυχής σύνδεση με {database.Name} (SQL Server {database.ServerVersion}).";
                _logger.LogInformation("SQL test connection succeeded.");
            }

            SavePreferences();
        }
        catch (OperationCanceledException)
        {
            Status = "Η διαδικασία ακυρώθηκε.";
            _logger.LogInformation("SQL operation canceled by user.");
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            Status = ex.Message;
            _logger.LogWarning("Invalid SQL connection settings: {ErrorType}.", ex.GetType().Name);
        }
        catch (SqlException ex)
        {
            var diagnostic = SqlConnectionErrorFormatter.Diagnose(ex);
            FailureTitle = diagnostic.Title;
            FailureGuidance = diagnostic.Suggestion;
            FailureTechnical = diagnostic.TechnicalDetails;
            HasSqlFailure = true;
            Changed(nameof(CopyableDiagnostic));
            Status = $"Αποτυχία SQL #{diagnostic.Number}: {diagnostic.Title}";

            // Καταγράφονται μόνο τεχνικοί κωδικοί και όχι το πλήρες SQL exception.
            _logger.LogWarning(
                "SQL operation failed: Number={SqlNumber}, State={SqlState}, Severity={SqlSeverity}, ConnectionId={ConnectionId}.",
                diagnostic.Number, diagnostic.State, diagnostic.Severity, diagnostic.ConnectionId);
        }
        catch (Exception ex)
        {
            Status = $"Η λειτουργία απέτυχε ({ex.GetType().Name}). Δες τα τεχνικά logs.";
            _logger.LogWarning("Operation failed: {ErrorType}.", ex.GetType().Name);
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
