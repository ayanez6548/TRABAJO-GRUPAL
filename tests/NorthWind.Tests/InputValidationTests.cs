using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthWind.Membership.Entities.Dtos;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using NorthWind.Sales.Validators.Entities;
using NorthWind.Validation.Entities;
using NorthWind.ValidationService.FluentValidation;

namespace NorthWind.Tests;

[TestClass]
public sealed class InputValidationTests
{
    [TestMethod]
    [DataRow("María José", "Muñoz", true)]
    [DataRow("Mari\u0301a", "Pen\u0303a", true)]
    [DataRow("Ana1", "Pérez", false)]
    [DataRow("Ana", "Pérez2", false)]
    [DataRow("Ana!", "Pérez", false)]
    [DataRow("   ", "Pérez", false)]
    public async Task Registration_AcceptsUnicodeLettersButRejectsNumbersAndPunctuation(string first, string last, bool valid)
    {
        IModelValidator<UserRegistrationDto> validator = new UserRegistrationDtoValidator();
        var errors = await validator.Validate(new("ana@example.test", "Clave123!", first, last));
        Assert.AreEqual(valid, errors.Count == 0);
    }

    [TestMethod]
    public async Task Registration_EnforcesFormLimitsWithoutRejectingBoundaryValues()
    {
        IModelValidator<UserRegistrationDto> validator = new UserRegistrationDtoValidator();
        var valid = new UserRegistrationDto("ana@example.test", "Aa1!" + new string('x', 26), new string('A', 30), new string('B', 30));
        Assert.IsEmpty(await validator.Validate(valid));
        var errors = await validator.Validate(valid with { FirstName = valid.FirstName + "A", LastName = valid.LastName + "B" });
        Assert.IsTrue(errors.Any(e => e.PropertyName == "FirstName"));
        Assert.IsTrue(errors.Any(e => e.PropertyName == "LastName"));
    }

    [TestMethod]
    public async Task RegistrationAndLogin_AcceptLongPasswordsAndStillRequireAValue()
    {
        IModelValidator<UserCredentialsDto> login = new UserCredentialsDtoValidator();
        IModelValidator<UserRegistrationDto> registration = new UserRegistrationDtoValidator();
        foreach (var length in new[] { 31, 64, 128, 1024 })
        {
            var password = "Aa1!" + new string('x', length - 4);
            Assert.IsEmpty(await login.Validate(new("ana@example.test", password)), $"Login: {length} characters");
            Assert.IsEmpty(await registration.Validate(new("ana@example.test", password, "Ana", "Pérez")), $"Registration: {length} characters");
        }
        Assert.IsTrue((await login.Validate(new("ana@example.test", ""))).Any(error => error.PropertyName == "Password"));
        Assert.IsTrue((await registration.Validate(new("ana@example.test", "", "Ana", "Pérez"))).Any(error => error.PropertyName == "Password"));
    }

    [TestMethod]
    [DataRow("ana@example.test", true)]
    [DataRow("ana+ventas@example.com", true)]
    [DataRow("A._-+01@sub.example-domain.com", true)]
    [DataRow("ana!ventas@example.com", false)]
    [DataRow("ana#ventas@example.com", false)]
    [DataRow("ana%ventas@example.com", false)]
    [DataRow("ana,ventas@example.com", false)]
    [DataRow("ana;ventas@example.com", false)]
    [DataRow("\"ana\"@example.com", false)]
    [DataRow("aña@example.com", false)]
    [DataRow(".ana@example.com", false)]
    [DataRow("ana.@example.com", false)]
    [DataRow("ana..ventas@example.com", false)]
    [DataRow("ana@my_domain.com", false)]
    [DataRow("ana@example+ventas.com", false)]
    [DataRow("ana@-example.com", false)]
    [DataRow("ana@example-.com", false)]
    [DataRow("ana@example..com", false)]
    [DataRow("ana@@example.com", false)]
    [DataRow("ana@example", false)]
    [DataRow("ana @example.com", false)]
    [DataRow("Ana <ana@example.com>", false)]
    public async Task LoginAndRegistration_UseTheSameEmailFormat(string email, bool valid)
    {
        IModelValidator<UserRegistrationDto> registration = new UserRegistrationDtoValidator();
        IModelValidator<UserCredentialsDto> login = new UserCredentialsDtoValidator();
        Assert.AreEqual(valid, !(await registration.Validate(new(email, "Clave123!", "Ana", "Pérez"))).Any(e => e.PropertyName == "Email"));
        Assert.AreEqual(valid, !(await login.Validate(new(email, "Clave123!"))).Any(e => e.PropertyName == "Email"));
    }

    [TestMethod]
    public async Task Email_EnforcesAccountLengthInBothForms()
    {
        var valid = new string('a', 18) + "@example.com";
        Assert.AreEqual(30, valid.Length);
        IModelValidator<UserRegistrationDto> registration = new UserRegistrationDtoValidator();
        IModelValidator<UserCredentialsDto> login = new UserCredentialsDtoValidator();
        foreach (var email in new[] { valid, "a" + valid, new string('a', 65) + "@example.com", "ana@" + new string('b', 64) + ".com" })
        {
            var expected = email == valid;
            Assert.AreEqual(expected, !(await registration.Validate(new(email, "Clave123!", "Ana", "Pérez"))).Any(e => e.PropertyName == "Email"));
            Assert.AreEqual(expected, !(await login.Validate(new(email, "Clave123!"))).Any(e => e.PropertyName == "Email"));
        }
    }

    [TestMethod]
    [DataRow(null, true)]
    [DataRow("", true)]
    [DataRow("00170101", true)]
    [DataRow("1234567890", true)]
    [DataRow("12345678901", false)]
    [DataRow("1701AB", false)]
    [DataRow("170 101", false)]
    [DataRow("١٧٠١٠١", false)]
    public async Task PostalCode_IsOptionalAndPreservesAsciiDigitsOnly(string? code, bool valid)
    {
        IModelValidator<CreateOrderDto> validator = new CreateOrderDtoValidator();
        Assert.AreEqual(valid, !(await validator.Validate(OrderTests.Valid() with { ShipPostalCode = code })).Any(e => e.PropertyName == "ShipPostalCode"));
    }

    [TestMethod]
    public async Task Shipping_EnforcesLengthsAndRejectsPaddingUsedToBypassMinimumLength()
    {
        IModelValidator<CreateOrderDto> validator = new CreateOrderDtoValidator();
        var valid = OrderTests.Valid() with { ShipAddress = new string('A', 60), ShipCity = new string('B', 15), ShipCountry = new string('C', 15) };
        Assert.IsEmpty(await validator.Validate(valid));
        var errors = await validator.Validate(valid with { ShipAddress = valid.ShipAddress + "A", ShipCity = " Q ", ShipCountry = valid.ShipCountry + "C" });
        Assert.IsTrue(errors.Any(e => e.PropertyName == "ShipAddress"));
        Assert.IsTrue(errors.Any(e => e.PropertyName == "ShipCity"));
        Assert.IsTrue(errors.Any(e => e.PropertyName == "ShipCountry"));
    }
}
