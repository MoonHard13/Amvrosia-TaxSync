using System.IO;
using System.Text;

namespace MoonHard.AmvrosiaTaxSync.Application.Services;

public sealed class UdlConnectionFileReader
{
    private const int MaxFileSize = 1_048_576;

    public async Task<UdlImportResult> LoadAsync(
        string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            !string.Equals(Path.GetExtension(path), ".udl", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Επίλεξε αρχείο με κατάληξη .udl.");

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
            FileShare.Read, bufferSize: 4096, useAsync: true);

        if (stream.Length is <= 0 or > MaxFileSize)
            throw new FormatException("Το UDL πρέπει να είναι μη κενό και μικρότερο από 1 MB.");

        var bytes = new byte[(int)stream.Length];
        await stream.ReadExactlyAsync(bytes, cancellationToken);

        Encoding encoding;
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            encoding = Encoding.Unicode;
        else if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            encoding = Encoding.BigEndianUnicode;
        else if (bytes.Length >= 2 && bytes[1] == 0)
            encoding = Encoding.Unicode;
        else
            encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        return UdlConnectionParser.Parse(encoding.GetString(bytes));
    }
}
