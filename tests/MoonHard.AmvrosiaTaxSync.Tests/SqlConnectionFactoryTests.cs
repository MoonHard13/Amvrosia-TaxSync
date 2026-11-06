using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoonHard.AmvrosiaTaxSync.Domain.Models;
using MoonHard.AmvrosiaTaxSync.Infrastructure.SqlServer.SqlServer;

namespace MoonHard.AmvrosiaTaxSync.Tests;

[TestClass]
public sealed class SqlConnectionFactoryTests
{
    [TestMethod]
    public void DefaultConnection_RequiresEncryptionAndTrustsCertificate()
    {
        var options = new SqlConnectionOptions("server", "database", true, null, null);
        var builder = new SqlConnectionStringBuilder(
            SqlConnectionFactory.BuildConnectionString(options));

        Assert.AreEqual(SqlConnectionEncryptOption.Mandatory, builder.Encrypt);
        Assert.IsTrue(builder.TrustServerCertificate);
        Assert.IsTrue(builder.IntegratedSecurity);
    }

    [TestMethod]
    public void StrictCertificateValidation_CanBeRequested()
    {
        var options = new SqlConnectionOptions("server", "database", true, null, null)
            with { TrustServerCertificate = false };
        var builder = new SqlConnectionStringBuilder(
            SqlConnectionFactory.BuildConnectionString(options));

        Assert.IsFalse(builder.TrustServerCertificate);
        Assert.AreEqual(SqlConnectionEncryptOption.Mandatory, builder.Encrypt);
    }

    [TestMethod]
    public void CustomPort_UsesSqlServerCommaSyntax()
    {
        var options = new SqlConnectionOptions("server", "database", true, null, null, 1444);
        var builder = new SqlConnectionStringBuilder(
            SqlConnectionFactory.BuildConnectionString(options));

        Assert.AreEqual("server,1444", builder.DataSource);
    }
}
