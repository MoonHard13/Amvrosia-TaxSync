# Amvrosia TaxSync by MoonHard

Επαγγελματική εφαρμογή Windows σε **C# / .NET 10 LTS / WPF** για τη μελλοντική επαλήθευση φορολογικών στοιχείων επιχειρήσεων στο **Amvrosia ERP** μέσω ΑΑΔΕ/ΓΓΠΣ.

> Ανεξάρτητο προϊόν της MoonHard — όχι επίσημο λογισμικό Sunsoft ή ΑΑΔΕ.

## Υλοποιημένες λειτουργίες (v0.2.0)

- Μοντέρνο WPF workspace με sidebar, ευανάγνωστα controls, **Light/Dark mode** και αποθήκευση επιλογής θέματος.
- **Φόρτωση UDL** SQL Server (`.udl`, UTF-16/UTF-8), αναγνώριση server, database, authentication, username, port, timeout και προσωρινή χρήση password.
- Χειροκίνητη σύνδεση με Windows ή SQL Authentication.
- Test Connection με `Encrypt=Mandatory;TrustServerCertificate=True` από προεπιλογή.
- **Read-only Schema Explorer** με tables, views, columns, PK/FK και ζωντανή αναζήτηση.
- Ακύρωση μεγάλων SQL εργασιών, CLI με `--udl`, τεχνικά logs, unit tests και Windows CI.

**Δεν έχουν ακόμη υλοποιηθεί:** ο πραγματικός Amvrosia customer adapter, AADE SOAP, comparison engine, εγκεκριμένα updates. Δεν εκτελούνται SQL data writes.

## Οδηγίες έναρξης

```powershell
git clone https://github.com/MoonHard13/Amvrosia-TaxSync.git
cd Amvrosia-TaxSync
dotnet restore AmvrosiaTaxSync.slnx
dotnet build AmvrosiaTaxSync.slnx -c Release
dotnet test tests/MoonHard.AmvrosiaTaxSync.Tests/MoonHard.AmvrosiaTaxSync.Tests.csproj -c Release
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Desktop
```

## CLI

```powershell
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- test --udl "C:\Connections\amvrosia.udl"
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- scan --udl "C:\Connections\amvrosia.udl"
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- test --server localhost --database master
```

Για SQL Authentication χωρίς password μέσα στο UDL, μπορεί να χρησιμοποιηθεί προσωρινά η μεταβλητή περιβάλλοντος `AMVROSIA_SQL_PASSWORD`. Δεν προσφέρουμε password command-line argument.

## Προϋποθέσεις

Windows 10/11, .NET 10 SDK και προσβάσιμος SQL Server. Για την επιθεώρηση απαιτούνται δικαιώματα ανάγνωσης SQL metadata.

## Ασφάλεια

{% hint style="warning" %}
Ορισμένα UDL αρχεία περιέχουν password **σε απλό κείμενο**. Χρησιμοποίησε μόνο εξουσιοδοτημένα αρχεία, μην τα ανεβάζεις στο Git και μην τα επισυνάπτεις σε issues. Η εφαρμογή δεν αποθηκεύει το password στα preferences/logs. Η προεπιλογή είναι `TrustServerCertificate=True`. Οι ρυθμίσεις TLS ή άλλες παλιές επιλογές provider από το UDL δεν μεταφέρονται αυτούσιες στον SqlClient.
{% endhint %}

[Αναλυτικό ελληνικό User Manual](docs/USER_MANUAL.md)

## Διαδικασία ανάπτυξης

Feature branch → Pull Request → GitHub Actions → review → merge. Δοκιμές GUI και SQL authentication σε πραγματικά Windows χρειάζονται manual smoke tests.

## SQL connection diagnostics και Tooltips

Ο έλεγχος SQL επιστρέφει πλέον ασφαλή, κατανοητή διάγνωση με SQL Number / State / Severity / Connection ID και ενέργειες αντιμετώπισης αντί για μόνο \`SqlException\`. Το GUI εμφανίζει διαγνωστική κάρτα με λειτουργία αντιγραφής και το CLI χρησιμοποιεί τον ίδιο formatter. Τα tooltips έχουν δικές τους παλέτες Light/Dark με ρητή αντίθεση.

Επίσημη τεκμηρίωση: https://learn.microsoft.com/en-us/sql/connect/ado-net/connection-string-syntax

**Παραμένει απαραίτητος χειροκίνητος έλεγχος SQL σύνδεσης και οπτικός έλεγχος WPF tooltips σε Windows.**


### Προεπιλεγμένη σύνδεση TLS

Το WPF και το CLI χρησιμοποιούν `Encrypt=Mandatory;TrustServerCertificate=True` χωρίς ανάγκη ενεργοποίησης εξαίρεσης. Το GUI προσφέρει επιλογή απενεργοποίησης της εξαίρεσης, ενώ το CLI δέχεται `--trust-server-certificate false` για αυστηρή επαλήθευση.

Η κρυπτογράφηση παραμένει ενεργή, όμως η ταυτότητα του server δεν επαληθεύεται με την προεπιλογή. Συνιστάται έγκυρο TLS certificate και αυστηρή επαλήθευση σε παραγωγικά περιβάλλοντα.


### Phase 2 — Πελάτες Αμβροσίας (v0.3.0)

Η εφαρμογή διαβάζει σελίδες 100 εγγραφών από τον πραγματικό πίνακα **TblSnCustomer** και παρουσιάζει **CustCode, CustTitle, CustName, CustAFM, CustInactive**, στατιστικά με / χωρίς ΑΦΜ, φίλτρο μόνο με ΑΦΜ και τοπική εγκυρότητα ελληνικού ΑΦΜ. Υπάρχει αναζήτηση (SQL parameters), πλοήγηση με Ctrl+3, read-only λειτουργία και υποστήριξη CLI.

Παράδειγμα:

~~~powershell
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- customers --udl "C:\Connections\amvrosia.udl" --page 1 --page-size 100
~~~

Η εφαρμογή δεν συνδέεται ακόμη με ΓΓΠΣ/ΑΑΔΕ ούτε γράφει στη βάση. Για αναλυτικές οδηγίες δες το ελληνικό docs/USER_MANUAL.md. Τα παραγόμενα JSON δεδομένα περιέχουν στοιχεία πελατών και δεν πρέπει να δημοσιεύονται.
