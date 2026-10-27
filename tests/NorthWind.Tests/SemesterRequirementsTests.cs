using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthWind.Exceptions.Entities;
using NorthWind.Sales.Backend.BusinessObjects.Aggregates;
using NorthWind.Sales.Entities.Dtos;
using NorthWind.Sales.Entities.Dtos.CreateOrder;
using NorthWind.Sales.Validators.Entities;
using NorthWind.Validation.Entities;
using NorthWind.Sales.WebApi.Invoices;

namespace NorthWind.Tests;

[TestClass]
public sealed class SemesterRequirementsTests
{
    [TestMethod]
    public async Task DuplicateProducts_AreRejectedByTheValidatorAndTheAggregate()
    {
        var input = OrderTests.Valid(new(1, 35m, 1), new(1, 35m, 2));
        IModelValidator<CreateOrderDto> validator = new CreateOrderDtoValidator();
        Assert.IsTrue((await validator.Validate(input)).Any(e => e.PropertyName == "OrderDetails"));
        Assert.ThrowsExactly<ValidationException>(() => OrderAggregate.From(input));
    }

    [TestMethod]
    public void Invoice_CalculatesDiscountVatAndRoundingFromPersistedValues()
    {
        var invoice = Invoice([new(1, "Sample product", 1.11m, 3)]);
        Assert.AreEqual(3.33m, invoice.Subtotal);
        Assert.AreEqual(0.33m, invoice.DiscountAmount);
        Assert.AreEqual(3m, invoice.TaxableAmount);
        Assert.AreEqual(0.45m, invoice.VatAmount);
        Assert.AreEqual(3.45m, invoice.Total);
        Assert.AreEqual(3m, (invoice with { VatRate = 0m }).Total);
    }

    [TestMethod]
    public void PdfInvoice_GeneratesAnActualPdfDocument()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var bytes = new InvoicePdfExporter().Export(Invoice([new(1, "Sample product", 35m, 2)]));
        Assert.IsGreaterThan(1000, bytes.Length);
        Assert.AreEqual("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [TestMethod]
    public async Task CityAndCountry_RejectNumbersWhileAddressAcceptsHouseNumbers()
    {
        IModelValidator<CreateOrderDto> validator = new CreateOrderDtoValidator();
        var errors = await validator.Validate(OrderTests.Valid() with { ShipCity = "Quito123", ShipCountry = "Ecuador1" });
        Assert.IsTrue(errors.Any(e => e.PropertyName == "ShipCity"));
        Assert.IsTrue(errors.Any(e => e.PropertyName == "ShipCountry"));
        Assert.IsEmpty(await validator.Validate(OrderTests.Valid() with { ShipAddress = "Main Street #12-34" }));
    }

    private static InvoiceDto Invoice(IReadOnlyList<InvoiceLineDto> lines) => new(42, DateTimeOffset.UtcNow,
        "ALFKI", "Sample Customer", "Main Street 123", "Quito", "Ecuador", "170101", "user@example.test", 10m, .15m, lines);
}
