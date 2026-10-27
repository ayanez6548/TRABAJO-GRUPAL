# Práctica 09: Eventos de dominio

Notificar al administrador cuando una orden tiene más de tres productos distintos.

## Implementación

SpecialOrderCreatedEvent se publica mediante IDomainEventHub y su manejador usa IMailService. El modo PickupDirectory escribe archivos .eml en App_Data/mail. Development utiliza el modo Smtp con Mailtrap por el puerto 587 y STARTTLS; sus credenciales se obtienen de los secretos de usuario de .NET.

## Archivos principales

- [SpecialOrderCreatedEvent.cs](../src/2.%20Application%20Business%20Rules/NorthWind.Sales.Backend.BusinessObjects/Events/SpecialOrderCreatedEvent.cs)
- [SendEMailWhenSpecialOrderCreatedEventHandler.cs](../src/2.%20Application%20Business%20Rules/NorthWind.Sales.Backend.UseCases/CreateOrder/SendEMailWhenSpecialOrderCreatedEventHandler.cs)
- [MailService.cs](../src/3.%20Interface%20Adapters/Gateways/EmailGateways/NorthWind.Sales.Backend.SmtpGateways/MailService.cs)

## Comprobación

Crea una orden con productos 1, 3, 4 y 5. En Development, busca el mensaje «Orden especial creada» en la bandeja de Email Sandbox de Mailtrap, con el ID y la cantidad de productos distintos. En modo PickupDirectory se genera un archivo .eml. Cuatro filas del mismo producto no califican como orden especial.

La solución es acumulativa: esta funcionalidad forma parte del estado final de las 16 guías.
