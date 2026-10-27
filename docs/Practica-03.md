# Práctica 03: Reglas de negocio empresariales

Transportar los datos mediante DTOs inmutables.

## Implementación

CreateOrderDto y CreateOrderDetailDto contienen solo datos de entrada. Transporte, fecha y descuento se resuelven dentro del dominio.

## Archivos principales

- [CreateOrderDto.cs](../src/1.%20Enterprise%20Business%20Rules/NorthWind.Sales.Entities/Dtos/CreateOrder/CreateOrderDto.cs)

## Comprobación

Comprueba que el JSON de creación no permita establecer CreatedBy, descuento ni fecha.

La solución es acumulativa: esta funcionalidad forma parte del estado final de las 16 guías.
