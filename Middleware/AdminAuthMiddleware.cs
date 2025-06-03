using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;

public class AdminAuthMiddleware
{
    private readonly RequestDelegate _next;

    public AdminAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path;

        // Kiểm tra nếu đường dẫn bắt đầu bằng /admin
        if (path.StartsWithSegments("/admin", out var remainingPath))
        {
            // Cho phép truy cập /admin/login chính xác, không bao gồm các subpath như /admin/login/reset
            if (!path.Equals("/admin/login", StringComparison.OrdinalIgnoreCase))
            {
                if (!context.User.Identity.IsAuthenticated || !context.User.IsInRole("Admin"))
                {
                    context.Response.Redirect("/admin/login");
                    return;
                }
            }
        }

        await _next(context);
    }
}
