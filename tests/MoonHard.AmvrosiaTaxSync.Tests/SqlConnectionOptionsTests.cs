using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoonHard.AmvrosiaTaxSync.Domain.Models;

namespace MoonHard.AmvrosiaTaxSync.Tests;

[TestClass]
public sealed class SqlConnectionOptionsTests
{
    [TestMethod]
    public void WindowsAuthentication_ValidProfile_DoesNotRequirePassword()
    {
        var settings = new SqlConnectionOptions("localhost", "master", true, null, null);
        settings.Validate();
    }

    [TestMethod]
    public void MissingServer_IsRejected()
    {
        var settings = new SqlConnectionOptions("", "master", true, null, null);
        Assert.ThrowsExactly<ArgumentException>(settings.Validate);
    }

    [TestMethod]
    public void SqlAuthentication_WithoutPassword_IsRejected()
    {
        var settings = new SqlConnectionOptions("localhost", "master", false, "user", null);
        Assert.ThrowsExactly<ArgumentException>(settings.Validate);
    }

    [TestMethod]
    public void NamedInstance_WithExplicitPort_IsRejected()
    {
        var settings = new SqlConnectionOptions(@"server\instance", "master", true, null, null, 1433);
        Assert.ThrowsExactly<ArgumentException>(settings.Validate);
    }

    [TestMethod]
    public void InvalidTimeout_IsRejected()
    {
        var settings = new SqlConnectionOptions("localhost", "master", true, null, null, 0, 0);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(settings.Validate);
    }
}
