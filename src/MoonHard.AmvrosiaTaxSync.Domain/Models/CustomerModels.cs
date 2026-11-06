namespace MoonHard.AmvrosiaTaxSync.Domain.Models;

public enum AfmStatus
{
    Missing,
    Valid,
    Invalid
}

public sealed record CustomerRecord(
    string Code,
    string Title,
    string Name,
    string Afm,
    bool IsInactive,
    AfmStatus AfmStatus)
{
    public string DisplayName => !string.IsNullOrWhiteSpace(Title) ? Title : Name;
    public string AfmStatusLabel => AfmStatus switch
    {
        AfmStatus.Valid => "Έγκυρο",
        AfmStatus.Missing => "Χωρίς ΑΦΜ",
        _ => "Μη έγκυρο"
    };
    public string ActivityLabel => IsInactive ? "Ανενεργός" : "Ενεργός";
}

public sealed record CustomerTotals(long Total, long WithAfm, long WithoutAfm);

public sealed record CustomerPage(
    IReadOnlyList<CustomerRecord> Customers,
    CustomerTotals Totals,
    long MatchedCount,
    int Page,
    int PageSize)
{
    public long PageCount => MatchedCount == 0 ? 0 : (MatchedCount - 1) / PageSize + 1;
}

public sealed record CustomerQuery(string Search = "", int Page = 1, int PageSize = 100)
{
    public void Validate()
    {
        if (Page < 1)
            throw new ArgumentOutOfRangeException(nameof(Page), "Η σελίδα πρέπει να είναι μεγαλύτερη του μηδενός.");
        if (PageSize is < 10 or > 500)
            throw new ArgumentOutOfRangeException(nameof(PageSize), "Το μέγεθος σελίδας πρέπει να είναι 10–500.");
        if (Search.Length > 100)
            throw new ArgumentException("Η αναζήτηση δέχεται έως 100 χαρακτήρες.", nameof(Search));
    }
}
