using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoonHard.AmvrosiaTaxSync.Infrastructure.SqlServer.Diagnostics;

namespace MoonHard.AmvrosiaTaxSync.Tests;

[TestClass]
public sealed class SqlConnectionErrorFormatterTests
{
    [DataTestMethod]
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
    public void UntrustedCertificateAuthority_Code_ProvidesSpecificGuidance()
    {
        var connectionId = Guid.NewGuid();
        const string unsafeRawMessage = "Password=secret-value; Data Source=private-server";

        var result = SqlConnectionErrorFormatter.Classify(
            -2146893019, state: 0, severity: 20,
            connectionId: connectionId, providerMessage: unsafeRawMessage);

        StringAssert.Contains(result.Title, "πιστοποιητικού");
        StringAssert.Contains(result.Suggestion, "CA");
        StringAssert.Contains(result.Suggestion, "SAN");
        StringAssert.Contains(result.TechnicalDetails, "-2146893019");
        StringAssert.Contains(result.TechnicalDetails, connectionId.ToString("D"));
        Assert.IsFalse(result.Suggestion.Contains("secret-value", StringComparison.Ordinal));
        Assert.IsFalse(result.TechnicalDetails.Contains("private-server", StringComparison.Ordinal));
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
