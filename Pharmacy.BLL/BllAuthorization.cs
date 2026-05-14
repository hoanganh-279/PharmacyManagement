using Pharmacy.Common;

namespace Pharmacy.BLL;

internal static class BllAuthorization
{
    public static void RequireAuthenticated()
    {
        if (!UserSession.IsAuthenticated)
            throw new InvalidOperationException("Chưa đăng nhập.");
    }

    public static void RequireAnyRole(params string[] allowed)
    {
        RequireAuthenticated();
        var role = UserSession.TenVaiTro;
        if (role is null || !allowed.Contains(role, StringComparer.Ordinal))
            throw new UnauthorizedAccessException("Không có quyền thực hiện thao tác này.");
    }
}
