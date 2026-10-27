# Práctica 15: Inicio de sesión

Verificar credenciales y emitir un token JWT con vencimiento.

## Implementación

UserLoginInteractor implementa IUserLoginInputPort y entrega Result<UserDto> a IUserLoginOutputPort. UserLoginPresenter transforma ese resultado en Result<TokensDto>, usando ITokenService únicamente si las credenciales fueron correctas. JwtService firma con HMAC SHA256 e incluye sub, email, name, FullName, jti y expiración. Los intentos incorrectos tienen bloqueo temporal de Identity.

## Archivos principales

- [JwtService.cs](../src/4.%20Frameworks%20And%20Drivers/Frameworks/NorthWind.Membership.Backend.AspNetIdentity/Services/JwtService.cs)
- [MembershipInteractors.cs](../src/2.%20Application%20Business%20Rules/NorthWind.Membership.Backend.Core/UseCases/MembershipInteractors.cs)
- [MembershipPresenters.cs](../src/3.%20Interface%20Adapters/Presenters/NorthWind.Sales.Backend.Presenters/Membership/MembershipPresenters.cs)
- [UserLoginPage.razor](../src/3.%20Interface%20Adapters/Views/NorthWind.Membership.Frontend.RazorViews/Pages/UserLoginPage.razor)

## Comprobación

POST /user/Login devuelve accessToken y expiresAt con credenciales correctas. Una contraseña incorrecta devuelve 401. No se imprime el token en los logs.

La solución es acumulativa: esta funcionalidad forma parte del estado final de las 16 guías.
