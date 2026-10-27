using NorthWind.Sales.Entities.Dtos;

namespace NorthWind.Sales.Backend.BusinessObjects.Interfaces;

public interface IInvoicePdfExporter { byte[] Export(InvoiceDto invoice); }
public interface ISalesSettings { decimal VatRate { get; } }
