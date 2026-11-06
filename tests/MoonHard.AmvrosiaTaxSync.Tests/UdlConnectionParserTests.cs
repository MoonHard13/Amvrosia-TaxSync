using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoonHard.AmvrosiaTaxSync.Application.Services;

namespace MoonHard.AmvrosiaTaxSync.Tests;

[TestClass]
public sealed class UdlConnectionParserTests
{
    private const string Header = "[oledb]\n; Everything after this line is an OLE DB initstring\n";

    [TestMethod]
    public void SqlAuthentication_ImportsFieldsAndPassword()
    {
        var input = Header + "Provider=SQLOLEDB.1;Data Source=sql01,1433;Initial Catalog=ERP;" +
            "User ID=worker;Password=\"secret;value\";Persist Security Info=False;";
        var result = UdlConnectionParser.Parse(input);

        Assert.AreEqual("sql01", result.Options.Server);
        Assert.AreEqual(1433, result.Options.Port);
        Assert.AreEqual("ERP", result.Options.Database);
        Assert.IsFalse(result.Options.WindowsAuthentication);
        Assert.AreEqual("worker", result.Options.Username);
        Assert.AreEqual("secret;value", result.Options.Password);
        Assert.IsTrue(result.ContainsPassword);
    }

    [TestMethod]
    public void WindowsAuthentication_DoesNotRequirePassword()
    {
        var result = UdlConnectionParser.Parse(Header +
            "Provider=MSOLEDBSQL19;Data Source=server\\instance;Initial Catalog=Amvrosia;Integrated Security=SSPI;");
        Assert.IsTrue(result.Options.WindowsAuthentication);
        Assert.AreEqual(@"server\instance", result.Options.Server);
        Assert.IsFalse(result.ContainsPassword);
    }

    [TestMethod]
    public void MissingSqlPassword_AllowsManualEntry()
    {
        var result = UdlConnectionParser.Parse(Header +
            "Provider=SQLNCLI11;Data Source=sqlserver;Initial Catalog=ERP;User ID=operator;");
        Assert.IsFalse(result.Options.WindowsAuthentication);
        Assert.IsNull(result.Options.Password);
    }

    [TestMethod]
    public void NonSqlProvider_IsRejected()
        => Assert.ThrowsExactly<NotSupportedException>(() =>
            UdlConnectionParser.Parse(Header +
                "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=file.accdb;Initial Catalog=ERP;"));

    [TestMethod]
    public void InvalidFileHeader_IsRejected()
        => Assert.ThrowsExactly<FormatException>(() =>
            UdlConnectionParser.Parse("Provider=SQLOLEDB;Data Source=server;Initial Catalog=ERP;"));

    [TestMethod]
    public async Task UnicodeUdlFile_IsReadCorrectly()
    {
        var file = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.udl");
        try
        {
            await File.WriteAllTextAsync(file, Header +
                "Provider=SQLOLEDB.1;Data Source=server;Initial Catalog=Μαγαζί;Integrated Security=SSPI;",
                Encoding.Unicode);
            var result = await new UdlConnectionFileReader().LoadAsync(file);
            Assert.AreEqual("Μαγαζί", result.Options.Database);
        }
        finally
        {
            File.Delete(file);
        }
    }

    [TestMethod]
    public async Task NonUdlExtension_IsRejected()
        => await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
            await new UdlConnectionFileReader().LoadAsync("connection.txt"));
}
