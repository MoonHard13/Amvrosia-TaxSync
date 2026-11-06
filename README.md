# Amvrosia TaxSync by MoonHard

Εφαρμογή Windows σε **C# / .NET 10 LTS / WPF** για ελεγχόμενο έλεγχο και μελλοντικό συγχρονισμό στοιχείων μητρώου επιχειρήσεων μεταξύ **Amvrosia ERP (Sunsoft)** και της εξουσιοδοτημένης υπηρεσίας **ΑΑΔΕ / ΓΓΠΣ**.

> Ανεξάρτητο εργαλείο της MoonHard. Δεν αποτελεί επίσημο προϊόν της Sunsoft ή της ΑΑΔΕ.

## Κατάσταση ανάπτυξης

**Phase 1 — Foundation (v0.1.0):** Αρχική WPF οθόνη, Windows/SQL Authentication, Test Connection, read-only αναγνώριση SQL πινάκων/views/columns/PK/FK, τοπικές μη ευαίσθητες ρυθμίσεις, JSONL τεχνικά logs, CLI και unit tests.

**Δεν έχουν υλοποιηθεί ακόμα:** σύνδεση με ΑΑΔΕ, ανάκτηση πελατών Amvrosia, field mappings, compare engine και ενημέρωση δεδομένων. Δεν υπάρχει SQL UPDATE/DELETE/INSERT.

## Προϋποθέσεις

- Windows 10/11, .NET 10 SDK και πρόσβαση σε SQL Server.
- Δικαίωμα ανάγνωσης schema metadata στη συγκεκριμένη βάση.
- SQL Server με αξιόπιστο TLS certificate. Το πρόγραμμα απαιτεί encryption και δεν παρακάμπτει την πιστοποίηση του server.

## Εκκίνηση (PowerShell)

    git clone https://github.com/MoonHard13/Amvrosia-TaxSync.git
    cd Amvrosia-TaxSync
    dotnet restore AmvrosiaTaxSync.slnx
    dotnet build AmvrosiaTaxSync.slnx -c Release
    dotnet test tests/MoonHard.AmvrosiaTaxSync.Tests/MoonHard.AmvrosiaTaxSync.Tests.csproj -c Release
    dotnet run --project src/MoonHard.AmvrosiaTaxSync.Desktop

## CLI

    dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- test --server localhost --database master
    dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- scan --server localhost --database master

Για SQL Authentication: χρησιμοποίησε \`--auth sql --user username\` και όρισε προσωρινά την μεταβλητή περιβάλλοντος \`AMVROSIA_SQL_PASSWORD\`. Μην εισάγεις ποτέ κωδικούς σε command-line arguments ή στο GitHub.

## Documentation

[Ελληνικό User Manual](docs/USER_MANUAL.md)

## Security

- Read-only SQL metadata operations, χωρίς μεταβολή σε business data.
- Ο SQL password δεν αποθηκεύεται και δεν καταγράφεται στα logs.
- Το connection string επιβάλλει TLS και certificate validation.
- Τα μη ευαίσθητα preferences αποθηκεύονται στο LocalAppData.
- Μη χρησιμοποιείς production credentials σε tests ή public issues.

## Contributing

Η εργασία γίνεται σε feature branches με Pull Requests και CI build/test checks πριν από merge.
