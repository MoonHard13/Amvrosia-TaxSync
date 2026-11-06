using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoonHard.AmvrosiaTaxSync.Domain.Models;

namespace MoonHard.AmvrosiaTaxSync.Tests;

[TestClass]
public sealed class CustomerQueryTests
{
    [TestMethod]
    public void ValidQuery_IsAccepted()
    {
        new CustomerQuery("ΔΟΚΙΜΗ", 2, 100).Validate();
    }

    [TestMethod]
    public void InvalidPage_IsRejected()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => new CustomerQuery("", 0, 100).Validate());
    }

    [TestMethod]
    public void HugePageSize_IsRejected()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => new CustomerQuery("", 1, 501).Validate());
    }

    [TestMethod]
    public void SearchOverLimit_IsRejected()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => new CustomerQuery(new string('x', 101), 1, 100).Validate());
    }

    [TestMethod]
    public void PageCount_UsesCeiling()
    {
        var page = new CustomerPage([], new CustomerTotals(201, 200, 1), 201, 1, 100);
        Assert.AreEqual(3L, page.PageCount);
    }
}
