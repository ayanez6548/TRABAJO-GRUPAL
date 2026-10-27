using NorthWind.Sales.Validators.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthWind.Result.Entities;
using NorthWind.Validation.Entities;
using NorthWind.Membership.Backend.Core.Interfaces;
using NorthWind.Membership.Backend.Core.UseCases;
using NorthWind.Membership.Entities.Dtos;
using NorthWind.Sales.Backend.Presenters.Membership;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using NorthWind.ValidationService.FluentValidation;
namespace NorthWind.Tests;

[TestClass]
public sealed class PracticeConformanceTests
{
    [TestMethod]
    [DataRow("CustomerId", 4, false)]
    [DataRow("CustomerId", 5, true)]
    [DataRow("CustomerId", 6, false)]
    [DataRow("ShipAddress", 0, false)]
    [DataRow("ShipAddress", 1, true)]
    [DataRow("ShipAddress", 60, true)]
    [DataRow("ShipAddress", 61, false)]
    [DataRow("ShipCity", 2, false)]
    [DataRow("ShipCity", 3, true)]
    [DataRow("ShipCity", 15, true)]
    [DataRow("ShipCity", 16, false)]
    [DataRow("ShipCountry", 2, false)]
    [DataRow("ShipCountry", 3, true)]
    [DataRow("ShipCountry", 15, true)]
    [DataRow("ShipCountry", 16, false)]
    [DataRow("ShipPostalCode", 0, true)]
    [DataRow("ShipPostalCode", 10, true)]
    [DataRow("ShipPostalCode", 11, false)]
    public async Task Practice07_EnforcesDocumentedLengthBoundaries(string property, int length, bool valid)
    {
        string value = new(property == "ShipPostalCode" ? '1' : 'A', length);
        var dto = property switch
        {
            "CustomerId" => OrderTests.Valid() with { CustomerId = value },
            "ShipAddress" => OrderTests.Valid() with { ShipAddress = value },
            "ShipCity" => OrderTests.Valid() with { ShipCity = value },
            "ShipCountry" => OrderTests.Valid() with { ShipCountry = value },
            "ShipPostalCode" => OrderTests.Valid() with { ShipPostalCode = value },
            _ => throw new ArgumentException(property)
        };
        IModelValidator<CreateOrderDto> validator = new CreateOrderDtoValidator();
        var errors = await validator.Validate(dto);
        Assert.AreEqual(valid, errors.Count == 0);
        if (!valid) Assert.IsTrue(errors.Any(e => e.PropertyName == property));
    }

    [TestMethod]
    public async Task Practice07_RejectsNullDetailWithoutAnUnexpectedException()
    {
        IModelValidator<CreateOrderDto> validator = new CreateOrderDtoValidator();
        var errors = await validator.Validate(OrderTests.Valid() with { OrderDetails = [null!] });
        Assert.IsTrue(errors.Any(e => e.PropertyName == "OrderDetails[0]"));
    }

    [TestMethod]
    [DataRow("Aa1!aa", true)]
    [DataRow("Aa1!a", false)]
    [DataRow("AA1!AA", false)]
    [DataRow("aa1!aa", false)]
    [DataRow("AaA!aa", false)]
    [DataRow("Aa11aa", false)]
    [DataRow("", false)]
    public async Task Practice14_EnforcesAllPasswordRequirements(string password, bool valid)
    {
        IModelValidator<UserRegistrationDto> validator = new UserRegistrationDtoValidator();
        var errors = await validator.Validate(new("usuario@example.test", password, "Ana", "Pérez"));
        Assert.AreEqual(valid, errors.Count == 0);
        if (!valid) Assert.IsTrue(errors.Any(e => e.PropertyName == "Password"));
    }

    [TestMethod]
    public async Task Practice14_RegistrationUsesPortsAndResultAndSkipsPersistenceForInvalidInput()
    {
        var service = new MembershipStub();
        var presenter = new UserRegistrationPresenter();
        IUserRegistrationInputPort port = new UserRegistrationInteractor(service, presenter,
            new ModelValidatorHub<UserRegistrationDto>([new UserRegistrationDtoValidator()]));
        await port.Handle(new("correo inválido", "123", "", ""));
        Assert.IsTrue(presenter.Result!.HasError);
        Assert.AreEqual(0, service.RegisterCalls);
        await port.Handle(new("ana@example.test", "Aa1!aa", "Ana", "Pérez"));
        Assert.IsFalse(presenter.Result!.HasError);
        Assert.AreEqual("user-id", presenter.Result.Value);
        Assert.AreEqual(1, service.RegisterCalls);
    }

    [TestMethod]
    public async Task Practice15_OnlySuccessfulCredentialsReachTokenGenerationInPresenter()
    {
        var membership = new MembershipStub();
        var tokens = new TokensStub();
        var presenter = new UserLoginPresenter(tokens);
        IUserLoginInputPort port = new UserLoginInteractor(membership, presenter,
            new ModelValidatorHub<UserCredentialsDto>([new UserCredentialsDtoValidator()]));
        await port.Handle(new("ana@example.test", "Incorrecta1!"));
        Assert.IsTrue(presenter.Result!.HasError);
        Assert.AreEqual(0, tokens.Calls);
        membership.Authenticate = true;
        await port.Handle(new("ana@example.test", "Aa1!aa"));
        Assert.IsFalse(presenter.Result!.HasError);
        Assert.AreEqual("signed-token", presenter.Result.Value!.AccessToken);
        Assert.AreEqual(1, tokens.Calls);
        await port.Handle(new("correo inválido", ""));
        Assert.IsTrue(presenter.Result!.HasError);
        Assert.AreEqual(1, tokens.Calls);
    }

    private sealed class MembershipStub : IMembershipService
    {
        public int RegisterCalls { get; private set; }
        public bool Authenticate { get; set; }
        public Task<Result<string>> Register(UserRegistrationDto dto, CancellationToken cancellationToken = default)
        {
            RegisterCalls++;
            return Task.FromResult(Result<string>.Success("user-id"));
        }
        public Task<UserDto?> GetUserByCredentials(UserCredentialsDto dto, CancellationToken cancellationToken = default) =>
            Task.FromResult<UserDto?>(Authenticate ? new("user-id", dto.Email, "Ana", "Pérez") : null);
    }
    private sealed class TokensStub : ITokenService
    {
        public int Calls { get; private set; }
        public TokensDto CreateToken(UserDto user) { Calls++; return new("signed-token", DateTimeOffset.UtcNow.AddMinutes(5)); }
    }
}
