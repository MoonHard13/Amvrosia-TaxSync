namespace MoonHard.AmvrosiaTaxSync.Domain.Models;

public sealed record SqlConnectionOptions(
    string Server,
    string Database,
    bool WindowsAuthentication,
    string? Username,
    string? Password,
    int Port = 0,
    int TimeoutSeconds = 15,
    bool TrustServerCertificate = false)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Server))
            throw new ArgumentException("Απαιτείται όνομα SQL Server.", nameof(Server));

        if (string.IsNullOrWhiteSpace(Database))
            throw new ArgumentException("Απαιτείται όνομα βάσης δεδομένων.", nameof(Database));

        if (Port is < 0 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(Port), "Η θύρα πρέπει να είναι 0 ή μεταξύ 1 και 65535.");

        if (Port > 0 && Server.Contains('\\'))
            throw new ArgumentException("Χρησιμοποίησε όνομα instance ή συγκεκριμένη θύρα, όχι και τα δύο.", nameof(Port));

        if (TimeoutSeconds is < 1 or > 120)
            throw new ArgumentOutOfRangeException(nameof(TimeoutSeconds), "Το timeout πρέπει να είναι 1–120 δευτερόλεπτα.");

        if (!WindowsAuthentication && string.IsNullOrWhiteSpace(Username))
            throw new ArgumentException("Απαιτείται SQL username.", nameof(Username));

        if (!WindowsAuthentication && string.IsNullOrEmpty(Password))
            throw new ArgumentException("Απαιτείται SQL password.", nameof(Password));
    }
}
