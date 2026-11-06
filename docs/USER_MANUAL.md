# Amvrosia TaxSync by MoonHard — Εγχειρίδιο Χρήσης v0.2.0

## 1. Σκοπός και στάδιο υλοποίησης

Το Amvrosia TaxSync είναι ανεξάρτητο τεχνικό εργαλείο της MoonHard για υποδομή φορολογικού συγχρονισμού με το ERP Amvrosia. Η έκδοση v0.2.0 παρέχει modern WPF workspace, εισαγωγή UDL, Test Connection και read-only Schema Explorer. **Η σύνδεση ΑΑΔΕ και οι τροποποιήσεις πελατών δεν είναι διαθέσιμες ακόμη.**

{% hint style="warning" %}
Μην θεωρείς ότι ο έλεγχος schema ενημερώνει πελάτες ή πραγματοποιεί σύνδεση με την ΑΑΔΕ. Για παραγωγική βάση χρησιμοποίησε αποκλειστικά λογαριασμό ελάχιστων δικαιωμάτων και εγκεκριμένη διαδικασία πρόσβασης.
{% endhint %}

## 2. Προαπαιτούμενα

- Windows 10/11 x64.
- .NET 10 SDK για ανάπτυξη.
- Πρόσβαση σε SQL Server με σωστά δικαιώματα ανάγνωσης metadata.
- Έγκυρο TLS certificate του SQL Server, αναγνωρισμένο από το Windows trust store.
- Προαιρετικό UDL αρχείο, συνήθως με κωδικοποίηση UTF-16 LE.

## 3. Εγκατάσταση / Εκτέλεση

Άνοιξε PowerShell από τον φάκελο του repository.

```powershell
git clone https://github.com/MoonHard13/Amvrosia-TaxSync.git
cd Amvrosia-TaxSync
dotnet restore AmvrosiaTaxSync.slnx
dotnet build AmvrosiaTaxSync.slnx -c Release
dotnet test tests/MoonHard.AmvrosiaTaxSync.Tests/MoonHard.AmvrosiaTaxSync.Tests.csproj -c Release
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Desktop
```

Σε περίπτωση που το PR δεν έχει γίνει merge, κατέβασε το branch `feat/modern-themes-udl` αντί για το `main`.

## 4. Light και Dark Mode

1. Εκκίνησε την εφαρμογή.
2. Στην επάνω δεξιά γωνία πάτησε **Light mode** ή **Dark mode**.
3. Η παλέτα εφαρμόζεται σε παράθυρο, sidebar, κουμπιά, πεδία, data grids και κατάσταση.
4. Η επιλογή αποθηκεύεται τοπικά και εφαρμόζεται σε επόμενη εκκίνηση.

{% hint style="info" %}
Συντόμευση για εναλλαγή Light/Dark: **Ctrl+Shift+L**. Δεν απαιτείται επανεκκίνηση.
{% endhint %}

## 5. Εισαγωγή SQL σύνδεσης από UDL

1. Άνοιξε τη σελίδα **Σύνδεση SQL** από το sidebar.
2. Πάτησε **Φόρτωση UDL** (ή **Ctrl+O**).
3. Επίλεξε το εξουσιοδοτημένο αρχείο `.udl`.
4. Η εφαρμογή ανακτά **Data Source, Initial Catalog, Integrated Security, User ID, Password** (όταν υπάρχει) και τις αναγνωρίσιμες ρυθμίσεις port/timeout.
5. Έλεγξε τα συμπληρωμένα πεδία και διόρθωσέ τα εφόσον χρειάζεται.
6. Πάτησε **Έλεγχος σύνδεσης (Ctrl+T)**.
7. Προαιρετικά πάτησε **Επιθεώρηση schema (Ctrl+R)**.

Υποστηρίζονται OLE DB providers SQL Server: **SQLOLEDB**, **SQLNCLI** και **MSOLEDBSQL**. Άλλα UDL providers απορρίπτονται. Το UDL χρησιμοποιείται **μόνο** για ανάγνωση στοιχείων σύνδεσης — η πραγματική σύνδεση πραγματοποιείται με **Microsoft.Data.SqlClient**.

{% hint style="danger" %}
Παλιές εφαρμογές ενδέχεται να αποθηκεύουν password ως plaintext στο UDL. Το Amvrosia TaxSync δεν αποθηκεύει αυτό το password στο `settings.json` ή στα logs. Προστάτεψε το αρχικό UDL με κατάλληλα NTFS permissions και μη το ανεβάζεις σε GitHub ή tickets.
{% endhint %}

{% hint style="warning" %}
Η εισαγωγή δεν μεταφέρει μη ασφαλείς ρυθμίσεις, όπως `TrustServerCertificate=True` ή απενεργοποίηση encryption. Αν ο SQL Server διαθέτει self-signed/μη έμπιστο certificate, θα αποτύχει το Test Connection μέχρι να διορθωθεί η TLS αλυσίδα εμπιστοσύνης.
{% endhint %}

## 6. Χειροκίνητη SQL σύνδεση

1. Εισήγαγε το **Server / Instance**, π.χ. `SERVER\SQLEXPRESS`.
2. Εισήγαγε το όνομα **Database**.
3. Θέσε **Port** ή άφησε `0` για default SQL ρύθμιση.
4. Επίλεξε **Windows Authentication** ή συμπλήρωσε SQL Username/Password.
5. Έλεγξε τη σύνδεση με **Ctrl+T**. Η εφαρμογή δεν αποθηκεύει το SQL Password.

## 7. Schema Explorer

1. Από το sidebar άνοιξε **Schema Explorer** ή πάτησε **Ctrl+2**.
2. Πάτησε **Ανανέωση schema**, ή **Ctrl+R** από οποιαδήποτε σελίδα.
3. Δες πίνακες/views στο αριστερό πλέγμα.
4. Αναζήτησε αντικείμενα από το πεδίο αναζήτησης με **Ctrl+F**.
5. Επίλεξε πίνακα/view για να προβληθούν τα columns, data types, nullable, PK/FK.

{% hint style="info" %}
Ο Schema Explorer εκτελεί αποκλειστικά SELECT σε system catalogs. Δεν πραγματοποιεί INSERT, UPDATE, DELETE ή αλλαγές schema.
{% endhint %}

## 8. Συντομεύσεις πληκτρολογίου

| Συντόμευση | Λειτουργία |
| --- | --- |
| Ctrl+1 | Σελίδα σύνδεσης |
| Ctrl+2 | Schema Explorer |
| Ctrl+O | Εισαγωγή UDL |
| Ctrl+T | Test Connection |
| Ctrl+R | Επιθεώρηση SQL schema |
| Ctrl+F | Εστίαση στο Server ή στο Schema Search |
| Ctrl+Shift+L | Εναλλαγή Light/Dark |
| Esc | Ακύρωση ενεργής λειτουργίας |

## 9. CLI / CMD

Χειροκίνητη σύνδεση:

```powershell
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- test --server localhost --database master
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- scan --server localhost --database master
```

Εισαγωγή μέσω UDL:

```powershell
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- test --udl "C:\Connections\amvrosia.udl"
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- scan --udl "C:\Connections\amvrosia.udl"
```

Όταν δεν υπάρχει password στο UDL για SQL Authentication:

```powershell
$secret = Read-Host "SQL password" -AsSecureString
$env:AMVROSIA_SQL_PASSWORD = [System.Net.NetworkCredential]::new("", $secret).Password
try {
    dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- test --udl "C:\Connections\amvrosia.udl"
}
finally {
    Remove-Item Env:AMVROSIA_SQL_PASSWORD -ErrorAction SilentlyContinue
    $secret.Dispose()
}
```

**Exit codes:** 0 επιτυχία, 1 runtime error, 2 μη έγκυρες παράμετροι, 3 ακύρωση.

## 10. Ρυθμίσεις, logs και privacy

- Preferences: `%LOCALAPPDATA%\MoonHard\AmvrosiaTaxSync\settings.json`
- Τεχνικά logs: `%LOCALAPPDATA%\MoonHard\AmvrosiaTaxSync\Logs\taxsync-YYYY-MM-DD.jsonl`
- Η αποθήκευση περιλαμβάνει theme και μη ευαίσθητα στοιχεία SQL (χωρίς password ή UDL path).
- Τα UDL αρχεία ενδέχεται να περιέχουν μυστικά. Μη τα συμπεριλαμβάνεις σε diagnostic exports.

## 11. Troubleshooting

| Σύμπτωμα | Ενέργεια |
| --- | --- |
| Unsupported UDL provider | Επίλεξε UDL με SQLOLEDB, SQLNCLI ή MSOLEDBSQL. |
| Invalid UDL | Έλεγξε ότι είναι αρχείο .udl με ενότητα `[oledb]`. |
| Λείπει SQL password | Συμπλήρωσε Password στο UI ή χρησιμοποίησε την κατάλληλη env var στο CLI. |
| Login failed | Έλεγξε credentials, login permissions και database. |
| Server not found | Έλεγξε instance, SQL Browser, port, firewall και δικτύωση. |
| Certificate error | Εγκατάστησε έμπιστο certificate στον SQL Server. |
| Κενή λίστα | Έλεγξε read-only metadata permissions. |
| Παλιά χρώματα | Δοκίμασε Ctrl+Shift+L και επανεκκίνηση. |

## 12. Δημιουργία EXE / Έλεγχοι

```powershell
dotnet publish src/MoonHard.AmvrosiaTaxSync.Desktop -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Το παραγόμενο exe βρίσκεται στον `bin\Release\net10.0-windows\win-x64\publish` φάκελο του Desktop project.

### Χειροκίνητος έλεγχος πριν από production

- [ ] Έλεγχος Light/Dark αντίθεσης (buttons, hover, disabled, focus, rows).
- [ ] Test UDL με Windows Auth, SQL Auth και UDL χωρίς password.
- [ ] Επιθεώρηση schema σε μη παραγωγική βάση.
- [ ] Έλεγχος αλλαγής θέματος και επανεκκίνησης.
- [ ] Επιβεβαίωση ότι δεν εμφανίζεται password σε preferences/logs.
- [ ] Έλεγχος TLS certificate validation και failure handling.
- [ ] Smoke test του publish output σε καθαρό Windows σύστημα.

{% hint style="warning" %}
Το επιτυχημένο build/test στο GitHub Actions δεν αντικαθιστά τον χειροκίνητο οπτικό έλεγχο WPF ή τον έλεγχο με πραγματικό SQL Server.
{% endhint %}
