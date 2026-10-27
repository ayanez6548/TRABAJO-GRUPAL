# Práctica 07: Validación

Validar formato, datos requeridos, saldo del cliente y existencias de productos.

## Implementación

Las reglas de ventas se alojan en NorthWind.Sales.Validators.Entities sin depender de frameworks. FluentValidation se usa para membresía detrás de IModelValidator. ModelValidatorHub ejecuta primero validaciones estructurales y evita consultar SQL si fallan. Los mismos validadores de DTO se usan en Blazor. Se validan cantidades agregadas por producto, no cada línea de manera aislada.

## Archivos principales

- [ModelValidators.cs](../src/4.%20Frameworks%20And%20Drivers/Libraries/NorthWind.ValidationService.FluentValidation/ModelValidators.cs)
- [CreateOrderBusinessValidators.cs](../src/2.%20Application%20Business%20Rules/NorthWind.Sales.Backend.UseCases/CreateOrder/CreateOrderBusinessValidators.cs)

## Comprobación

Prueba cliente ANTON, cliente ZZZZZ, producto 2, producto 99, cantidades negativas y filas repetidas que superen el stock. Todos deben producir errores sin crear órdenes.

La solución es acumulativa: esta funcionalidad forma parte del estado final de las 16 guías.

Validador compartido de ventas: [CreateOrderDtoValidator.cs](../src/1.%20Enterprise%20Business%20Rules/NorthWind.Sales.Validators.Entities/CreateOrderDtoValidator.cs).
