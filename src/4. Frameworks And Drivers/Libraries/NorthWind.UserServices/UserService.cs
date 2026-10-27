using Microsoft.AspNetCore.Http;
using NorthWind.Entities.Interfaces;
namespace NorthWind.UserServices;
public sealed class UserService(IHttpContextAccessor accessor) : IUserService
{
    public bool IsAuthenticated => accessor.HttpContext?.User.Identity?.IsAuthenticated == true;
    public string UserName => accessor.HttpContext?.User.Identity?.Name ?? "";
    public string FullName => accessor.HttpContext?.User.FindFirst("FullName")?.Value ?? UserName;
}
