# Amvrosia TaxSync by MoonHard — Οδηγός Χρήσης

## 1. Εισαγωγή

Το **Amvrosia TaxSync by MoonHard** είναι ένα ανεξάρτητο τεχνικό εργαλείο για το ERP Αμβροσία. Η τρέχουσα αρχική έκδοση επιτρέπει **έλεγχο σύνδεσης SQL Server και read-only επιθεώρηση της δομής της βάσης**. Η διασύνδεση με ΑΑΔΕ και οι ενημερώσεις πελατών θα υλοποιηθούν σε επόμενες εκδόσεις.

{% hint style="warning" %}
Η εφαρμογή δεν πραγματοποιεί αυτή τη στιγμή αλλαγές σε πελάτες, πίνακες ή άλλα δεδομένα ERP. Δεν πρέπει να θεωρείται ακόμη ολοκληρωμένο εργαλείο φορολογικού συγχρονισμού.
{% endhint %}

## 2. Προϋποθέσεις

1. Υπολογιστής με Windows 10/11 x64.
2. Εγκατεστημένο .NET 10 SDK για ανάπτυξη ή κατάλληλο runtime για framework-dependent deployment.
3. Πρόσβαση SQL Server στη βάση που επιθεωρείται.
4. Κατάλληλο SQL account με δικαιώματα ανάγνωσης metadata. Προτείνεται ο κανόνας ελάχιστων δικαιωμάτων.
5. Έγκυρο και αξιόπιστο TLS certificate του SQL Server.

## 3. Εγκατάσταση για ανάπτυξη

Από PowerShell εκτέλεσε:

~~~powershell
git clone https://github.com/MoonHard13/Amvrosia-TaxSync.git
cd Amvrosia-TaxSync
dotnet restore AmvrosiaTaxSync.slnx
dotnet build AmvrosiaTaxSync.slnx -c Release
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Desktop
~~~

## 4. Σύνδεση SQL Server

1. Συμπλήρωσε το πεδίο **Server / Instance** (π.χ. \`localhost\` ή \`SERVER\INSTANCE\`).
2. Συμπλήρωσε την **Database** (το πραγματικό όνομα βάσης).
3. Προαιρετικά όρισε **Port**. Η τιμή \`0\` χρησιμοποιεί τις προεπιλογές SQL Server.
4. Χρησιμοποίησε **Windows Authentication**, εφόσον έχεις κατάλληλα δικαιώματα.
5. Για SQL Authentication, απενεργοποίησε το Windows Authentication και συμπλήρωσε SQL Username και SQL Password.
6. Πάτησε **Έλεγχος σύνδεσης (Ctrl+T)**.
7. Έλεγξε την ένδειξη κατάστασης για επιτυχία ή σφάλμα.

{% hint style="info" %}
Η εφαρμογή αποθηκεύει μόνο μη ευαίσθητες ρυθμίσεις. Ο SQL κωδικός δεν αποθηκεύεται σε preferences ή logs.
{% endhint %}

## 5. Επιθεώρηση Schema

1. Βεβαιώσου ότι τα στοιχεία σύνδεσης είναι σωστά.
2. Πάτησε **Επιθεώρηση schema (Ctrl+R)**.
3. Στο αριστερό πλαίσιο εμφανίζονται οι διαθέσιμοι πίνακες και views.
4. Επίλεξε αντικείμενο για να δεις ονόματα και τύπους πεδίων.
5. Οι στήλες **PK** και **FK** υποδεικνύουν συμμετοχή σε primary/foreign keys.
6. Πάτησε **Ακύρωση (Esc)** κατά τη διάρκεια λειτουργίας εφόσον χρειάζεται.

Η επιθεώρηση γίνεται αποκλειστικά με SQL metadata SELECT queries.

## 6. Συντομεύσεις

| Συντόμευση | Λειτουργία |
| --- | --- |
| Ctrl+T | Έλεγχος σύνδεσης |
| Ctrl+R | Επιθεώρηση Schema |
| Ctrl+F | Εστίαση στο πεδίο Server |
| Esc | Ακύρωση ενεργής διαδικασίας |

## 7. CLI / CMD

Εκτέλεσε από τον φάκελο του repository:

~~~powershell
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- test --server localhost --database master
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- scan --server localhost --database master
~~~

Για SQL Authentication:

~~~powershell
$env:AMVROSIA_SQL_PASSWORD = Read-Host "SQL password"
dotnet run --project src/MoonHard.AmvrosiaTaxSync.Cli -- test --server SERVER --database DB --auth sql --user USER
Remove-Item Env:AMVROSIA_SQL_PASSWORD
~~~

{% hint style="warning" %}
Η μεταβλητή περιβάλλοντος παραμένει προσβάσιμη στο τρέχον process context. Μην χρησιμοποιείς κοινόχρηστες συνεδρίες ή καταγραφή PowerShell history με κωδικούς. Η εντολή Read-Host παραπάνω απαιτεί προσοχή: προτίμησε ασφαλή μέθοδο εισαγωγής μυστικών στο δικό σου περιβάλλον.
{% endhint %}

Exit codes CLI: **0** επιτυχία, **1** σφάλμα, **2** λάθος παράμετροι, **3** ακύρωση.

## 8. Αρχεία ρυθμίσεων και logs

- Ρυθμίσεις: \`%LOCALAPPDATA%\MoonHard\AmvrosiaTaxSync\settings.json\`
- Logs: \`%LOCALAPPDATA%\MoonHard\AmvrosiaTaxSync\Logs\taxsync-YYYY-MM-DD.jsonl\`

Τα logs περιλαμβάνουν χρόνο, επίπεδο, κατηγορία και τεχνικό μήνυμα. Δεν περιλαμβάνουν SQL passwords ή ολοκληρωμένα connection strings.

## 9. Αντιμετώπιση προβλημάτων

| Σύμπτωμα | Έλεγχος |
| --- | --- |
| Login failed | Επιβεβαίωσε authentication και SQL permissions. |
| Server not found | Έλεγξε server name, instance, port και firewall. |
| Certificate error | Ρύθμισε σωστό SQL Server certificate και trust chain. Μην απενεργοποιείς certificate validation. |
| Κενή λίστα αντικειμένων | Έλεγξε επιλογή database και δικαιώματα metadata. |
| Timeout | Έλεγξε δικτύωση, SQL απόκριση και όριο timeout. |

## 10. Ασφάλεια, backup και επόμενες εκδόσεις

- Ξεκίνα με SQL user μόνο ανάγνωσης.
- Κράτησε κανονικά SQL backups μέσω της τυπικής διαδικασίας του οργανισμού.
- Μην δίνεις production credentials σε issues, screenshots ή source files.
- Η προσθήκη field mappings, ΑΑΔΕ και εγκεκριμένων ενημερώσεων θα γίνει σε ξεχωριστά Pull Requests μετά την επιβεβαίωση πραγματικού schema και επίσημων κανόνων σύνδεσης.
- Για έκδοση Windows executable: \`dotnet publish src/MoonHard.AmvrosiaTaxSync.Desktop -c Release -r win-x64 --self-contained true\`. Η εκτέλεση σε καθαρό Windows υπολογιστή χρειάζεται ξεχωριστή επιβεβαίωση.

{% hint style="info" %}
Το παρόν manual καλύπτει την έκδοση foundation v0.1.0. Η εγκατάσταση installer και η αυτοματοποίηση updates δεν είναι διαθέσιμες ακόμη.
{% endhint %}
