using System.Net.Http.Headers;

namespace GestionObras.Web.Services;

/// <summary>
/// Reenvia la cookie de autenticacion del usuario actual hacia la API
/// para que ambos hosts puedan compartir el contexto de identidad.
/// </summary>
public sealed class ApiAuthCookieHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiAuthCookieHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var cookieHeader = _httpContextAccessor.HttpContext?.Request.Headers.Cookie.ToString();
        if (!string.IsNullOrWhiteSpace(cookieHeader) && !request.Headers.Contains("Cookie"))
        {
            request.Headers.Add("Cookie", cookieHeader);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
