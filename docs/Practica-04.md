# Práctica 04: Reglas de negocio de la aplicación

Definir enumeraciones, POCOs, Value Objects, agregados, repositorios y puertos.

## Implementación

OrderAggregate agrupa productos repetidos y suma cantidades. La orden usa transporte terrestre, descuento porcentual de 10 y fecha UTC de creación. CreateOrderInteractor coordina el caso de uso.

## Archivos principales

- [OrderAggregate.cs](../src/2.%20Application%20Business%20Rules/NorthWind.Sales.Backend.BusinessObjects/Aggregates/OrderAggregate.cs)
- [CreateOrderInteractor.cs](../src/2.%20Application%20Business%20Rules/NorthWind.Sales.Backend.UseCases/CreateOrder/CreateOrderInteractor.cs)

## Comprobación

Ejecuta OrderTests. Al enviar dos líneas del mismo producto, se guarda una sola con la suma de cantidades. Si los precios difieren, prevalece el último, igual que en el PDF.

La solución es acumulativa: esta funcionalidad forma parte del estado final de las 16 guías.
