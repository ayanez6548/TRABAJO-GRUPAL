# Práctica 10: Registro de sucesos

Persistir auditoría del inicio y resultado de la creación de órdenes.

## Implementación

DomainLogger depende de IDomainLogsRepository y realiza Add y SaveChanges, siguiendo el patrón Repository y Unit Of Work de la guía. DomainLogsRepository transforma el registro al modelo de datos y el contexto EF Core realiza la persistencia. Se guardan fecha UTC, descripción y usuario en DomainLogs. ILogger se usa para diagnósticos técnicos. El registro de inicio persiste antes de la transacción; el de creación participa en ella.

## Archivos principales

- [DomainLogs.cs](../src/1.%20Enterprise%20Business%20Rules/NorthWind.DomainLogs.Entities/DomainLogs.cs)
- [DomainLogger.cs](../src/2.%20Application%20Business%20Rules/NorthWind.Sales.Backend.UseCases/Common/DomainLogger.cs)
- [DomainLogsRepository.cs](../src/3.%20Interface%20Adapters/Gateways/Repositories/NorthWind.Sales.Backend.Repositories/Repositories/DomainLogsRepository.cs)

## Comprobación

Consulta SELECT * FROM DomainLogs ORDER BY Id. Una orden correcta registra inicio y creación, con el correo del usuario autenticado.

La solución es acumulativa: esta funcionalidad forma parte del estado final de las 16 guías.
