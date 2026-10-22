using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Localization;
using Microsoft.JSInterop;

namespace CommunityLink.App.Services;

/// <summary>
/// Scoped service that manages the user's active culture in the Blazor circuit.
/// Enables seamless, in-place culture switching without requiring a full browser page reload.
/// Persists the user's choice to the .AspNetCore.Culture cookie via JS interop for subsequent visits.
/// </summary>
public class CultureService
{
    private readonly IJSRuntime? _js;
    private CultureInfo _currentCulture;
    private CultureInfo _currentUICulture;

    public event Action? OnCultureChanged;

    public CultureInfo CurrentCulture => _currentCulture;
    public CultureInfo CurrentUICulture => _currentUICulture;
    public string CurrentCultureName => _currentUICulture.TwoLetterISOLanguageName;
    public bool IsMyanmar => string.Equals(CurrentCultureName, "my", StringComparison.OrdinalIgnoreCase);

    public CultureService(IJSRuntime? js = null)
    {
        _js = js;

        // Initialize from the ambient UICulture established by UseRequestLocalization on initial HTTP request
        var initialTwoLetter = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var canonical = CultureEndpoints.TryNormalizeCulture(initialTwoLetter, out var norm)
            ? norm
            : CultureEndpoints.DefaultCulture;

        _currentCulture = new CultureInfo(canonical);
        _currentUICulture = new CultureInfo(canonical);
    }

    /// <summary>
    /// Changes the active culture in-memory for the current circuit, updates ambient thread cultures,
    /// persists the standard ASP.NET Core culture cookie via JavaScript, and notifies all UI subscribers
    /// so the component tree re-renders in-place without a page reload.
    /// </summary>
    public async Task SetCultureAsync(string cultureCode)
    {
        if (!CultureEndpoints.TryNormalizeCulture(cultureCode, out var canonical))
            return;

        if (string.Equals(CurrentCultureName, canonical, StringComparison.OrdinalIgnoreCase))
            return;

        var newCulture = new CultureInfo(canonical);
        _currentCulture = newCulture;
        _currentUICulture = newCulture;

        // Update ambient thread execution context
        CultureInfo.CurrentCulture = newCulture;
        CultureInfo.CurrentUICulture = newCulture;
        CultureInfo.DefaultThreadCurrentCulture = newCulture;
        CultureInfo.DefaultThreadCurrentUICulture = newCulture;

        // Persist cookie and update html lang attribute in browser without reloading the page
        if (_js != null)
        {
            try
            {
                var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(canonical, canonical));
                var script = $"document.cookie = '{CookieRequestCultureProvider.DefaultCookieName}={Uri.EscapeDataString(cookieValue)};path=/;max-age=31536000;SameSite=Lax'; if (document.documentElement) document.documentElement.lang = '{canonical}';";
                await _js.InvokeVoidAsync("eval", script);
            }
            catch
            {
                // Silently ignore if JS is not available (e.g. during pre-rendering or unit testing)
            }
        }

        // Notify layout and subscribers to trigger in-place re-rendering
        OnCultureChanged?.Invoke();
    }

    /// <summary>
    /// Synchronous culture setter for headless testing or non-JS environments.
    /// </summary>
    public void SetCulture(string cultureCode)
    {
        if (!CultureEndpoints.TryNormalizeCulture(cultureCode, out var canonical))
            return;

        var newCulture = new CultureInfo(canonical);
        _currentCulture = newCulture;
        _currentUICulture = newCulture;

        CultureInfo.CurrentCulture = newCulture;
        CultureInfo.CurrentUICulture = newCulture;
        CultureInfo.DefaultThreadCurrentCulture = newCulture;
        CultureInfo.DefaultThreadCurrentUICulture = newCulture;

        OnCultureChanged?.Invoke();
    }
}
