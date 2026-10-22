using System;
using System.Globalization;

namespace CommunityLink.App.Services;

public class CultureService
{
    private CultureInfo _currentCulture;

    public event Action? OnCultureChanged;

    public CultureService()
    {
        var uiCulture = CultureInfo.CurrentUICulture;
        var canonical = uiCulture.TwoLetterISOLanguageName.Equals("my", StringComparison.OrdinalIgnoreCase)
            ? "my"
            : "en";
        _currentCulture = new CultureInfo(canonical);
    }

    public CultureInfo CurrentCulture => _currentCulture;

    public string CurrentCultureName => _currentCulture.TwoLetterISOLanguageName;

    public bool IsMyanmar => string.Equals(_currentCulture.TwoLetterISOLanguageName, "my", StringComparison.OrdinalIgnoreCase);

    public void SetCulture(string cultureName)
    {
        var canonical = cultureName.StartsWith("my", StringComparison.OrdinalIgnoreCase) ? "my" : "en";
        if (string.Equals(_currentCulture.TwoLetterISOLanguageName, canonical, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var culture = new CultureInfo(canonical);
        _currentCulture = culture;

        // Update thread and async context culture
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        OnCultureChanged?.Invoke();
    }
}
