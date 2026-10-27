# Previous-semester requirements

The user-supplied requirements supersede the original practice behavior when they differ. In particular, repeated products are rejected, inventory is deducted, only positive-stock products are listed, and sales invoices include VAT. Application identifiers, classes, methods, properties, routes and API payload names are in English. User-facing UI, validation feedback, email notifications and invoice PDFs are in Spanish, as clarified by the user on 21 October 2026. Technical audit records retain English text. Existing customer names and addresses remain business data.

| No. | Requirement | Implementation |
|---|---|---|
| 1 | Normalized entity-relationship model | Customers, Products, Orders and OrderDetails; relational foreign keys and composite line key. See DATA-MODEL.md. |
| 2 | Create a complete sales order | Shipping fields, customer, current product prices and quantities; transactional persistence. |
| 3 | One occurrence per product | UI prevents duplicate selection; DTO validator and aggregate reject duplicates; database composite primary key. |
| 4 | Subtotal, discount, VAT and total | InvoiceDto and CreateOrderViewModel use decimal arithmetic and cent rounding. VAT rate is saved with the order. |
| 5 | Edit only necessary fields | Customer/product selection, shipping and quantity are editable. Prices, stock, IDs of saved orders and calculated amounts are read-only. Server verifies prices again. |
| 6 | Validate input | Required values, numeric quantity/price limits, letters in names/cities/countries, address and postal-code formats, email and password rules. Customer IDs are five-character NorthWind business codes, not national identity numbers; no national-ID field is collected. |
| 7 | Automatic inventory reduction | Conditional stock update inside the same transaction as header, details and success audit. |
| 8 | Show only available products | Both catalog listing and paginated product search filter UnitsInStock > 0. |
| 9 | Prevent overselling | Early validation plus an atomic conditional update; a concurrent stock or price change returns HTTP 409. |
| 10 | Remove a draft product | Remove button edits the draft before persistence. |
| 11 | Change a draft product without losing other data | Change button opens the lookup; quantity and other order lines are preserved. |
| 12 | Reconstruct and print/download an invoice | Protected invoice page, saved header/detail JSON, server-generated PDF, embedded viewer, Download PDF and Print PDF controls. Users can access their own invoices. |
| 13 | Usability | Visible labels, responsive tables, read-only calculated fields, status feedback, modal search, pagination and keyboard Escape. |
| 14 | Custom errors | Shared validation errors, ProblemDetails with trace ID, stock conflict, authentication errors and transaction completion/failure messages. |
| 15 | Modal lookup by at least two fields, with paging | Customer ID/name and product ID/name; server-side Skip/Take and bounded page sizes. |
| 16 | Clean code and English | Four architectural layers, ports, repositories, dependency injection and English identifiers; Spanish user-facing text and reusable DTOs. |
| 17 | Transactions and concurrency | Stock updates acquire write locks until commit; conditional stock/price predicates detect conflicts. Failed orders roll back stock, header, details and success audit. |
| 18 | No catalog CRUD required | No customer/product maintenance screens or public write endpoints were added. Sample records are seeded; additional fixtures can be inserted with SQL. |
| 19 | Layered programming | The existing distribution of 31 projects is preserved. Invoice PDF generation stays in infrastructure; application logic depends on interfaces. |

## Calculation policy

The academic configuration uses a 10% discount and **15% example VAT**, applied to the discounted subtotal. Change `SalesSettings:VatRate` to match the exercise's required rate. This configuration is not a claim about the legally applicable tax rate. The PDF is an academic sales receipt, without tax-authority electronic-invoice integration.

Prices are checked against the database while stock is reserved. A client cannot reduce the catalog price by modifying the JSON request. Historical unit prices and the order's VAT/discount rates reconstruct the invoice without using today's catalog price.

The special-order notification threshold from the practices remains: more than three distinct products. SMTP failure cancels the transaction and restores inventory. Sending an email is external to the database transaction, as documented in the original practices.
