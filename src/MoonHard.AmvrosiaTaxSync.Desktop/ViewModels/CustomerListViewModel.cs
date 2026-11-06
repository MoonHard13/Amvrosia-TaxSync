using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MoonHard.AmvrosiaTaxSync.Domain.Models;

namespace MoonHard.AmvrosiaTaxSync.Desktop.ViewModels;

public sealed class CustomerListViewModel : INotifyPropertyChanged
{
    private string _search = "";
    private bool _onlyWithAfm;
    private int _page = 1;
    private int _pageSize = 100;
    private long _matched;
    private long _total;
    private long _withAfm;
    private long _withoutAfm;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<CustomerRecord> Rows { get; } = [];

    public string Search
    {
        get => _search;
        set => Set(ref _search, value);
    }

    public bool OnlyWithAfm
    {
        get => _onlyWithAfm;
        set => Set(ref _onlyWithAfm, value);
    }

    public int Page => _page;
    public int PageSize => _pageSize;
    public long MatchedCount => _matched;
    public long Total => _total;
    public long WithAfm => _withAfm;
    public long WithoutAfm => _withoutAfm;
    public bool HasPrevious => _page > 1;
    public bool HasNext => (long)_page * _pageSize < _matched;
    public string PageLabel =>
        _matched == 0 ? "Δεν βρέθηκαν πελάτες" : $"Σελίδα {_page:N0} / {(_matched - 1) / _pageSize + 1:N0} • {_matched:N0} αποτελέσματα";

    public void SetPage(CustomerPage result)
    {
        Rows.Clear();
        foreach (var row in result.Customers)
            Rows.Add(row);

        _page = result.Page;
        _pageSize = result.PageSize;
        _matched = result.MatchedCount;
        _total = result.Totals.Total;
        _withAfm = result.Totals.WithAfm;
        _withoutAfm = result.Totals.WithoutAfm;
        Notify(nameof(Page));
        Notify(nameof(PageSize));
        Notify(nameof(MatchedCount));
        Notify(nameof(Total));
        Notify(nameof(WithAfm));
        Notify(nameof(WithoutAfm));
        Notify(nameof(HasPrevious));
        Notify(nameof(HasNext));
        Notify(nameof(PageLabel));
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        Notify(name);
    }

    private void Notify(string? name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
