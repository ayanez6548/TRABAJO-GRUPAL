# Práctica 11: Manejo de transacciones

Cancelar la orden y la auditoría de éxito si falla cualquier paso, incluida la notificación.

## Implementación

IDomainTransaction se implementa con una transacción explícita EF Core en una única base de datos. Ante una falla se hace rollback, se limpia el seguimiento y se escribe la cancelación fuera de la transacción. El Presenter se invoca solo después del commit.

## Archivos principales

- [DomainTransaction.cs](../src/4.%20Frameworks%20And%20Drivers/Frameworks/NorthWind.Sales.Backend.DataContexts.EFCore/Services/DomainTransaction.cs)
- [CreateOrderInteractor.cs](../src/2.%20Application%20Business%20Rules/NorthWind.Sales.Backend.UseCases/CreateOrder/CreateOrderInteractor.cs)

## Comprobación

Ejecuta UseCaseTests. Para probar SQL, configura un servidor SMTP inaccesible y crea una orden especial: la orden y sus detalles no deben persistir; la auditoría conserva inicio y cancelación. Un envío SMTP ya realizado no puede deshacerse por rollback; una solución productiva puede usar outbox.

La solución es acumulativa: esta funcionalidad forma parte del estado final de las 16 guías.
