> Historical .NET 8 baseline. Superseded by [NET10-VERIFICATION.md](NET10-VERIFICATION.md) and [SEMESTER-REQUIREMENTS.md](SEMESTER-REQUIREMENTS.md).

# Revisión final de las prácticas 01 a 16

Revisión funcional: 16 de octubre de 2026. Distribución de proyectos actualizada: 20 de octubre de 2026.

La solución ahora contiene **31 proyectos**, con las cuatro capas y subcarpetas de las capturas. La reorganización pasó la compilación y 54 pruebas, incluida integración con SQL. Consulta [ESTRUCTURA.md](../ESTRUCTURA.md). Las evidencias HTTP/SMTP de la revisión funcional conservan su fecha original.

## Resultado y alcance

Se compararon los 16 documentos con el código y se comprobaron compilación, pruebas automatizadas, persistencia SQL y endpoints reales. Las funcionalidades principales del caso de uso están implementadas y las comprobaciones ejecutadas pasan. La solución conserva .NET 8 y C# 12.

**No es una reproducción literal de cada archivo, proyecto y firma de método de los PDF.** Se mantiene la decisión de usar una única solución y una única base `NorthWindPracticas8`. Las otras diferencias de implementación se detallan más abajo para que no se confunda equivalencia funcional con una copia exacta del ejercicio.

## Matriz de revisión

| Práctica | Requisito contrastado | Evidencia en esta solución | Resultado |
|---|---|---|---|
| 01 | Regla de dependencia y cuatro capas | Carpetas de solución y ArchitectureTests; no hay referencias desde las capas internas hacia las externas | Estructura comprobada |
| 02 | Crear orden con cliente, envío y productos; devolver número de orden | CreateOrderDto, CreateOrderInteractor y endpoint /CreateOrder; prueba HTTP de creación | Flujo comprobado |
| 03 | DTOs de entrada sin lógica de negocio | CreateOrderDto y CreateOrderDetailDto son registros de datos inmutables | Código comprobado |
| 04 | ShippingType, DiscountType, Order, OrderDetail, agregado, puertos y Unit Of Work | OrderAggregate agrupa productos, suma cantidades, conserva último precio, aplica Road, Percentage, 10 y fecha de creación | Reglas comprobadas |
| 05 | Repositorio, presentador, controlador e inyección de dependencias | CommandsRepository, QueriesRepository, CreateOrderPresenter, EndpointsContainer e IoC | Flujo HTTP y dependencias comprobados |
| 06 | EF Core, SQL Server, migraciones, Swagger y Blazor WebAssembly | NorthWindContext, migración InitialCreate y ambos hosts net8.0 | Compilación, esquema, API y recursos del cliente comprobados |
| 07 | Datos requeridos, longitudes, cliente existente y sin deuda, productos existentes y stock suficiente | ModelValidatorHub, validadores compartidos de ventas, adaptador FluentValidation de membresía y validadores de negocio; 18 pruebas de límites, casos de entradas nulas y pruebas HTTP con errores por campo | Reglas comprobadas; framework de validación simplificado |
| 08 | Excepciones propias, IExceptionHandler y ProblemDetails; recepción de errores en el cliente | ApiExceptionHandler; pruebas 400/401/500, contenido errors y ausencia de detalles SQL; ExceptionDelegatingHandler | Contrato HTTP comprobado |
| 09 | Evento de orden especial y notificación al administrador para más de 3 productos | SpecialOrderCreatedEvent, manejador y MailService; orden especial 15 aceptada mediante SMTP Mailtrap | Publicación, plantilla y aceptación SMTP comprobadas |
| 10 | Auditoría del inicio y creación; repositorio y Unit Of Work | IDomainLogsRepository, DomainLogsRepository y DomainLogger; registros persistidos con fecha y usuario | Persistencia y abstracciones comprobadas |
| 11 | Transacción del caso de uso y rollback si falla el correo; registrar cancelación | IDomainTransaction; prueba SQL confirma ausencia de cabecera, detalles y auditoría de éxito de la orden cancelada | Atomicidad comprobada; se usa transacción EF en una base |
| 12 | Specification para determinar una orden especial | Specification<T> y SpecialOrderSpecification: más de 3 productos distintos después de agruparlos | Umbral comprobado |
| 13 | Solo usuarios autenticados; guardar usuario creador y autor de auditoría | IUserService, guardia del Interactor, CreatedBy y DomainLog.UserName; pruebas de rechazo previo a cualquier escritura | Autenticación y autoría comprobadas |
| 14 | Registro con Identity, Result, validación, puertos y presentador | IUserRegistrationInputPort/OutputPort, UserRegistrationInteractor/Presenter, UserManager, índice de correo y formulario | Registro, contraseña débil y correo duplicado comprobados |
| 15 | Login, credenciales y emisión de JWT a través del presentador | IUserLoginInputPort/OutputPort, UserLoginInteractor/Presenter, ITokenService y JwtService | Credenciales correctas e incorrectas y emisión de token comprobadas |
| 16 | JWT Bearer, Swagger Authorize y sesión de Blazor | Validación de firma, emisor, audiencia y vencimiento; localStorage, AuthenticationStateProvider, AuthorizeRouteView y bearer handler | Protección HTTP y JWT comprobados; composición del cliente revisada |

## Correcciones realizadas durante esta revisión

1. **Errores de validación incompletos.** La respuesta se serializaba como ProblemDetails y podía omitir la propiedad Errors del subtipo. Ahora `errors` se añade como extensión explícita. Se comprobó tanto con una prueba de regresión como con peticiones HTTP: llegan `OrderDetails[0].Quantity` y `OrderDetails[0].ProductId` completos.
2. **Puertos y presentadores de membresía.** Registro y login ahora tienen InputPort y OutputPort propios. Los controladores dependen de estos contratos. El Interactor de login entrega el usuario al presentador y este solicita la generación del token, como plantea la práctica 15.
3. **Repositorio de auditoría.** DomainLogger dejó de depender de EF Core. Depende de IDomainLogsRepository y coordina Add y SaveChanges. El adaptador y el contexto se encargan de la persistencia, según el patrón de la práctica 10.
4. **Errores de persistencia.** UpdateException devuelve HTTP 500, según la práctica 8. Una notificación fallida también devuelve 500 y conserva el rollback. Antes se usaban 409 y 503 respectivamente.
5. **Ubicación del error de producto.** Los validadores contra SQL ahora identifican la fila y propiedad afectadas, siguiendo el formato de la práctica 7.

No se modificaron el esquema ni los datos existentes mediante migraciones durante estas correcciones. Las pruebas añaden usuarios, órdenes y registros de auditoría de prueba.

## Diferencias que se mantienen de forma explícita

- **Base de datos única.** Los PDF separan ventas, auditoría y membresía en distintos contextos/bases. Aquí comparten `NorthWindPracticas8`, conforme a la decisión tomada para el proyecto. Se conservan las 13 tablas necesarias para el esquema actual, incluidas Identity y el historial de migraciones.
- **Transacciones.** La práctica 11 ejemplifica TransactionScope/MSDTC; esta solución utiliza IDbContextTransaction sobre la conexión compartida. Se verificó el mismo comportamiento de confirmación y cancelación de orden y auditoría, pero el mecanismo no es idéntico.
- **Organización y empaquetado.** La solución contiene 31 proyectos locales distribuidos como en las capturas: 11 de entidades, 4 de aplicación, 8 adaptadores y 8 de infraestructura. Eventos, transacciones, validación, Result y auditoría tienen sus propios proyectos. Las pruebas se conservan aparte en tests/NorthWind.Tests. No se requieren publicaciones NuGet privadas NorthWind.*.
- **Framework de validación.** Se preservan el contrato IModelValidator, el hub, el orden de validación y el aislamiento de FluentValidation. No se reproduce la API completa IValidationService/IValidationRules y sus clases auxiliares paso a paso; las reglas de ventas están en NorthWind.Sales.Validators.Entities y las reglas de membresía usan el adaptador NorthWind.ValidationService.FluentValidation.
- **Presentación y errores.** Los presentadores de membresía devuelven Result tipado y la traducción a HTTP queda en el controlador; las guías muestran IResult dentro de esos presentadores. El manejo de excepciones está centralizado en un IExceptionHandler que agrupa los casos, en lugar de tres clases separadas. `errors` usa el diccionario estándar por propiedad en vez de la lista de ValidationError mostrada en algunos ejemplos. Los mensajes se mantienen como cadenas en C# en lugar de archivos .resx.
- **Interfaz del cliente.** Se conserva el formulario de orden, registro, login, navegación protegida, cierre de sesión y visualización de errores. La composición y el diseño no copian literalmente las pantallas o todos los componentes auxiliares de las guías.
- **Tipos y controles adicionales.** Se usan DateTimeOffset en UTC, decimal para descuentos, controles de precisión monetaria y desbordamiento de cantidades. El formato de JWT usa `name` configurado expresamente como NameClaimType. Se añaden vencimiento en el cliente, bloqueo de intentos y protección de rutas de catálogo.
- **Reglas que no se amplían.** Se comprueba stock, pero no se descuenta inventario ni se agregan reservas, CRUD de catálogos, refresh tokens o recuperación de contraseña. El correo conserva el asunto y la frase mínima de la práctica 9; no se añadió una factura HTML.

Estas diferencias deben considerarse si la evaluación exige nombres y archivos idénticos a los PDF. El resultado de esta revisión acredita el funcionamiento probado y documenta las equivalencias; no certifica identidad literal con todos los fragmentos de las guías.

## Comprobaciones ejecutadas

- Compilación de la solución: **0 errores, 0 advertencias**.
- Suite automatizada: **52 pruebas aprobadas, 0 fallidas, 0 omitidas** con conexión SQL configurada.
- HTTP: **14 casos aprobados** y **2 comprobaciones adicionales del contenido de errors**.
- SQL: una sola base de usuario (`NorthWindPracticas8`), 13 tablas, migración aplicada, órdenes y auditoría persistidas.
- SMTP: la creación de la orden especial **15** terminó en HTTP 200 después de que MailService completara el envío a Mailtrap. Eso acredita aceptación SMTP; esta revisión no certifica la visualización del mensaje en la interfaz web de Mailtrap.
- Swagger: verificación de disponibilidad, esquema Bearer y endpoints.
- Blazor: compilación del cliente y revisión de registro de servicios, rutas, almacenamiento del token, estado de autenticación y manejo de respuestas. La prueba visual de navegación, recarga y cierre de sesión se realizó en la verificación inicial; no se presenta como una nueva prueba visual de esta revisión.

Las pruebas usan identidades de prueba únicas. La integración de rollback emplea un servicio de correo controlado que falla intencionalmente y no envía mensajes externos. La prueba HTTP sí envía una notificación al sandbox configurado.

Evidencias locales reproducibles:

- `artifacts/RevisionFinal/RevisionFinal.trx`: resultado de la suite.
- `artifacts/RevisionFinal/Http-Smoke.log`: comprobaciones HTTP.
- `tools/Smoke-Test.ps1`: script para repetir el flujo HTTP.

## Repetir la comprobación

Desde la raíz de NorthWind:

```powershell
dotnet build NorthWind.sln
$env:NORTHWIND_TEST_CONNECTION = 'Server=(localdb)\MSSQLLocalDB;Database=NorthWindPracticas8;Trusted_Connection=True;TrustServerCertificate=True'
dotnet test tests/NorthWind.Tests/NorthWind.Tests.csproj
# Con la API iniciada:
./tools/Smoke-Test.ps1
```

La API se reinició durante la revisión. Si el cliente conserva un token anterior y recibe 401, vuelve a iniciar sesión.

## Documentos fuente contrastados

- [001-Introduccion_unlocked.pdf](../../001-Introduccion_unlocked.pdf)
- [002-Practica-(Definición del caso de uso)_unlocked.pdf](../../002-Practica-%28Definici%C3%B3n%20del%20caso%20de%20uso%29_unlocked.pdf)
- [003-Practica-(Implementando la capa de Reglas de negocio Empresariales)_unlocked.pdf](../../003-Practica-%28Implementando%20la%20capa%20de%20Reglas%20de%20negocio%20Empresariales%29_unlocked.pdf)
- [004-Practica-(Implementando la capa de Reglas de negocio de la aplicacion)_unlocked.pdf](../../004-Practica-%28Implementando%20la%20capa%20de%20Reglas%20de%20negocio%20de%20la%20aplicacion%29_unlocked.pdf)
- [005-Practica-(Implementando la capa de Adaptadores de Interfaz)_unlocked.pdf](../../005-Practica-%28Implementando%20la%20capa%20de%20Adaptadores%20de%20Interfaz%29_unlocked.pdf)
- [006-Practica-(Implementando la capa Frameworks And Drivers)_unlocked.pdf](../../006-Practica-%28Implementando%20la%20capa%20Frameworks%20And%20Drivers%29_unlocked.pdf)
- [007-Practica-(Implementando Validacion)_unlocked.pdf](../../007-Practica-%28Implementando%20Validacion%29_unlocked.pdf)
- [008-Practica-(Manejo de Excepciones)_unlocked.pdf](../../008-Practica-%28Manejo%20de%20Excepciones%29_unlocked.pdf)
- [009-Practica-(Eventos de Dominio)_unlocked.pdf](../../009-Practica-%28Eventos%20de%20Dominio%29_unlocked.pdf)
- [010-Practica-(Registro de Sucesos)_unlocked.pdf](../../010-Practica-%28Registro%20de%20Sucesos%29_unlocked.pdf)
- [011-Practica-(Manejo de Transacciones)_unlocked.pdf](../../011-Practica-%28Manejo%20de%20Transacciones%29_unlocked.pdf)
- [012-Practica-(Patron Specification)_unlocked.pdf](../../012-Practica-%28Patron%20Specification%29_unlocked.pdf)
- [013-Practica-(Autenticacion y Autorizacion)_unlocked.pdf](../../013-Practica-%28Autenticacion%20y%20Autorizacion%29_unlocked.pdf)
- [014-Practica-(Registro de Usuarios)_unlocked.pdf](../../014-Practica-%28Registro%20de%20Usuarios%29_unlocked.pdf)
- [015-Practica-(Inicio de Sesion-Login)_unlocked.pdf](../../015-Practica-%28Inicio%20de%20Sesion-Login%29_unlocked.pdf)
- [016-Practica-(Configuracion de Autenticacion y Autorizacion)_unlocked.pdf](../../016-Practica-%28Configuracion%20de%20Autenticacion%20y%20Autorizacion%29_unlocked.pdf)
