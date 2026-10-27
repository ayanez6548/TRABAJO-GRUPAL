# Práctica 05: Adaptadores de interfaz

Implementar Repository, Presenter y Controller.

## Implementación

CommandsRepository transforma el agregado al modelo persistente. QueriesRepository encapsula consultas. CreateOrderPresenter devuelve el ID. EndpointsContainer comunica HTTP con los puertos.

## Archivos principales

- [CommandsRepository.cs](../src/3.%20Interface%20Adapters/Gateways/Repositories/NorthWind.Sales.Backend.Repositories/Repositories/CommandsRepository.cs)
- [CreateOrderPresenter.cs](../src/3.%20Interface%20Adapters/Presenters/NorthWind.Sales.Backend.Presenters/CreateOrder/CreateOrderPresenter.cs)
- [EndpointsContainer.cs](../src/3.%20Interface%20Adapters/Controllers/NorthWind.Sales.Backend.Controllers/EndpointsContainer.cs)

## Comprobación

Crea una orden en Swagger: el resultado HTTP 200 contiene el ID entero de la orden.

La solución es acumulativa: esta funcionalidad forma parte del estado final de las 16 guías.
