# Amvrosia TaxSync by MoonHard — Εγχειρίδιο Χρήσης v0.3.0

## 1. Σκοπός και στάδιο υλοποίησης

Το Amvrosia TaxSync είναι ανεξάρτητο τεχνικό εργαλείο της MoonHard για υποδομή φορολογικού συγχρονισμού με το ERP Amvrosia. Η έκδοση v0.3.0 προσθέτει read-only πελάτες από το TblSnCustomer στο WPF και στο CLI. **Η σύνδεση ΑΑΔΕ και οι τροποποιήσεις πελατών δεν είναι διαθέσιμες ακόμη.**

{% hint style="warning" %}
Μην θεωρείς ότι ο έλεγχος schema ενημερώνει πελάτες ή πραγματοποιεί σύνδεση με την ΑΑΔΕ. Για παραγωγική βάση χρησιμοποίησε αποκλειστικά λογαριασμό ελάχιστων δικαιωμάτων και εγκεκριμένη διαδικασία πρόσβασης.
{% endhint %}

## 2. Προαπαιτούμενα

- Windows 10/11 x64.
- .NET 10 SDK για ανάπτυξη.
- Πρόσβαση σε SQL Server με σωστά δικαιώματα ανάγνωσης metadata.
- Ο SQL Server πρέπει να είναι προσβάσιμος· η εφαρμογή χρησιμοποιεί από προεπιλογή `TrustServerCertificate=True` με υποχρεωτική κρυπτογράφηση.
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
Η εισαγωγή UDL δεν απενεργοποιεί την κρυπτογράφηση. Το πρόγραμμα εφαρμόζει `Encrypt=Mandatory;TrustServerCertificate=True` από προεπιλογή, ακόμη κι όταν το UDL δεν περιέχει αυτή τη ρύθμιση.
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

## 13. Διάγνωση αποτυχίας SQL Server

Η εφαρμογή εμφανίζει πλέον ξεχωριστή κάρτα διάγνωσης όταν αποτυγχάνει το Test Connection ή το Schema Explorer.

1. Δοκίμασε **Έλεγχος σύνδεσης (Ctrl+T)**.
2. Αν προκύψει αποτυχία, δες την περιγραφή και την πρόταση επίλυσης.
3. Πάτησε **Αντιγραφή διάγνωσης** για να αποθηκεύσεις το ασφαλές αναγνωριστικό προβλήματος.
4. Κατάγραψε μόνο τον αριθμό SQL, State, Severity και Connection ID στο τεχνικό ticket.

### Συνήθεις SQL κωδικοί

| SQL Number | Πιθανό πρόβλημα | Έλεγχος |
| --- | --- | --- |
| 18456 | Αποτυχία login | Authentication, credentials, SQL login |
| 18452 | Windows Authentication / domain | Domain, account trust |
| 4060 / 916 | Μη προσβάσιμη βάση | Database name, SQL user mapping/permissions |
| 26 / 53 / 10060 / 10061 | Server/instance ή δικτύωση | Server, port, firewall, SQL Browser |
| -2 | Timeout | Δίκτυο, χρόνο απόκρισης, ρύθμιση timeout |
| 233 | Κλείσιμο σύνδεσης από τον server | SQL error logs, protocols, authentication |
| -2146893019 / 0x80090325 | Μη έμπιστη αρχή πιστοποίησης TLS | Έγκυρη αλυσίδα Root/Intermediate CA, SAN/DNS hostname, SQL Server TLS certificate |
| 0 με ένδειξη TLS / SSL / certificate | Πιστοποιητικό ή TLS | Certificate trust chain, server name, λήξη certificate |

{% hint style="warning" %}
Ο ίδιος SQL Number μπορεί να εμφανιστεί για διαφορετικές πραγματικές αιτίες. Οι προτάσεις είναι διαγνωστικά βήματα, όχι εγγυημένη διάγνωση. Για λεπτομερή διερεύνηση έλεγξε SQL Server Error Log και Windows Event Viewer.
{% endhint %}

{% hint style="danger" %}
Η προεπιλογή είναι `Encrypt=Mandatory;TrustServerCertificate=True`. Η σύνδεση κρυπτογραφείται, αλλά δεν επαληθεύεται η ταυτότητα του server. Σε ευαίσθητα/παραγωγικά περιβάλλοντα προτείνεται έμπιστο TLS certificate και ενεργή επαλήθευση.
{% endhint %}

### Light / Dark tooltips

Τα tooltips των κουμπιών ακολουθούν τη χρωματική παλέτα του προγράμματος και έχουν ρητά καθορισμένα χρώματα κειμένου, φόντου και περιγράμματος. Έλεγξε οπτικά hover/keyboard focus σε Light και Dark Mode.

Τα SQL passwords, τα UDL περιεχόμενα και τα πλήρη SQL exceptions **δεν** συμπεριλαμβάνονται στα κοινά logs ή στο κείμενο που αντιγράφει το κουμπί **Αντιγραφή διάγνωσης**.

## 14. Ειδική αντιμετώπιση SQL Error -2146893019

Το σφάλμα `-2146893019` (`0x80090325`) υποδεικνύει ότι η TLS αλυσίδα πιστοποιητικών που παρουσίασε ο SQL Server δεν θεωρείται έμπιστη από τον Windows client. **Δεν αποδεικνύει σφάλμα username, password ή SQL permissions.**

1. Στον SQL Server άνοιξε **SQL Server Configuration Manager → SQL Server Network Configuration → Protocols for [instance] → Properties → Certificate**.
2. Επιβεβαίωσε ποιο πιστοποιητικό χρησιμοποιεί το instance. Έλεγξε validity, **Server Authentication EKU** και ότι το **Subject Alternative Name (SAN)** περιλαμβάνει το DNS όνομα με το οποίο συνδέεται το Amvrosia TaxSync.
3. Σε περιβάλλον παραγωγής προτίμησε πιστοποιητικό SQL Server από έμπιστη εταιρική ή δημόσια CA, όχι το αυτόματα δημιουργημένο self-signed certificate.
4. Αν έχεις εσωτερική CA, εγκατάστησε **μόνο** το επαληθευμένο root CA certificate (και την απαιτούμενη intermediate chain) στα κατάλληλα Windows certificate stores του client, μέσω IT/διαχειριστή. Πριν από την εισαγωγή έλεγξε την προέλευσή του από αξιόπιστο κανάλι.
5. Μετά την εγκεκριμένη αλλαγή πιστοποιητικού στο instance μπορεί να χρειαστεί επανεκκίνηση της υπηρεσίας SQL Server, σε προγραμματισμένο maintenance window. Έλεγξε ξανά με **Ctrl+T**.

{% hint style="danger" %}
Μην κάνεις import τυχαίου πιστοποιητικού στα Trusted Root Certification Authorities και μην ρυθμίσεις `TrustServerCertificate=True` σε παραγωγικό σύστημα μόνο για να περάσει η σύνδεση. Η παράκαμψη πιστοποιητικού μπορεί να αποκρύψει παραπλάνηση του server.
{% endhint %}

Επίσημες πηγές: [Microsoft — Untrusted Certificate Authority](https://learn.microsoft.com/en-us/troubleshoot/sql/database-engine/connect/error-message-when-you-connect), [Certificate Requirements](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/certificate-requirements), [Certificate Management](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/manage-certificates).


## 15. Προεπιλεγμένη σύνδεση με TrustServerCertificate=True

Η εφαρμογή συνδέεται στον SQL Server με προεπιλογή:

```text
Encrypt=Mandatory;TrustServerCertificate=True
```

### Χρήση από το GUI

1. Άνοιξε τη σελίδα **Σύνδεση SQL**.
2. Πάτησε **Φόρτωση UDL (Ctrl+O)** ή συμπλήρωσε χειροκίνητα Server/Database/Authentication.
3. Πάτησε **Έλεγχος σύνδεσης (Ctrl+T)**.
4. Για ανάγνωση metadata πάτησε **Επιθεώρηση schema (Ctrl+R)**.

Η σύνδεση χρησιμοποιεί αυτόματα `TrustServerCertificate=True` κατά την εκκίνηση και μετά την εισαγωγή UDL. Δεν χρειάζεται να ενεργοποιήσεις χειροκίνητα κάποια εξαίρεση. Το υπάρχον κουμπί **Απενεργοποίηση εξαίρεσης TLS** μπορεί να χρησιμοποιηθεί αν θέλεις να επιβάλεις επαλήθευση πιστοποιητικού στην τρέχουσα συνεδρία.

### Χρήση από CLI

Η ίδια προεπιλογή ισχύει και στις εντολές `test` / `scan`:

```powershell
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- test --udl "C:\Connections\amvrosia.udl"
```

Για αυστηρή επαλήθευση πιστοποιητικού σε μεμονωμένη CLI κλήση:

```powershell
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- test --udl "C:\Connections\amvrosia.udl" --trust-server-certificate false
```

{% hint style="warning" %}
Η μεταφορά παραμένει κρυπτογραφημένη, αλλά όταν `TrustServerCertificate=True` δεν επαληθεύεται η ταυτότητα του SQL Server. Αυτό αυξάνει τον κίνδυνο σύνδεσης με μη εξουσιοδοτημένο server. Για production συνιστάται η αποκατάσταση της αλυσίδας TLS πιστοποιητικών και η ενεργοποίηση επαλήθευσης.
{% endhint %}

## 16. Πελάτες Αμβροσίας — Customer Discovery (v0.3.0)

### 16.1 Σύνδεση και προβολή

Η νέα ενότητα **Πελάτες Αμβροσίας** διαβάζει αποκλειστικά τον πίνακα **TblSnCustomer**. Δεν ενημερώνει το ERP, δεν καλεί ΑΑΔΕ και δεν αλλάζει κανένα πεδίο.

1. Φόρτωσε UDL (**Ctrl+O**) ή συμπλήρωσε τα στοιχεία SQL σύνδεσης.
2. Πάτησε **Πελάτες Αμβροσίας** ή **Ctrl+3**.
3. Η πρώτη σελίδα φορτώνεται αυτόματα.
4. Δες τους μετρητές **Σύνολο**, **Με ΑΦΜ** και **Χωρίς ΑΦΜ**.
5. Αναζήτησε με κωδικό, επωνυμία, όνομα ή ΑΦΜ, με Enter ή **Αναζήτηση / Ανανέωση**.
6. Χρησιμοποίησε **Προηγούμενη** / **Επόμενη** για τη σελιδοποίηση ανά 100 εγγραφές. Για ακύρωση πάτησε Esc.

### 16.2 Αντιστοίχιση πεδίων

| Πεδίο SQL | Σκοπός |
| --- | --- |
| CustCode | Κωδικός πελάτη |
| CustTitle | Επωνυμία |
| CustName | Όνομα |
| CustAFM | ΑΦΜ |
| CustInactive | Ενεργός / ανενεργός |
| CustomerOID | Εσωτερική ταξινόμηση σελίδων |

Ο SQL reader εντοπίζει το schema του TblSnCustomer μέσω sys.tables και επαληθεύει τις απαιτούμενες στήλες. Δεν υποθέτει τυφλά το schema dbo. Εάν υπάρχει περισσότερος από ένας πίνακας με ίδιο όνομα ή λείπουν απαιτούμενα πεδία, εμφανίζει σφάλμα χωρίς να διαβάσει λάθος πίνακα.

### 16.3 Έλεγχος ΑΦΜ

**Έγκυρο:** ακριβώς 9 ψηφία με σωστό ελληνικό ψηφίο ελέγχου. **Χωρίς ΑΦΜ:** κενό ή NULL. **Μη έγκυρο:** λάθος μήκος, μη αριθμητικοί χαρακτήρες ή λάθος ψηφίο ελέγχου.

{% hint style="info" %}
Το «Έγκυρο» αφορά μόνο τη μαθηματική μορφή του ελληνικού ΑΦΜ, όχι την πραγματική εγγραφή του στη φορολογική αρχή ή την αντιστοίχισή του στον πελάτη. Η επικύρωση ΓΓΠΣ/ΑΑΔΕ είναι επόμενη φάση.
{% endhint %}

Οι μετρητές Με / Χωρίς ΑΦΜ υπολογίζουν αν υπάρχει τιμή, όχι αν αυτή είναι έγκυρη. Ο τοπικός αλγοριθμικός έλεγχος τρέχει στις εγγραφές της ενεργής σελίδας.

### 16.4 CLI

Από τον ριζικό φάκελο του repository:

~~~powershell
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- customers --udl "C:\Connections\amvrosia.udl" --page 1 --page-size 100
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- customers --udl "C:\Connections\amvrosia.udl" --search "123456783"
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- customers --udl "C:\Connections\amvrosia.udl" --only-with-afm true
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- customers --server SQL-SERVER --database Amvrosia --auth windows
~~~

Η έξοδος είναι JSON με εγγραφές, συνολικούς μετρητές και πληροφορίες σελίδας. Το page-size δέχεται 10–500, η σελίδα αρχίζει από 1 και η αναζήτηση δέχεται έως 100 χαρακτήρες. Για SQL Authentication όρισε τον κωδικό μέσω AMVROSIA_SQL_PASSWORD.

{% hint style="warning" %}
Η έξοδος JSON περιέχει πραγματικά στοιχεία πελατών. Μην την ανεβάζεις σε δημόσιο GitHub repository ή issue και μη βάζεις προσωπικά δεδομένα σε logs.
{% endhint %}

### 16.5 Ασφάλεια, troubleshooting και έλεγχοι

- Όλη η λειτουργία χρησιμοποιεί μόνο SELECT και SQL parameters, με server-side OFFSET/FETCH, CancellationToken και logging χωρίς τιμές πελατών.
- Η αναζήτηση αντιμετωπίζει ειδικούς SQL LIKE χαρακτήρες ως απλό κείμενο.
- Εάν δεν βρεθεί TblSnCustomer, έλεγξε ότι συνδέθηκες στη σωστή βάση.
- Εάν λείπουν πεδία, σύγκρινε το schema και τα δικαιώματα ανάγνωσης SQL metadata.
- Για κενή λίστα ή πολύ αργή αναζήτηση, έλεγξε τα φίλτρα και τα indexes.
- Το SQL connection default παραμένει Encrypt=Mandatory;TrustServerCertificate=True.

{% hint style="warning" %}
Οι ακριβείς SQL τύποι δεδομένων και τα indexes δεν έχουν ακόμη επιβεβαιωθεί σε πραγματική βάση. Τα automated tests δεν υποκαθιστούν την ολοκλήρωση read-only δοκιμής σε ελεγχόμενο SQL Server.
{% endhint %}
