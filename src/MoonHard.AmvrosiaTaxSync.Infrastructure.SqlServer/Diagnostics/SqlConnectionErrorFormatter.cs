using Microsoft.Data.SqlClient;

namespace MoonHard.AmvrosiaTaxSync.Infrastructure.SqlServer.Diagnostics;

public sealed record SqlConnectionFailure(
    string Title,
    string Suggestion,
    int Number,
    byte State,
    byte Severity,
    Guid ConnectionId)
{
    public string TechnicalDetails =>
        $"SQL Number: {Number}  |  State: {State}  |  Severity: {Severity}  |  Connection ID: {ConnectionId:D}";
}

public static class SqlConnectionErrorFormatter
{
    public static SqlConnectionFailure Diagnose(SqlException exception) =>
        Classify(
            exception.Number,
            exception.State,
            exception.Class,
            exception.ClientConnectionId,
            exception.Message);

    public static SqlConnectionFailure Classify(
        int number,
        byte state = 0,
        byte severity = 0,
        Guid connectionId = default,
        string? providerMessage = null)
    {
        // Ο αρνητικός κωδικός προέρχεται από τον Windows TLS provider, όχι από SQL permissions.
        const int UntrustedCertificateAuthority = -2146893019;

        // Διαβάζουμε το μήνυμα μόνο για ταξινόμηση, χωρίς να το εμφανίζουμε ή να το καταγράφουμε.
        var tlsIssue = number == 0 &&
            (Contains(providerMessage, "certificate") ||
             Contains(providerMessage, "SSL Provider") ||
             Contains(providerMessage, "TLS") ||
             Contains(providerMessage, "SSL"));

        var (title, suggestion) = number == UntrustedCertificateAuthority
            ? ("Μη έμπιστη αλυσίδα πιστοποιητικού SQL Server (TLS)",
               "Ο SQL Server παρουσίασε πιστοποιητικό που δεν επαληθεύεται από τον υπολογιστή. Ζήτησε έγκυρο πιστοποιητικό TLS από έμπιστη CA, έλεγξε την αλυσίδα Root/Intermediate και ότι το DNS όνομα ταιριάζει στο SAN. Μην παρακάμπτεις το certificate validation.")
            : tlsIssue
            ? ("Πρόβλημα TLS / πιστοποιητικού SQL Server",
               "Έλεγξε αν το πιστοποιητικό του SQL Server είναι έμπιστο, έγκυρο και αντιστοιχεί στο όνομα του server. Η εφαρμογή διατηρεί υποχρεωτικό TLS και δεν παρακάμπτει την πιστοποίηση.")
            : number switch
            {
                18456 => ("Αποτυχία σύνδεσης SQL (login)",
                    "Έλεγξε την επιλογή Windows/SQL Authentication, τα διαπιστευτήρια και αν ο SQL Server επιτρέπει το συγκεκριμένο login."),
                18452 => ("Μη έγκυρος ή μη έμπιστος Windows λογαριασμός",
                    "Έλεγξε domain, Windows Authentication και δικαιώματα πρόσβασης στον SQL Server."),
                4060 or 916 => ("Αδυναμία πρόσβασης στη βάση δεδομένων",
                    "Έλεγξε το όνομα της βάσης και τα δικαιώματα του λογαριασμού σε αυτή."),
                26 or 53 or 10060 or 10061 or 11001 => ("Δεν ήταν δυνατή η επικοινωνία με τον SQL Server",
                    "Έλεγξε Server/Instance, port, δίκτυο, firewall και υπηρεσία SQL Server. Για named instance έλεγξε και τον SQL Browser."),
                -2 => ("Έληξε το χρονικό όριο σύνδεσης ή εκτέλεσης",
                    "Έλεγξε προσβασιμότητα server, port, χρόνο απόκρισης και τη ρύθμιση timeout."),
                233 => ("Ο SQL Server έκλεισε τη σύνδεση",
                    "Έλεγξε πρωτόκολλα SQL Server, TLS, authentication και τα SQL Server error logs."),
                0 => ("Αποτυχία επικοινωνίας με τον SQL Server",
                    "Έλεγξε την κατάσταση του SQL Server, τη δικτύωση και την πιστοποίηση TLS. Χρησιμοποίησε τους τεχνικούς κωδικούς για περαιτέρω διάγνωση."),
                _ => ("Αποτυχία λειτουργίας SQL Server",
                    "Έλεγξε τα SQL Server logs με βάση τον αριθμό σφάλματος, το state και το Connection ID. Δεν εμφανίζονται ευαίσθητα connection details.")
            };

        return new SqlConnectionFailure(title, suggestion, number, state, severity, connectionId);
    }

    private static bool Contains(string? message, string token) =>
        message?.Contains(token, StringComparison.OrdinalIgnoreCase) == true;
}
