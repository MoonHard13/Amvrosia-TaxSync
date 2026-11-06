using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoonHard.AmvrosiaTaxSync.Infrastructure.SqlServer.Diagnostics;

namespace MoonHard.AmvrosiaTaxSync.Tests;

[TestClass]
public sealed class SqlConnectionErrorFormatterTests
{
    [TestMethod]
    [DataRow(18456, "login")]
    [DataRow(4060, "βάση")]
    [DataRow(53, "επικοινωνία")]
    [DataRow(-2, "χρονικό")]
    [DataRow(233, "έκλεισε")]
    public void SqlCodes_ProvideActionableClassification(int code, string expected)
    {
        var diagnosis = SqlConnectionErrorFormatter.Classify(code);
        StringAssert.Contains(diagnosis.Title, expected);
    }

    [TestMethod]
    public void TlsFailure_IsRecognizedWithoutLeakingProviderMessage()
    {
        const string message = "SSL Provider: certificate validation failed; Password=private-value;";
        var result = SqlConnectionErrorFormatter.Classify(0, 1, 20, Guid.NewGuid(), message);

        StringAssert.Contains(result.Title, "TLS");
        Assert.IsFalse(result.TechnicalDetails.Contains("private-value", StringComparison.Ordinal));
        Assert.IsFalse(result.Suggestion.Contains("private-value", StringComparison.Ordinal));
    }

    [TestMethod]
    public void UnknownCode_IncludesSafeTechnicalNumbers()
    {
        var id = Guid.NewGuid();
        var diagnosis = SqlConnectionErrorFormatter.Classify(123456, 7, 12, id, "Password=hidden");

        StringAssert.Contains(diagnosis.TechnicalDetails, "123456");
        StringAssert.Contains(diagnosis.TechnicalDetails, "State: 7");
        StringAssert.Contains(diagnosis.TechnicalDetails, id.ToString("D"));
        Assert.IsFalse(diagnosis.TechnicalDetails.Contains("hidden", StringComparison.Ordinal));
    }
}
