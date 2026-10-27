# Práctica 13: Autenticación y autorización en el dominio

Exigir un usuario autenticado y registrar quién crea la orden.

## Implementación

IUserService abstrae el usuario HTTP. El Interactor verifica autenticación antes de validar o persistir, y asigna CreatedBy desde la identidad. DomainLog también registra al usuario.

## Archivos principales

- [UserService.cs](../src/4.%20Frameworks%20And%20Drivers/Libraries/NorthWind.UserServices/UserService.cs)
- [CreateOrderInteractor.cs](../src/2.%20Application%20Business%20Rules/NorthWind.Sales.Backend.UseCases/CreateOrder/CreateOrderInteractor.cs)

## Comprobación

Sin token, POST /CreateOrder devuelve 401. Verifica CreatedBy en Orders y UserName en DomainLogs tras crear una orden autenticada.

La solución es acumulativa: esta funcionalidad forma parte del estado final de las 16 guías.
