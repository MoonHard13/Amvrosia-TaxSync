using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoonHard.AmvrosiaTaxSync.Application.Services;
using MoonHard.AmvrosiaTaxSync.Domain.Models;

namespace MoonHard.AmvrosiaTaxSync.Tests;

[TestClass]
public sealed class GreekAfmValidatorTests
{
    [TestMethod]
    public void ValidNineDigitAfm_ReturnsValid()
    {
        Assert.AreEqual(AfmStatus.Valid, GreekAfmValidator.Check("123456783"));
        Assert.AreEqual(AfmStatus.Valid, GreekAfmValidator.Check("094259216"));
    }

    [TestMethod]
    public void BlankAfm_ReturnsMissing()
    {
        Assert.AreEqual(AfmStatus.Missing, GreekAfmValidator.Check(""));
        Assert.AreEqual(AfmStatus.Missing, GreekAfmValidator.Check("  "));
        Assert.AreEqual(AfmStatus.Missing, GreekAfmValidator.Check(null));
    }

    [TestMethod]
    public void InvalidAfm_ReturnsInvalid()
    {
        Assert.AreEqual(AfmStatus.Invalid, GreekAfmValidator.Check("123456789"));
        Assert.AreEqual(AfmStatus.Invalid, GreekAfmValidator.Check("000000000"));
        Assert.AreEqual(AfmStatus.Invalid, GreekAfmValidator.Check("ABC456783"));
        Assert.AreEqual(AfmStatus.Invalid, GreekAfmValidator.Check("12345678"));
    }
}
