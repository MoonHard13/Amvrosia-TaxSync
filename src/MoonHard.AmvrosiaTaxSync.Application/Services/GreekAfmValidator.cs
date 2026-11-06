using MoonHard.AmvrosiaTaxSync.Domain.Models;

namespace MoonHard.AmvrosiaTaxSync.Application.Services;

public static class GreekAfmValidator
{
    public static AfmStatus Check(string? rawAfm)
    {
        var afm = rawAfm?.Trim();
        if (string.IsNullOrEmpty(afm))
            return AfmStatus.Missing;
        if (afm.Length != 9 || afm.Any(ch => ch is < '0' or > '9') || afm.All(ch => ch == '0'))
            return AfmStatus.Invalid;

        var weightedSum = 0;
        for (var index = 0; index < 8; index++)
            weightedSum += (afm[index] - '0') * (1 << (8 - index));

        return weightedSum % 11 % 10 == afm[8] - '0'
            ? AfmStatus.Valid
            : AfmStatus.Invalid;
    }
}
