# Práctica 14: Registro de usuarios

Crear cuentas con Identity y devolver errores esperados mediante Result.

## Implementación

UserRegistrationInteractor implementa IUserRegistrationInputPort, valida el DTO y delega en IMembershipService. Devuelve el resultado por IUserRegistrationOutputPort, implementado por UserRegistrationPresenter. MembershipService usa UserManager y almacena hashes mediante ASP.NET Core Identity. Result<T> contiene valor o errores. El cliente incluye confirmación de contraseña.

## Archivos principales

- [MembershipInteractors.cs](../src/2.%20Application%20Business%20Rules/NorthWind.Membership.Backend.Core/UseCases/MembershipInteractors.cs)
- [MembershipPorts.cs](../src/2.%20Application%20Business%20Rules/NorthWind.Membership.Backend.Core/Interfaces/MembershipPorts.cs)
- [MembershipPresenters.cs](../src/3.%20Interface%20Adapters/Presenters/NorthWind.Sales.Backend.Presenters/Membership/MembershipPresenters.cs)
- [MembershipService.cs](../src/4.%20Frameworks%20And%20Drivers/Frameworks/NorthWind.Membership.Backend.AspNetIdentity/Services/MembershipService.cs)
- [UserRegistrationPage.razor](../src/3.%20Interface%20Adapters/Views/NorthWind.Membership.Frontend.RazorViews/Pages/UserRegistrationPage.razor)

## Comprobación

Registra una cuenta, repite el correo y comprueba el error. Prueba contraseña débil y confirmación distinta. Nunca se guarda la contraseña en texto plano.

La solución es acumulativa: esta funcionalidad forma parte del estado final de las 16 guías.
